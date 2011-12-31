//
// Render textured geometry into GBuffer.
//

float4x4 World;
float4x4 View;
float4x4 Projection;
float SpecularIntensity = 0.0f;
float SpecularPower = 0.5f; 

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

struct VertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
    float2 TexCoord		: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
	float3 Normal			: NORMAL0;
    float2 TexCoord			: TEXCOORD0;
    float2 Depth			: TEXCOORD1;
};

VertexShaderOutput DeferredGeometryVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    return output;
}

struct PixelShaderOutput
{
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
};

PixelShaderOutput DeferredGeometryPS(VertexShaderOutput input)
{
    PixelShaderOutput output;

	// Diffuse
    output.Color = tex2D(diffuseSampler, input.TexCoord);

	// Alpha 0x00 - 0x7f can be used for specular.
	// Alpha 0x80 - 0xff are emissive and are left alone.
	if (output.Color.a < 0.5f)
	{
		output.Color.a = SpecularIntensity;
	}

	// Normal
	output.Normal = float4(input.Normal, SpecularPower);

	// Depth
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}
technique DeferredTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 DeferredGeometryVS();
        PixelShader = compile ps_3_0 DeferredGeometryPS();
    }
}
