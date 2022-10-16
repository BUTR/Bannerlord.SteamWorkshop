namespace Bannerlord.SteamWorkshop
{
    public record ChangelogEntry(string Version, string[] SupportedGameVersions, string Description)
    {
        public string GetFullDescription() => $@"For {string.Join('/', SupportedGameVersions)}
{Description}";
    }
}