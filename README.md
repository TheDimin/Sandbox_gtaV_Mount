# GTA V Enhanced mount for s&box

This Windows mount reads the map and assets from your GTA V Enhanced installation. Install the .NET 10 SDK and supply your own game files and local archive keys.

## Set the GTA V install path

Set `GTA_V_ENHANCED_DIR` to the folder that contains `GTA5_Enhanced.exe` and `common.rpf`. Run this command in PowerShell. Replace the example path with your install path, then start a new editor process.

```powershell
[Environment]::SetEnvironmentVariable('GTA_V_ENHANCED_DIR', 'C:\Games\Grand Theft Auto V Enhanced', 'User')
```

Without this variable, the mount looks for the Steam or Rockstar installation. Put your local archive keys in `%USERPROFILE%\.ragekit\keys`. See the [ragekit key instructions](https://github.com/RatPrez/ragekit/blob/9d4110ef0fce900787aaeccb52b67fa5402ff41b/docs/archives.md#keys).

## Build and install

For the optimized geometry path, use the [Borealis build on TheDimin/sbox-public master](https://github.com/TheDimin/sbox-public/tree/master). From the engine repository, run `Setup.bat`. Then build the engine with `dotnet run --project engine/Tools/SboxBuild/SboxBuild.csproj -- build --config Developer`. Then run this command from the mount repository. Set `SboxEngineDir` to the fork's `game` folder.

```powershell
dotnet build -c Release "-p:SboxEngineDir=C:\src\sbox-public\game" -p:GtaVUseBorealisGeometry=true
```

`GtaVUseBorealisGeometry=true` defines `GTAV_BOREALIS_GEOMETRY` and enables the fork extensions by default. The mount uses stored geometry when the Borealis GPU renderer is active or on a dedicated server. To build against Facepunch's stock `borealis` branch, omit the geometry option. The default build decodes geometry at runtime and can stream more slowly.

Copy `Sandbox.Mounting.GtaV.dll`, `Assets`, and `licenses` from `bin/Release` into `game/mounts/gtav` in the engine checkout. The fork build also writes a GTA mount DLL to this folder. Replace that DLL with the one you just built.

## Open the map

1. Start `game/sbox-dev.exe` from the engine checkout and open an s&box project.
2. In **Mounts**, enable **Grand Theft Auto V Enhanced** for that project.
3. Open `mount://gtav/gtav.scene` from the Asset Browser. To inspect a single map section, try `mount://gtav/maps/bh1_02_cutscenetrailer.scene`.
