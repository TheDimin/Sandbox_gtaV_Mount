using System;
using System.IO;
#if GTAV_FORK_DIRECT_GEOMETRY_UPLOAD
using GtaV.ManagedReader;
#endif

namespace GtaV;

/// <summary>The worker's result. The placement or synchronous load owns its lifetime.</summary>
internal readonly struct PreparedModelData : IDisposable
{
	internal BinaryReader Packet { get; }

	internal PreparedModelData( BinaryReader packet ) => Packet = packet;
#if GTAV_FORK_DIRECT_GEOMETRY_UPLOAD
	internal MountedDrawable Drawable { get; }
	internal PreparedModelData( MountedDrawable drawable ) => Drawable = drawable;
#endif

	public void Dispose()
	{
		Packet?.Dispose();
#if GTAV_FORK_DIRECT_GEOMETRY_UPLOAD
		Drawable?.Dispose();
#endif
	}
}
