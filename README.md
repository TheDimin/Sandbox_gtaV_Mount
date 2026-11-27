# GTA V Enhanced mount for s&box

This Windows mount reads the installed GTA V Enhanced archives and creates s&box resources at runtime. It supplies the world, individual YMAP scenes, archetype prefabs, models, textures, and collision. The repository contains mount code and shaders. Supply your own game installation and local archive keys.

## Build and install on Facepunch Borealis

Install the .NET 10 SDK and build the `borealis` branch of `Facepunch/sbox-public`. Set `GTA_V_ENHANCED_DIR` to the folder that contains `GTA5_Enhanced.exe` and `common.rpf`, or use the Rockstar installation found in the Windows registry. The managed reader uses the local [ragekit key files](https://github.com/RatPrez/ragekit/blob/9d4110ef0fce900787aaeccb52b67fa5402ff41b/docs/archives.md#keys) in `%USERPROFILE%/.ragekit/keys`.

```powershell
dotnet build -c Release -p:SboxEngineDir=<path-to-sbox-game>
```

`SboxEngineDir` defaults to `FACEPUNCH_ENGINE`. Set `SboxManagedDir` to use assemblies from a separate build directory. Copy `Sandbox.Mounting.GtaV.dll`, `Assets`, and `licenses` from `bin/Release` to `mounts/gtav` under the engine's game directory. Use the engine's shader builder if its shader format differs from the bundled files.

Start s&box, enable **Grand Theft Auto V Enhanced** in Mounts, and open `mount://gtav/gtav.scene`. Open `mount://gtav/maps/bh1_02_cutscenetrailer.scene` to inspect an individual map.

## Water and destruction

The mount reads GTA water regions and creates the nearby water tile surfaces and volumes. It also owns the distant water mesh and shoreline foam. Each nearby tile uses a stock WaterBody for rendering, swimming, and buoyancy. With fork extensions enabled, tiles also use shared swell, fade local waves at their edges, and disable per-tile refraction captures.

The mount uses the engine's prop damage, fragmentation, and explosion physics. Its own presentation component maps GTA cues to s&box effects and sounds, with surface fallbacks and optional `effects/gtav/props/<name>.gtapropfx` overrides. Presentation runs on the process that handles the prop events.


## Benchmark stored geometry

Build both variants against the same fork assemblies. The optimized variant uses the fork's stored-geometry APIs and the friend assembly declarations for `Sandbox.Mounting.GtaV`. Use the same renderer, route, camera, and settings for both runs.

```powershell
dotnet build -c Release -p:SboxEngineDir=<path-to-sbox-game> -p:GtaVUseBorealisGeometry=false -p:GtaVUseForkExtensions=true -p:OutputPath=bin/benchmark-legacy
dotnet build -c Release -p:SboxEngineDir=<path-to-sbox-game> -p:GtaVUseBorealisGeometry=true -p:OutputPath=bin/benchmark-borealis
```

The default mount build uses public engine APIs and decoded geometry. On the fork, set GtaVUseBorealisGeometry=true to compile the stored-geometry path. The mount selects that path when Borealis GPU rendering is active or on a dedicated server. GtaVUseForkExtensions defaults to the same value as GtaVUseBorealisGeometry. Set it explicitly on both benchmark builds to keep the water and prop settings the same.
