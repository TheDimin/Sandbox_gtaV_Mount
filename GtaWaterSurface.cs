using System;
using Sandbox;

namespace GtaV;

/// <summary>Creates the render surface and physics volume for a streamed water tile.</summary>
internal static class GtaWaterSurface
{
	internal static GameObject Create( GameObject parent, Material material, BBox bounds, float height, float heading )
	{
		const float units = PacketReader.UnitsPerMetre;
		var coastal = bounds.Center.z == 0;
		var swell = coastal ? 0.12f * units : 0;
		var obj = new GameObject( parent, false, coastal ? "Coastal water" : "Inland water" );
		obj.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.NotNetworked;
		obj.NetworkMode = NetworkMode.Never;
		obj.WorldPosition = bounds.Center * units;

		var water = obj.Components.Create<WaterBody>();
		if ( material is not null ) water.Material = material;
		water.Waves = height > 0 || swell > 0;
		water.WaveDirection = heading;
		water.WaveLength = 28f * units;
		water.Size = new Vector2( bounds.Size.x, bounds.Size.y ) * units;
		water.Depth = 100f * units;

#if GTAV_FORK_EXTENSIONS
		water.CaptureRefraction = false;
		water.SharedSwellHeight = swell;
		water.WaveHeight = height * units;
		water.WaveEdgeFade = Math.Min( 12f, 0.25f * Math.Min( bounds.Size.x, bounds.Size.y ) ) * units;
#else
		// Stock water has one wave height. Use the swell for calm coastal tiles.
		water.WaveHeight = Math.Max( height * units, swell );
#endif

		obj.Enabled = true;
		return obj;
	}
}
