using Bannerlord.SteamWorkshop.Options;

using CommandLine;

using Steamworks;
using Steamworks.Ugc;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VdfConverter;

namespace Bannerlord.SteamWorkshop
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var parser = Parser.Default
                .ParseArguments<SteamTOTPOptions, LoginAndPublishOptions, SteamRetagOptions>(args);
            parser = parser
                .WithParsed<LoginAndPublishOptions>(o => LoginAndPublish(o.Login, o.Password, o.TOTP, o.FileId, o.ContentPath, o.Description));
            parser = parser
                .WithParsed<SteamTOTPOptions>(o => Console.Write(SteamTOTP.GenerateAuthCode(o.Secret, o.Time)));
            parser = await parser
                .WithParsedAsync<SteamRetagOptions>(async o =>
                {
                    foreach (var fileId in o.FileIds)
                        await SteamRetag(o.AppId, fileId, o.Tags, o.Description);
                });
            parser = parser
                .WithNotParsed(e => { Console.Write("INVALID COMMAND"); });
        }

        private class Root
        {
            public WorkshopItem workshopitem { get; set; }
        }
        private class WorkshopItem
        {
            public string appid { get; set; }
            public string publishedfileid { get; set; }
            public string contentfolder { get; set; }
            public string changenote { get; set; }
            //public List<string> tags { get; set; }
        }

        private static void LoginAndPublish(string login, string password, string? totp, string fileId, string contentPath, string changelog)
        {
            //var changelogCurrent = GetChangelogEntries(new MemoryStream(Encoding.UTF8.GetBytes(changelog)))
            //    .MaxBy(x => x.Version, new AlphanumComparatorFast());
            //var versions = changelogCurrent?.SupportedGameVersions;

            var serializer = new VdfSerializer();
            var content = serializer.Serialize(new Root
            {
                workshopitem = new WorkshopItem
                {
                    appid = "261550",
                    publishedfileid = fileId,
                    contentfolder = contentPath,
                    changenote = changelog,
                    //tags = versions?.ToList() ?? new List<string>()
                }
            });

            var code = SteamTOTP.GenerateAuthCode(totp, null);
            var file = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.vdf");
            File.WriteAllText(file, content);

            var processStartInfo = new ProcessStartInfo("steamcmd", $@"+login ""{login}"" ""{password}"" ""{code}"" +workshop_build_item ""{file}"" +quit");
            Process? process = default;
            try
            {
                var retryCounter = 0;
                do
                {
                    retryCounter++;
                    process = Process.Start(processStartInfo)!;
                    process.WaitForExit();

                } while (retryCounter < 10 && process.ExitCode != 0);
                if (retryCounter >= 10 && process.ExitCode != 0)
                    throw new Exception("Failed to Publish!");
            }
            finally
            {
                process?.Dispose();
            }

            File.Delete(file);
        }

        private static async Task SteamRetag(uint appId, ulong fileId, IEnumerable<string> tags, string changelog)
        {
            var changelogCurrent = GetChangelogEntries(new MemoryStream(Encoding.UTF8.GetBytes(changelog)))
                .MaxBy(x => x.Version, new AlphanumComparatorFast());
            var versions = changelogCurrent?.SupportedGameVersions;

            SteamClient.Init(appId);
            var ed = new Editor(fileId);
            foreach (var tag in tags.Concat(versions ?? Enumerable.Empty<string>()))
                ed.WithTag(tag);
            await ed.SubmitAsync();
            SteamClient.Shutdown();
        }

        public static IEnumerable<ChangelogEntry> GetChangelogEntries(Stream stream)
        {
            var reader = new PeekingStreamReader(stream);

            while (reader.PeekLine() is { } line)
            {
                if (line.StartsWith("-"))
                {
                    reader.ReadLine();
                    var changelogEntry = ReadChangeLogEntry(reader);
                    if (changelogEntry != null)
                        yield return changelogEntry;
                }
                else
                {
                    reader.ReadLine();
                }
            }
        }
        public static ChangelogEntry? ReadChangeLogEntry(PeekingStreamReader reader)
        {
            var version = string.Empty;
            var supportedGameVersions = Array.Empty<string>();
            var description = string.Empty;

            var builder = new StringBuilder();
            while (reader.PeekLine() is { } line)
            {
                switch (line)
                {
                    case { } when line.StartsWith("Version:"):
                        version = line.Replace("Version:", "").Trim();
                        reader.ReadLine();
                        continue;
                    case { } when line.StartsWith("Game Versions:"):
                        supportedGameVersions = line.Replace("Game Versions:", "").Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        reader.ReadLine();
                        continue;
                    case { } when line.StartsWith("-"):
                        description = builder.ToString().Trim('\r', '\n');
                        return new ChangelogEntry(version, supportedGameVersions, description);
                    default:
                        builder.AppendLine(line);
                        reader.ReadLine();
                        continue;
                }
            }

            return null;
        }
    }
}