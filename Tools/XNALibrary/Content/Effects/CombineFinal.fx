texture colorMap;
texture lightMap;
texture depthMap;

float3 AmbientColor;
float AmbientIntensity;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
sampler lightSampler = sampler_state
{
    Texture = (lightMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
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
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
	float Depth : DEPTH;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	// Output Colour
    float3 diffuseColor = tex2D(colorSampler,input.TexCoord).rgb;
    float4 light = tex2D(lightSampler,input.TexCoord);
    float3 diffuseLight = light.rgb;
    float specularLight = light.a;
	float3 ambientLight = diffuseColor * AmbientColor * AmbientIntensity;
    output.Color = float4((diffuseColor * diffuseLight + specularLight + ambientLight),1);

	// Output Depth
	// This information is used by forward renderering to insert
	// objects (such as billboards) into scene with depth intact
	float4 depth = tex2D(depthSampler,input.TexCoord);
	output.Depth = depth;

	return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
