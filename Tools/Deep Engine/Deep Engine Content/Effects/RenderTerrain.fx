///////////////////////////////////////////////////////////////////
// Parameters

// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;

// Textures
texture2D VertexTexture;
texture2D BlendTexture;
texture2D Diffuse1Texture;
texture2D Diffuse2Texture;
texture2D Diffuse3Texture;
texture2D Diffuse4Texture;
texture2D Diffuse5Texture;

// Vertical scale
float2 SampleOffset;
float2 SampleScale;
float TextureRepeat;
float MaxHeight;


///////////////////////////////////////////////////////////////////
// Samplers

sampler VertexSampler = sampler_state
{
	Texture = <VertexTexture>;
	MipFilter = Point;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler BlendSampler = sampler_state
{
    Texture = <BlendTexture>;
};

sampler Diffuse1Sampler = sampler_state
{
    Texture = <Diffuse1Texture>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Diffuse2Sampler = sampler_state
{
    Texture = <Diffuse2Texture>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Diffuse3Sampler = sampler_state
{
    Texture = <Diffuse3Texture>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Diffuse4Sampler = sampler_state
{
    Texture = <Diffuse4Texture>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Diffuse5Sampler = sampler_state
{
    Texture = <Diffuse5Texture>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};


///////////////////////////////////////////////////////////////////
// Default technique inputs and outputs

struct Default_VSI
{
	float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
    float2 TexCoord		: TEXCOORD0;
};

struct Default_VSO
{
    float4 Position		: POSITION0;
	float3 Normal		: NORMAL0;
    float2 TexCoord		: TEXCOORD0;
    float2 Depth		: TEXCOORD1;
};

struct Default_PSO
{
    float4 Color		: COLOR0;
    float4 Normal		: COLOR1;
    float4 Depth		: COLOR2;
};


///////////////////////////////////////////////////////////////////
// Default technique shaders

Default_VSO RenderTerrain_VS(Default_VSI input)
{
    Default_VSO output;

	// Get sample point
	float2 terrainuv = input.TexCoord * SampleScale + SampleOffset;

	// Sample terrain vertex texture
	float4 terrainSample = tex2Dlod(VertexSampler, float4(terrainuv.xy, 0, 0));

	// Set normal and height
	input.Position.y = terrainSample.w * MaxHeight;
	
	// Calculate data for the pixel shader.
	output.Position = mul(input.Position, World);
	output.Position = mul(output.Position, View);
	output.Position = mul(output.Position, Projection);
	
	// Pass-through remaining coordinates
	output.Normal = terrainSample.xyz;
	output.TexCoord = input.TexCoord;
	output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;
	
    return output;
}

Default_PSO RenderTerrain_PS(Default_VSO input)
{
	Default_PSO output;

	// Get sample point
	float2 blenduv = input.TexCoord * SampleScale + SampleOffset;

	// Sample blend texture
	float blendfactor = 1.0f;
	float4 blendlayers = tex2D(BlendSampler, blenduv);

	// Scale diffuse coordinates
	float2 uv = input.TexCoord * TextureRepeat;
	
	// Texture1
	float3 diffuse = tex2D(Diffuse1Sampler, uv) * blendfactor * blendlayers.x;
	blendfactor = saturate(blendfactor - blendlayers.x);
	
	// Texture2
	diffuse += tex2D(Diffuse2Sampler, uv) * blendfactor * blendlayers.y;
	blendfactor = saturate(blendfactor - blendlayers.y);
	
	// Texture3
	diffuse += tex2D(Diffuse3Sampler, uv) * blendfactor * blendlayers.z;
	blendfactor = saturate(blendfactor - blendlayers.z);
	
	// Texture4
	diffuse += tex2D(Diffuse4Sampler, uv) * blendfactor * blendlayers.w;
	blendfactor = saturate(blendfactor - blendlayers.w);
	
	// Texture5
	diffuse += tex2D(Diffuse5Sampler, uv) * blendfactor;

	// Set output
	output.Color = float4(diffuse, 0);
	output.Normal = float4(0.5f * (input.Normal + 1.0f), 0);
	output.Depth = input.Depth.x / input.Depth.y;
	
	return output;
}


///////////////////////////////////////////////////////////////////
// Technique declarations

technique Default
{
    pass P0
    {
        VertexShader = compile vs_3_0 RenderTerrain_VS();
        PixelShader = compile ps_3_0 RenderTerrain_PS();
    }
}
