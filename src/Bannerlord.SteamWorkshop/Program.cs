﻿using Bannerlord.SteamWorkshop.Options;

using CommandLine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VdfConverter;

namespace Bannerlord.SteamWorkshop
{
    public static class Program
    {
        public static void Main(string[] args) => Parser.Default
                .ParseArguments<SteamTOTPOptions, LoginAndPublishOptions>(args)
                .WithParsed<LoginAndPublishOptions>(o => { LoginAndPublish(o.Login, o.Password, o.TOTP, o.FileId, o.ContentPath, o.Description); })
                .WithParsed<SteamTOTPOptions>(o => { Console.Write(SteamTOTP.GenerateAuthCode(o.Secret, o.Time)); })
                .WithNotParsed(e => { Console.Write("INVALID COMMAND"); });

        private class Item
        {
            public string AppId { get; set; }
            public string PublishedFileId { get; set; }
            public string ContentFolder { get; set; }
            public string Changenote { get; set; }
            public List<string> Tags { get; set; }
        }

        private static void LoginAndPublish(string login, string password, string? totp, string fileId, string contentPath, string changelog)
        {
            var changelogCurrent = GetChangelogEntries(new MemoryStream(Encoding.UTF8.GetBytes(changelog)))
                .OrderByDescending(x => x.Version, new AlphanumComparatorFast())
                .First();
            var versions = changelogCurrent.SupportedGameVersions;

            var serializer = new VdfSerializer();
            var content = serializer.Serialize(new Item
            {
                AppId = "261550",
                PublishedFileId = fileId,
                ContentFolder = contentPath,
                Changenote = changelog,
                Tags = versions.ToList()
            });

            var file = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.vdf");
            File.WriteAllText(file, content);
            Process.Start("steamcmd", $@"+login ""{login}"" ""{password}"" ""{totp}"" +workshop_build_item ""{file}"" +quit").WaitForExit();
            File.Delete(file);
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