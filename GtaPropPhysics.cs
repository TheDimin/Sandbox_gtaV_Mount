using Sandbox;

namespace GtaV;

internal static class GtaPropPhysics
{
	internal static void SetEnabled( Prop prop, bool enabled )
	{
#if GTAV_FORK_EXTENSIONS
		prop.PhysicsEnabled = enabled;
#else
		// Stock Prop creates these components when it becomes active.
		// Change their state independently from its renderer and damage component.
		var components = prop.Components;
		var body = components.Get<Rigidbody>( true );
		var collider = components.Get<ModelCollider>( true );
		var modelPhysics = components.Get<ModelPhysics>( true );
		if ( !enabled && body.IsValid() ) body.Enabled = false;
		if ( collider.IsValid() ) collider.Enabled = enabled;
		if ( modelPhysics.IsValid() ) modelPhysics.Enabled = enabled;
		if ( enabled && body.IsValid() ) body.Enabled = true;
#endif
	}
}
