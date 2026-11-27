HEADER
{
	Description = "GTA world normal and specular material";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		return FinalizeVertex( ProcessVertex( i ) );
	}
}

PS
{
	#include "common/pixel.hlsl"

	CreateInputTexture2D( TextureColor, Srgb, 8, "", "_color", "Color,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateInputTexture2D( TextureNormal, Linear, 8, "NormalizeNormals", "_normal", "Normal,10/20", Default3( 0.5, 0.5, 1.0 ) );
	CreateInputTexture2D( TextureSpecular, Linear, 8, "", "_specular", "Specular,10/30", Default4( 0.0, 0.0, 0.0, 1.0 ) );
	Texture2D g_tColor < Channel( RGBA, Box( TextureColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( TextureNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	Texture2D g_tSpecular < Channel( RGBA, Box( TextureSpecular ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	float4 g_vSpecResponse < Default4( 1.0, 0.0, 0.0, 0.125 ); >;
	float g_flSpecFalloff < Default( 100.0 ); >;

	float4 ShadeGtaWorld( Material m, float dielectricReflectance )
	  {
	    // Want it right before the lighting
	    Decals::Apply( m.WorldPosition, m );

	    // Do our magic alpha to coverage adjustment
	    AdjustAlphaToCoverage( m );

	    // Depth mode writes only normals and roughness, or nothing but alpha to coverage. Before lighting: its cluster walks
	    // are wave ops, which the compiler keeps even when their result is dead
	    if( DepthNormals::WantsDepthNormals() )
	      return DepthNormals::Output( m.Normal, m.Roughness, m.Opacity );

	    // A lit pass module's normal and roughness for Borealis's screen-space reflections, as the depth-normals pass writes
	    // them: the forward entry the compiler wraps round its MainPs defines this and writes them (sbox/forward_normals.slangh)
	    #ifdef FORWARD_NORMALS_KEEP
	      ForwardNormalsKeep( m.Normal, m.Roughness );
	    #endif

	    LightingTerms_t lightingTerms = InitLightingTerms();
	    CombinerInput combinerInput = ShadingModelStandard::MaterialToCombinerInput( m );
	    if ( dielectricReflectance >= 0.0 )
	      combinerInput.vSpecularColor = dielectricReflectance.xxx;

	    // Calculate lighting
	    {
	      ComputeDirectLighting( lightingTerms, combinerInput );
	      CalculateIndirectLighting( lightingTerms, combinerInput );
	    }

	    // Composite lighting terms, apply adjustments
	    float4 color;
	    {
	      float3 vDiffuseAO = CalculateDiffuseAmbientOcclusion( combinerInput, lightingTerms );
	      lightingTerms.vIndirectDiffuse.rgb *= vDiffuseAO.rgb;
	      lightingTerms.vDiffuse.rgb *= lerp( float3( 1.0, 1.0, 1.0 ), vDiffuseAO.rgb, combinerInput.flAmbientOcclusionDirectDiffuse );

	      float3 vSpecularAO = CalculateSpecularAmbientOcclusion( combinerInput, lightingTerms );
	      lightingTerms.vIndirectSpecular.rgb *= vSpecularAO.rgb;
	      lightingTerms.vSpecular.rgb *= lerp( float3( 1.0, 1.0, 1.0 ), vSpecularAO.rgb, combinerInput.flAmbientOcclusionDirectSpecular );

	      float3 vDiffuse = ( ( lightingTerms.vDiffuse.rgb + lightingTerms.vIndirectDiffuse.rgb ) * combinerInput.vDiffuseColor.rgb ) + combinerInput.vEmissive.rgb;
	      float3 vSpecular = lightingTerms.vSpecular.rgb + lightingTerms.vIndirectSpecular.rgb;

	      // A surface's own per-light terms, summed by the light loop (sbox/surface_passes.slangh), as emission
	      #ifdef SURFACE_LIGHT_HOOK
	        vDiffuse += SurfaceLightTotal();
	      #endif

	      color = float4( vDiffuse + vSpecular, m.Opacity );
	    }


	    if ( g_bWireframeMode )
	      return g_vWireframeColor;

	    if( ToolsVis::WantsToolsVis() )
	      return ShadingModelStandard::DoToolsVis( color, m, lightingTerms );

	    if ( g_bFogEnabled )
	    {
	      color.rgb = Fog::Apply( m.WorldPosition, m.ScreenPosition.xy, color.rgb );
	    }

	    return color;
	  }

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float2 uv = i.vTextureCoords.xy;
		float4 color = g_tColor.Sample( g_sAniso, uv );
		float2 n = g_tNormal.Sample( g_sAniso, uv ).xy * 2.0 - 1.0;
		float3 tangentNormal = float3( n, sqrt( saturate( 1.0 - dot( n, n ) ) ) );
		// GTA stores a specular exponent in alpha. Convert it to an approximate
		// roughness for the s&box standard lighting model.
		float4 specular = g_tSpecular.Sample( g_sAniso, uv );
		float reflectance = saturate( dot( specular.rgb, g_vSpecResponse.rgb ) * g_vSpecResponse.a );
		float exponent = max( 0.0, specular.a * g_flSpecFalloff );
		Material m = Material::Init( i );
		m.Albedo = color.rgb;
		m.Opacity = color.a;
		m.Roughness = saturate( pow( 2.0 / (exponent + 2.0), 0.25 ) );
		m.Normal = normalize( tangentNormal.x * i.vTangentUWs + tangentNormal.y * i.vTangentVWs + tangentNormal.z * normalize( i.vNormalWs.xyz ) );
		return ShadeGtaWorld( m, reflectance );
	}
}
