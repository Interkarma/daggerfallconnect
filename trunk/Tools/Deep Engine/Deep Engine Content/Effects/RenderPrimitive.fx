//
// Render colored primitive into GBuffer.
//

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 DiffuseColor = float3(1,1,1);

struct VertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
	float3 Normal			: NORMAL0;
    float2 Depth			: TEXCOORD1;
};

VertexShaderOutput DeferredPrimitiveVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
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

PixelShaderOutput DeferredPrimitivePS(VertexShaderOutput input)
{
    PixelShaderOutput output;
    output.Color = float4(DiffuseColor, 1);
	output.Normal = float4(input.Normal, 1);
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}

technique DeferredPrimitiveTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 DeferredPrimitiveVS();
        PixelShader = compile ps_3_0 DeferredPrimitivePS();
    }
}
