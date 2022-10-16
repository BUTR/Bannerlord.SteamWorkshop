# Bannerlord.SteamWorkshop
Simple tool that can login to Steam and upload content mods for Bannerlord.

### Requirements
Requires BUTR GitHub Package Registry to be added.
```shell
nuget sources add -name "BUTR" -Source https://nuget.pkg.github.com/BUTR/index.json -Username YOURGITHUBUSERNAME -Password YOURGITHUBTOKEN
```

### Installation
```shell
dotnet tool install -g Bannerlord.SteamWorkshop
```

### Example
When installed as a global tool:
```shell
bannerlord_steam_workshop loginandpublish -l login -p password -t XXXXX -f 1234 -c "$PWD/Modules/Module" -d test

bannerlord_steam_workshop steamtotp -s "sdfsdfgdshdh"
```