# SIT.Tarkov.Core

## Status - Complete

## Disclaimer

This is by no means designed for cheats or illegally downloading the game. This is purely for educational and game modification purposes.

## Summary

This is a port of the Just Emu Tarkov core functionality to allow the game to run without worrying about BattlEye and its File Checker using BepInEx

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

## License

>>SOME<< of the original core functionality completed by TheMaoci, AppeazeTheCheese and the SPT-Aki team. There may be licenses pertaining to them within this source.