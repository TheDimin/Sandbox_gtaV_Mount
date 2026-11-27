using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sandbox;

namespace GtaV;

/// <summary>Plays the mount's mapped effects when a stock prop takes damage or breaks.</summary>
public sealed class GtaDestructionPresentation : Component
{
	static readonly ConditionalWeakTable<Model, GtaDestructionCues> ModelCues = new();

	[Property] public string SurfacePath { get; set; }
	[Property] public string ProfilePath { get; set; }
	[Property] public string DamageEffect { get; set; }
	[Property] public string BreakEffect { get; set; }
	[Property] public string GibEffect { get; set; }
	[Property] public string BreakSound { get; set; }
	[Property] public string GibSound { get; set; }

	Prop prop;

	internal static void Register( Model model, GtaDestructionCues cues )
	{
		if ( model.IsValid() && cues is not null ) ModelCues.AddOrUpdate( model, cues );
	}

	internal static void Attach( Prop prop )
	{
		if ( !prop.Model.IsValid() || !ModelCues.TryGetValue( prop.Model, out var cues ) ) return;
		var effects = prop.Components.Create<GtaDestructionPresentation>( false );
		effects.SurfacePath = cues.Surface;
		effects.ProfilePath = cues.Profile;
		effects.DamageEffect = cues.DamageEffect;
		effects.BreakEffect = cues.BreakEffect;
		effects.GibEffect = cues.GibEffect;
		effects.BreakSound = cues.BreakSound;
		effects.GibSound = cues.GibSound;
		effects.Enabled = true;
	}

	protected override void OnEnabled()
	{
		prop = Components.Get<Prop>( true );
		if ( !prop.IsValid() ) return;
		prop.OnPropTakeDamage += OnDamage;
		prop.OnPropBreak += OnBreak;
		prop.OnGibsCreated += OnGibs;
	}

	protected override void OnDisabled()
	{
		if ( prop is null ) return;
		prop.OnPropTakeDamage -= OnDamage;
		prop.OnPropBreak -= OnBreak;
		prop.OnGibsCreated -= OnGibs;
		prop = null;
	}

	GtaPropDestructionProfile Profile => string.IsNullOrEmpty( ProfilePath ) ? null :
		ResourceLibrary.TryGet<GtaPropDestructionProfile>( ProfilePath, out var profile ) ? profile : null;

	Surface FindSurface( Surface hit = null )
	{
		if ( hit.IsValid() && hit.Index != 0 ) return hit;
		return string.IsNullOrEmpty( SurfacePath ) ? null : ResourceLibrary.Get<Surface>( SurfacePath );
	}

	void OnDamage( DamageInfo hit )
	{
		if ( hit.Damage <= 0 || !hit.Position.IsFinite || !CanPlay ) return;

		var profile = Profile;
		var surface = hit.Tags.Has( "bullet" ) ? null : FindSurface( hit.Shape?.Surface );
		var inherited = surface?.GetBaseSurface();
		var prefab = profile?.DamageEffect?.ResourcePath ?? DamageEffect ?? SurfaceEffect( surface, inherited );
		var sound = profile?.DamageSound ?? surface?.SoundCollection.ImpactSoft ?? inherited?.SoundCollection.ImpactSoft;
		var direction = hit.Position - hit.Origin;
		if ( !direction.IsFinite || direction.LengthSquared <= 0 ) direction = Vector3.Down;
		Play( prefab, sound, hit.Position, Rotation.LookAt( direction ) );
	}

	void OnBreak()
	{
		if ( !CanPlay ) return;
		var profile = Profile;
		var surface = prop.IsExplosive ? null : FindSurface();
		var inherited = surface?.GetBaseSurface();
		var prefab = profile?.BreakEffect?.ResourcePath ?? BreakEffect ?? SurfaceEffect( surface, inherited );
		var sound = profile?.BreakSound ?? LoadSound( BreakSound )
			?? surface?.SoundCollection.Break ?? inherited?.SoundCollection.Break
			?? surface?.SoundCollection.ImpactHard ?? inherited?.SoundCollection.ImpactHard;
		Play( prefab, sound, WorldPosition, Rotation.LookAt( Vector3.Down ) );
		if ( prop.IsExplosive && (profile?.ShakeOnExplosion ?? true) )
		{
			var radius = prop.Model?.Data.ExplosionRadius ?? 0;
			if ( !float.IsFinite( radius ) || radius <= 0 ) radius = 256;
			Scene.GetSystem<CameraEffectSystem>()?.AddShake( WorldPosition, Math.Clamp( radius * 2f, 1f, 4096f ), 2f, 22f, 0.35f );
		}
	}

	void OnGibs( List<Gib> pieces )
	{
		if ( !CanPlay ) return;
		var profile = Profile;
		var prefab = profile?.GibEffect?.ResourcePath ?? GibEffect;
		var sound = profile?.GibSound ?? LoadSound( GibSound );
		if ( string.IsNullOrEmpty( prefab ) && !sound.IsValid() ) return;

		foreach ( var piece in pieces )
		{
			if ( !piece.IsValid() ) continue;
			Play( prefab, sound, piece.WorldPosition, piece.WorldRotation );
			sound = null;
		}
	}

	bool CanPlay => !Application.IsDedicatedServer && !Scene.IsEditor && !prop.IsProxy;

	static string SurfaceEffect( Surface surface, Surface inherited )
	{
		var effect = surface?.PrefabCollection.BulletImpact ?? inherited?.PrefabCollection.BulletImpact;
		return effect.IsValid() ? effect.PrefabInstanceSource ?? (effect as PrefabScene)?.Source?.ResourcePath : null;
	}

	static SoundEvent LoadSound( string path ) => string.IsNullOrEmpty( path ) ? null : ResourceLibrary.Get<SoundEvent>( path );

	void Play( string prefab, SoundEvent sound, Vector3 position, Rotation rotation )
	{
		if ( !position.IsFinite || !rotation.IsFinite ) return;
		if ( sound.IsValid() ) Sound.Play( sound, position );
		if ( string.IsNullOrEmpty( prefab ) || GameObject.GetPrefab( prefab ) is not { } source ) return;

		using var scope = Scene.Push();
		var effect = source.Clone( new CloneConfig( new Transform( position, rotation ), startEnabled: false ) );
		if ( !effect.IsValid() ) return;
		effect.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.NotNetworked;
		effect.NetworkMode = NetworkMode.Never;
		if ( !effect.Components.Get<TemporaryEffect>( true ).IsValid() )
			effect.Components.Create<TemporaryEffect>().WaitForChildEffects = false;
		effect.Enabled = true;
	}
}
