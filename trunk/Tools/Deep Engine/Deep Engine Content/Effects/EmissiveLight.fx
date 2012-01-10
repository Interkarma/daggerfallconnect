/*
 * Adds emissive light from diffuse map alpha buffer.
 * Color sampler alpha 0.5 - 1.0 are reserved for emissive textures.
 * 0.5 is dark and 1.0 is fullbright.
*/

texture colorMap;				// Diffuse colour and brightness in alpha channel
float2 GBufferTextureSize;		// GBuffer size


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


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);

    // Align texture coordinates
	output.TexCoord = input.TexCoord - float2(-0.5f / GBufferTextureSize.xy);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Get specular or emissive intensity value from color sampler
	// 0.0 - 0.5 are specular intensity
	// 0.5 - 1.0 are emissive intensity
    float specularOrEmissiveIntensityValue = tex2D(colorSampler, input.TexCoord).a;

	// Ignore everything below 0.5f at it is specular intensity
	clip(specularOrEmissiveIntensityValue - 0.5f);

	// Shift emissive intensity value into 0.0 - 1.0 range
	float emissiveIntensity = -((0.5f - specularOrEmissiveIntensityValue) / 0.5f);

	// Paint emissive light
	return float4(emissiveIntensity, emissiveIntensity, emissiveIntensity, 0);
}


technique Technique0
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}