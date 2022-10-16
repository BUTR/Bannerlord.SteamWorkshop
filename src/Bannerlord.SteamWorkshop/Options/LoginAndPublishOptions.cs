using CommandLine;

namespace Bannerlord.SteamWorkshop.Options
{
    [Verb("loginandpublish", HelpText = "Login to Steam and publish content.")]
    public class LoginAndPublishOptions
    {
        // string login, string password, string totp, string fileId, string contentPath, string description
        [Option('l', "login", Required = true)]
        public string Login { get; set; } = default!;

        [Option('p', "password", Required = true)]
        public string Password { get; set; } = default!;

        [Option('t', "totp", Required = false)]
        public string? TOTP { get; set; }

        [Option('f', "fileid", Required = true)]
        public string FileId { get; set; } = default!;

        [Option('c', "contentpath", Required = true)]
        public string ContentPath { get; set; } = default!;

        [Option('d', "description", Required = true)]
        public string Description { get; set; } = default!;
    }
}