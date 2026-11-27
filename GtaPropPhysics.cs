using Sandbox;

namespace GtaV;

/// <summary>Disable the generated prop collision where the map supplies its own collision.</summary>
public sealed class GtaPropPhysics : Component, Component.ExecuteInEditor
{
	protected override void OnEnabled()
	{
		if ( Components.Get<Prop>() is { } prop ) Apply( prop, false );
	}

	internal static void Apply( Prop prop, bool enabled )
	{
		var components = prop.Components;
		if ( components.Get<ModelCollider>( true ) is { } collider ) collider.Enabled = enabled;
		if ( components.Get<Rigidbody>( true ) is { } body ) body.Enabled = enabled;
		if ( components.Get<ModelPhysics>( true ) is { } physics ) physics.Enabled = enabled;
	}
}
