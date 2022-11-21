using CommandLine;

using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.SteamWorkshop.Options
{
    [Verb("steamretag", HelpText = "Get Steam TOTP.")]
    public class SteamRetagOptions
    {
        [Option('a', "appid", Required = true)]
        public uint AppId { get; set; } = 0;

        [Option('f', "fileid", Required = true)]
        public IEnumerable<ulong> FileIds { get; set; } = Enumerable.Empty<ulong>();

        [Option('t', "tag", Required = true)]
        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

        [Option('d', "description", Required = false)]
        public string Description { get; set; } = string.Empty;
    }
}