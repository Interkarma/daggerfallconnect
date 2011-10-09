/*
 * Adds emissive light from diffuse map alpha buffer.
 * Alpha 0x80 - 0xff are reserved for emissive textures.
 * 0x80 is dark and 0xfe is fullbright.
 * 0xff is reserved for windows.
*/

// Diffuse colour and brightness in alpha channel
texture colorMap;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);

    // Align texture coordinates
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Get emissive value from colorMap
    float emissiveValue = tex2D(colorSampler, input.TexCoord).a;

	// Everything below 0x80 (0.5f) is specular
	if (emissiveValue < 0.5f)
	{
		// Paint transparent
		return float4(0,0,0,0);
	}

	// Shift emissive value into 0.0 - 1.0 range
	float emissiveIntensity = -((0.5f - emissiveValue) / 0.5f);

	// Paint emissive light
	return float4(emissiveIntensity,emissiveIntensity,emissiveIntensity,0);
}

technique Technique0
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}