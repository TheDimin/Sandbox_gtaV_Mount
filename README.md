# GTA V Enhanced mount for s&box

This Windows mount reads the installed GTA V Enhanced archives and creates s&box resources at runtime. It supplies the world, individual YMAP scenes, archetype prefabs, models, textures, and collision. The repository contains mount code and shaders. Supply your own game installation and local archive keys.

## Build

Install the .NET 10 SDK and s&box. Set `GTA_V_ENHANCED_DIR` to the directory that contains `GTA5_Enhanced.exe` and `common.rpf`, or use the Rockstar installation found in the Windows registry. The managed reader uses the local [ragekit key files](https://github.com/RatPrez/ragekit/blob/9d4110ef0fce900787aaeccb52b67fa5402ff41b/docs/archives.md#keys) in `%USERPROFILE%/.ragekit/keys`.

```powershell
dotnet build -c Release '-p:SboxEngineDir=C:/Program Files (x86)/Steam/steamapps/common/sbox'
```

`SboxEngineDir` defaults to `FACEPUNCH_ENGINE`. The default build uses the public mesh builders. The output includes the mount DLL, shaders compiled with the stock compiler, and license notices. Run the target engine's shader builder again if a different engine version needs a different shader format.

## Install

Use one mount directory for the target engine. Copy the output's `Assets` and `licenses` directories into it.

| Mount loader | Directory relative to the engine | DLL filename |
| --- | --- | --- |
| Stock Steam build with the `mount` directory loader | `mount/gtav` | `gtav.dll` |
| Loader with assembly metadata discovery under `mounts` | `mounts/gtav` | `Sandbox.Mounting.GtaV.dll` |

Rename the DLL file only for the first loader. On Windows the stock loader finds the `Assets` folder with either letter case. Start s&box, enable **Grand Theft Auto V Enhanced** in Mounts, and open `mount://gtav/gtav.scene`. Open `mount://gtav/maps/bh1_02_cutscenetrailer.scene` to inspect an individual map.

## Water and destruction

The mount reads GTA water regions, streams nearby stock `WaterBody` volumes, and owns the distant water mesh and shoreline foam. Near water uses opaque surfaces to avoid a refraction capture for every streamed tile. Stock water provides swimming and buoyancy. The fork's shared near-water batch, edge fade, and shared swell are outside this stock path.

The mount retains stock prop damage, fragmentation, and explosion physics. Its own presentation component maps GTA cues to s&box effects and sounds, with surface fallbacks and optional `effects/gtav/props/<name>.gtapropfx` overrides. Presentation runs on the process that handles the prop events. This port has no separate network broadcast for those effects.


## Benchmark stored geometry

Build both variants against the same compatible fork. The optimized variant needs the fork's stored-geometry APIs and the friend assembly declarations for `Sandbox.Mounting.GtaV`. Use the same renderer, route, camera, and settings for both runs.

```powershell
dotnet build -c Release -p:SboxEngineDir=E:/Projects/sbox-public/game -p:GtaVUseBorealisGeometry=false -p:OutputPath=bin/benchmark-legacy
dotnet build -c Release -p:SboxEngineDir=E:/Projects/sbox-public/game -p:GtaVUseBorealisGeometry=true -p:OutputPath=bin/benchmark-borealis
```

The second command defines `GTAV_BOREALIS_GEOMETRY` and compiles the stored-geometry path. It selects that path when Borealis GPU rendering is requested or on a dedicated server. Both variants use the same water and destruction code.
