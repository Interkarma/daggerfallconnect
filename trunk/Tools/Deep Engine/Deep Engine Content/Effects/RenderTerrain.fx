///////////////////////////////////////////////////////////////////
// Parameters

// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;

// Textures
texture2D VertexTexture;
texture2D BlendTexture0;
texture2D BlendTexture1;
texture2D Texture0;
texture2D Texture1;
texture2D Texture2;
texture2D Texture3;
texture2D Texture4;
texture2D Texture5;
texture2D Texture6;
texture2D Texture7;
texture2D Texture8;

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

sampler BlendSampler1 = sampler_state
{
    Texture = <BlendTexture0>;
};

sampler BlendSampler2 = sampler_state
{
    Texture = <BlendTexture1>;
};

sampler Texture0Sampler = sampler_state
{
    Texture = <Texture0>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture1Sampler = sampler_state
{
    Texture = <Texture1>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture2Sampler = sampler_state
{
    Texture = <Texture2>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture3Sampler = sampler_state
{
    Texture = <Texture3>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture4Sampler = sampler_state
{
    Texture = <Texture4>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture5Sampler = sampler_state
{
    Texture = <Texture5>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture6Sampler = sampler_state
{
    Texture = <Texture6>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture7Sampler = sampler_state
{
    Texture = <Texture7>;
	MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler Texture8Sampler = sampler_state
{
    Texture = <Texture8>;
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

	// Sample blend textures
	float blendfactor = 1.0f;
	float4 blendlayers1 = tex2D(BlendSampler1, blenduv);
	float4 blendlayers2 = tex2D(BlendSampler2, blenduv);

	// Scale diffuse coordinates
	float2 uv = input.TexCoord * TextureRepeat;
	
	// Texture1
	float3 diffuse = tex2D(Texture1Sampler, uv) * blendfactor * blendlayers1.x;
	blendfactor = saturate(blendfactor - blendlayers1.x);
	
	// Texture2
	diffuse += tex2D(Texture2Sampler, uv) * blendfactor * blendlayers1.y;
	blendfactor = saturate(blendfactor - blendlayers1.y);
	
	// Texture3
	diffuse += tex2D(Texture3Sampler, uv) * blendfactor * blendlayers1.z;
	blendfactor = saturate(blendfactor - blendlayers1.z);
	
	// Texture4
	diffuse += tex2D(Texture4Sampler, uv) * blendfactor * blendlayers1.w;
	blendfactor = saturate(blendfactor - blendlayers1.w);

	// Texture5
	diffuse += tex2D(Texture5Sampler, uv) * blendfactor * blendlayers2.x;
	blendfactor = saturate(blendfactor - blendlayers2.x);
	
	// Texture6
	diffuse += tex2D(Texture6Sampler, uv) * blendfactor * blendlayers2.y;
	blendfactor = saturate(blendfactor - blendlayers2.y);
	
	// Texture7
	diffuse += tex2D(Texture7Sampler, uv) * blendfactor * blendlayers2.z;
	blendfactor = saturate(blendfactor - blendlayers2.z);
	
	// Texture8
	diffuse += tex2D(Texture8Sampler, uv) * blendfactor * blendlayers2.w;
	blendfactor = saturate(blendfactor - blendlayers2.w);
	
	// Texture0
	diffuse += tex2D(Texture0Sampler, uv) * blendfactor;

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
