//
// Clears GBuffer.
//

float3 ClearColor = float3(0,0,0);	// Colour to clear diffuse buffer.

struct VertexShaderInput
{
    float3 Position		: POSITION0;
};

struct VertexShaderOutput
{
    float4 Position		: POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    return output;
}

struct PixelShaderOutput
{
	float4 Color		: COLOR0;
    float4 Normal		: COLOR1;
	float4 Depth		: COLOR2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

	// Color
    output.Color = float4(ClearColor, 0);

    // Normal
	// When transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;

    // No specular power
    output.Normal.a = 0.0f;

	// Max depth
    output.Depth = 1.0f;

    return output;
}

technique ClearTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}