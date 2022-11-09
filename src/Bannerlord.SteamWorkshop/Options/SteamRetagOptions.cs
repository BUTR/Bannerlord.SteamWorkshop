using CommandLine;

using System.Collections.Generic;

namespace Bannerlord.SteamWorkshop.Options
{
    [Verb("steamretag", HelpText = "Get Steam TOTP.")]
    public class SteamRetagOptions
    {
        [Option('a', "appid", Required = true)]
        public uint AppId { get; set; } = default!;

        [Option('f', "fileid", Required = true)]
        public IEnumerable<ulong> FileIds { get; set; } = default!;

        [Option('t', "tag", Required = false)]
        public IEnumerable<string> Tags { get; set; } = default!;

        [Option('d', "description", Required = false)]
        public string Description { get; set; } = default!;
    }
}