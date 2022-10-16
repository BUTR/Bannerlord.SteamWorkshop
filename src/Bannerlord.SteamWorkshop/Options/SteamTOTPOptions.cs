using CommandLine;

namespace Bannerlord.SteamWorkshop.Options
{
    [Verb("steamtotp", HelpText = "Get Steam TOTP.")]
    public class SteamTOTPOptions
    {
        [Option('t', "time", Required = false)]
        public long? Time { get; set; }

        [Option('s', "secret", Required = true)]
        public string Secret { get; set; } = default!;
    }
}