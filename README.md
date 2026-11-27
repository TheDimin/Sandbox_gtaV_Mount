# GTA V Enhanced mount for s&box

This Windows mount reads the installed GTA V Enhanced archives and creates s&box resources at runtime. It supplies the world, individual YMAP scenes, archetype prefabs, models, textures, and collision. The repository contains mount code and shaders. Supply your own game installation and local archive keys.

## Build and install on the fork

Install the .NET 10 SDK and build the s&box fork. Set `GTA_V_ENHANCED_DIR` to the folder that contains `GTA5_Enhanced.exe` and `common.rpf`, or use the Rockstar installation found in the Windows registry. The managed reader uses the local [ragekit key files](https://github.com/RatPrez/ragekit/blob/9d4110ef0fce900787aaeccb52b67fa5402ff41b/docs/archives.md#keys) in `%USERPROFILE%/.ragekit/keys`.

```powershell
dotnet build -c Release -p:SboxEngineDir=E:/Projects/sbox-public/game
```

`SboxEngineDir` defaults to `FACEPUNCH_ENGINE`. Set `SboxManagedDir` to use assemblies from a separate build directory. Copy `Sandbox.Mounting.GtaV.dll`, `Assets`, and `licenses` from `bin/Release` to `mounts/gtav` under the fork's game directory. Use the fork's shader builder if its shader format differs from the bundled files.

Start s&box, enable **Grand Theft Auto V Enhanced** in Mounts, and open `mount://gtav/gtav.scene`. Open `mount://gtav/maps/bh1_02_cutscenetrailer.scene` to inspect an individual map.

## Water and destruction

The mount reads GTA water regions, streams nearby `WaterBody` volumes, and owns the distant water mesh and shoreline foam. Near water uses shared swell and fades local waves at tile edges. The mount disables per-tile refraction captures. The engine's water component supplies swimming and buoyancy. The mount uses individual near-water bodies and their wave settings.

The mount uses the engine's prop damage, fragmentation, and explosion physics. Its own presentation component maps GTA cues to s&box effects and sounds, with surface fallbacks and optional `effects/gtav/props/<name>.gtapropfx` overrides. Presentation runs on the process that handles the prop events.


## Benchmark stored geometry

Build both variants against the same fork assemblies. The optimized variant uses the fork's stored-geometry APIs and the friend assembly declarations for `Sandbox.Mounting.GtaV`. Use the same renderer, route, camera, and settings for both runs.

```powershell
dotnet build -c Release -p:SboxEngineDir=E:/Projects/sbox-public/game -p:GtaVUseBorealisGeometry=false -p:OutputPath=bin/benchmark-legacy
dotnet build -c Release -p:SboxEngineDir=E:/Projects/sbox-public/game -p:GtaVUseBorealisGeometry=true -p:OutputPath=bin/benchmark-borealis
```

Borealis is enabled by default. Set `GtaVUseBorealisGeometry=false` to build the decoded-geometry benchmark baseline. The default defines `GTAV_BOREALIS_GEOMETRY` and compiles the stored-geometry path. It selects that path when Borealis GPU rendering is requested or on a dedicated server. Both variants use the same water and destruction code.
