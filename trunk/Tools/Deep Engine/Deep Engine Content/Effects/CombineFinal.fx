//
// Combines GBuffer targets into final image.
//

texture ColorMap;
texture LightMap;
texture DepthMap;
float3 AmbientColor;
float AmbientIntensity;
float2 HalfPixel;

sampler colorSampler = sampler_state
{
    Texture = <ColorMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler lightSampler = sampler_state
{
    Texture = <LightMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler depthSampler = sampler_state
{
    Texture = <DepthMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position		: POSITION0;
    float2 TexCoord		: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position		: POSITION0;
    float2 TexCoord		: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position,1);
    output.TexCoord = input.TexCoord - HalfPixel;

    return output;
}

struct PixelShaderOutput
{
    float4 Color		: COLOR0;
	float Depth			: DEPTH;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	// Calculate final colour
    float3 diffuseColor = tex2D(colorSampler,input.TexCoord).rgb;
    float4 light = tex2D(lightSampler,input.TexCoord);
    float3 diffuseLight = light.rgb;
    float specularLight = light.a;
	float3 ambientLight = diffuseColor * AmbientColor * AmbientIntensity;

	// Output Depth
	// This information is used by forward rendering to insert
	// objects (such as billboards) into scene with depth intact
	float4 depth = tex2D(depthSampler,input.TexCoord);
	output.Depth = depth;

	// Output final color on every pixel except those still at max depth (i.e. the sky).
	// Instead draw a tranparent pixel, allowing the background to be seen.
	if (depth.r < 1.0f)
		output.Color = float4((diffuseColor * diffuseLight + specularLight + ambientLight),1);
	else
		output.Color = float4(0,0,0,0);

	return output;
}

technique CombineTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
