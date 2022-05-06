# SIT.Tarkov.Core

## Status - Complete

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

Original Just Emu Tarkov core functionality completed by TheMaoci and AppeazeTheCheese. 

As the original license is GNU GENERAL PUBLIC LICENSE v2 the license for this piece of software remains the same.
All work derived from this work must contain creditation to this repo and the two afromentionted orignal authors.
