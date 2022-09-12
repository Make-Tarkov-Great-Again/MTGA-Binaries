# SIT.Tarkov.Core

## Status - Complete

## Disclaimer

This is by no means designed for cheats or illegally downloading the game. This is purely for educational and game modification purposes. You must buy the game to use this. 
You can obtain it here. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com)

## Discord

You can [join here](https://shorturl.at/abHVW). Please be aware there is a strict NO PIRACY policy to this project. Any piracy talk in the discord is a bannable offence. Do not join unless you own the game!

## Summary

The Stay in Tarkov Core handles almost everything to create the Single Player experience of Escape from Tarkov.
Including but not limited to:
- Turning off BattlEye (lets be honest, it doesn't work anyway BSG, please change to something else!)
- Turning off FileChecker (this is BSG's own checker, this needs to be turned off to allow us to mod the game) - See FileChecker
- Setting up Auto Singleplayer mode (see Menus)
- Fixing bots / AI to shoot each other (see AI)
- Fixing bots / AI to become "PMC" (see AI)
- Fixing "offline" mode to use only the designated online spawn points
- Fixing "offline" mode to save Progression of the character (something I used was in Live in some form!)
- Fixing "offline" mode to save Health of the character (something I used was in Live in some form!)
- Lots more

## Which version of BepInEx is this built for?
Version 5

# How to install BepInEx
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## How to compile? 
- The references for Assembly-CSharp (cleaned via de4dot), Comfort, FilesChecker, UnityEngine will need to be setup to your Tarkov installation
- You will need BepInEx Nuget Feed installed on your PC by running the following command in a terminal. 
```
dotnet new -i BepInEx.Templates --nuget-source https://nuget.bepinex.dev/v3/index.json
```
- Open the .sln with Visual Studio 2022
- Rebuild Solution (This should download and install all nuget packages on compilation)

## Install to Tarkov
BepInEx 5 must be installed and configured first (see How to install BepInEx)
Place the built .dll in the BepInEx plugins folder

## Test in Tarkov
- Browse to where BepInEx is installed within your Tarkov folder
- Open config
- Open BepInEx.cfg
- Change the following setting [Logging.Console] Enabled to True
- Save the config file
- Run Tarkov through a launcher or bat file like this one (replacing the token with your ID)
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=AID062158106353313252ruc -config={"BackendUrl":"https://localhost:7777","Version":"live"}
```
- If BepInEx is working a console should open and display the module "plugin" as started

## Thanks List
- SPT-Aki team
- JET team
- SERVPH for https://github.com/S3RAPH-1M/SERVPH-Mods/tree/main/VisceralRagdolls

## License

>>SOME<< of the original core functionality completed by JustEmuTarkov and SPT-Aki teams. There may be licenses pertaining to them within this source.