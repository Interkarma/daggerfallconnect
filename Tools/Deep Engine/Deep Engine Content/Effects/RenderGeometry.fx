///////////////////////////////////////////////////////////////////
// Parameters

float4x4 World;
float4x4 View;
float4x4 Projection;
texture Texture;
float SpecularIntensity = 0.0f;
float SpecularPower = 0.5f;
float3 DiffuseColor = float3(1,1,1);


///////////////////////////////////////////////////////////////////
// Samplers

sampler ColorTextureSampler = sampler_state
{
    Texture = (Texture);
};


///////////////////////////////////////////////////////////////////
// Default technique inputs and outputs

struct Default_VSI
{
    float4 Position			: POSITION0;
    float3 Normal			: NORMAL0;
    float2 TexCoord			: TEXCOORD0;
};

struct Default_VSO
{
    float4 Position			: POSITION0;
	float3 Normal			: NORMAL0;
    float2 TexCoord			: TEXCOORD0;
    float2 Depth			: TEXCOORD1;
};

struct Default_PSO
{
    half4 Color				: COLOR0;
    half4 Normal			: COLOR1;
    half4 Depth				: COLOR2;
};


///////////////////////////////////////////////////////////////////
// Diffuse technique inputs and outputs

struct Diffuse_VSI
{
    float4 Position			: POSITION0;
    float3 Normal			: NORMAL0;
};

struct Diffuse_VSO
{
    float4 Position			: POSITION0;
	float3 Normal			: NORMAL0;
    float2 Depth			: TEXCOORD1;
};


///////////////////////////////////////////////////////////////////
// Default technique shaders

Default_VSO Default_VS(Default_VSI input)
{
    Default_VSO output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    return output;
}

Default_PSO Default_PS(Default_VSO input)
{
    Default_PSO output;

	// Colour texture
    output.Color = tex2D(ColorTextureSampler, input.TexCoord);

	// Alpha 0x00 - 0x7f can be used for specular.
	// Alpha 0x80 - 0xff are emissive and are left alone.
	if (output.Color.a < 0.5f)
	{
		output.Color.a = SpecularIntensity;
	}

	// Normal
	output.Normal.rgb = 0.5f * (input.Normal + 1.0f);
	output.Normal.a = SpecularPower;

	// Depth
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}


///////////////////////////////////////////////////////////////////
// Diffuse technique shaders

Diffuse_VSO Diffuse_VS(Diffuse_VSI input)
{
    Diffuse_VSO output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    return output;
}

Default_PSO Diffuse_PS(Diffuse_VSO input)
{
    Default_PSO output;
    output.Color = float4(DiffuseColor, 1);
	output.Normal.rgb = 0.5f * (input.Normal + 1.0f);
	output.Normal.a = 1;
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}


///////////////////////////////////////////////////////////////////
// Technique declarations

// Default technique. Draws fully textured primitives.
technique Default
{
    pass P0
    {
        VertexShader = compile vs_3_0 Default_VS();
        PixelShader = compile ps_3_0 Default_PS();
    }
}

// Diffuse technique. Draws primitives with a single diffuse colour in place of a texture.
technique Diffuse
{
	pass P0
	{
		VertexShader = compile vs_3_0 Diffuse_VS();
		PixelShader = compile ps_3_0 Diffuse_PS();
	}
}