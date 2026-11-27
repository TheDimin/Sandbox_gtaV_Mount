using Sandbox;

namespace GtaV;

/// <summary>Overrides presentation for a mounted GTA prop.</summary>
[AssetType( Name = "GTA Prop Destruction Profile", Extension = "gtapropfx", Category = "Effects" )]
public sealed class GtaPropDestructionProfile : GameResource
{
	public PrefabFile DamageEffect { get; set; }
	public PrefabFile BreakEffect { get; set; }
	public PrefabFile GibEffect { get; set; }
	public SoundEvent DamageSound { get; set; }
	public SoundEvent BreakSound { get; set; }
	public SoundEvent GibSound { get; set; }
	public bool ShakeOnExplosion { get; set; } = true;
}
