//
// Outputs Single formatted depth buffer to a non-linear image.
//


///////////////////////////////////////////////////////////////////
// Parameters

texture DepthMap;
float2 HalfPixel;


///////////////////////////////////////////////////////////////////
// Samplers

sampler DepthSampler = sampler_state
{
    Texture = <DepthMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};


///////////////////////////////////////////////////////////////////
// Default technique inputs and outputs

struct VertexShaderInput
{
    float3 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Color		: COLOR0;
};


///////////////////////////////////////////////////////////////////
// Default technique shaders

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position,1);
    output.TexCoord = input.TexCoord - HalfPixel;

    return output;
}


PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	// Get depth value
	float4 depth = tex2D(DepthSampler, input.TexCoord);

	// Cheat a reasonable representation of the depth distribution.
	// Not as robust as properly unprojecting, but good enough for this purpose.
	float z = saturate((depth.r - 0.99f) * 100);

	output.Color = float4(z,z,z,1);

	return output;
}


///////////////////////////////////////////////////////////////////
// Technique declarations

technique Default
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}