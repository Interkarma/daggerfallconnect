//
// Render textured and normal mapped geometry into GBuffer.
//

float4x4 World;
float4x4 View;
float4x4 Projection;
float specularIntensity = 0.0f;
float specularPower = 0.5f; 

texture Texture;
sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

/*
texture SpecularMap;
sampler specularSampler = sampler_state
{
    Texture = (SpecularMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};
*/

texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
    float2 TexCoord		: TEXCOORD0;
    float3 Binormal		: BINORMAL0;
    float3 Tangent		: TANGENT0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
    float2 Depth			: TEXCOORD1;
    float3x3 TangentToWorld	: TEXCOORD2;
};

VertexShaderOutput DeferredVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors
    output.TangentToWorld[0] = mul(input.Tangent, World);
    output.TangentToWorld[1] = mul(input.Binormal, World);
    output.TangentToWorld[2] = mul(input.Normal, World);

    return output;
}
struct PixelShaderOutput
{
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
};

PixelShaderOutput DeferredPS(VertexShaderOutput input)
{
    PixelShaderOutput output;
    output.Color = tex2D(diffuseSampler, input.TexCoord);
    
    //float4 specularAttributes = tex2D(specularSampler, input.TexCoord);

	// Alpha 0x00 - 0x7f can be used for specular.
	// Alpha 0x80 - 0xff are emissive and are left alone.
	if (output.Color.a < 0.5f)
	{
		output.Color.a = specularIntensity;
		//output.Color.a = specularAttributes.r;
	}
    
    // read the normal from the normal map
    float3 normalFromMap = tex2D(normalSampler, input.TexCoord);
    //tranform to [-1,1]
    normalFromMap = 2.0f * normalFromMap - 1.0f;
    //transform into world space
    normalFromMap = mul(normalFromMap, input.TangentToWorld);
    //normalize the result
    normalFromMap = normalize(normalFromMap);
    //output the normal, in [0,1] space
    output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);

    //specular Power
    //output.Normal.a = specularAttributes.a;
	output.Normal.a = specularPower;

    output.Depth = input.Depth.x / input.Depth.y;
    return output;
}
technique DeferredTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 DeferredVS();
        PixelShader = compile ps_3_0 DeferredPS();
    }
}
