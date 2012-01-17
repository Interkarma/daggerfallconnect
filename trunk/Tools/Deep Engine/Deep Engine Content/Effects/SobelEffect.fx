// The value of Normal Strength should be tweaked until the result is satisfying.
// A larger value will result in more pronounced lighting.
float NormalStrength = 8.0f;

// Size of one texel (1.0 / TextureDimension)
float TexelSize;

sampler TextureSampler : register(s0);

float4 ComputeNormalsPS(float2 uv:TEXCOORD0) : COLOR0
{
    float tl = abs(tex2D (TextureSampler, uv + TexelSize * float2(-1, -1)).x);   // top left
    float  l = abs(tex2D (TextureSampler, uv + TexelSize * float2(-1,  0)).x);   // left
    float bl = abs(tex2D (TextureSampler, uv + TexelSize * float2(-1,  1)).x);   // bottom left
    float  t = abs(tex2D (TextureSampler, uv + TexelSize * float2( 0, -1)).x);   // top
    float  b = abs(tex2D (TextureSampler, uv + TexelSize * float2( 0,  1)).x);   // bottom
    float tr = abs(tex2D (TextureSampler, uv + TexelSize * float2( 1, -1)).x);   // top right
    float  r = abs(tex2D (TextureSampler, uv + TexelSize * float2( 1,  0)).x);   // right
    float br = abs(tex2D (TextureSampler, uv + TexelSize * float2( 1,  1)).x);   // bottom right

    // Compute dx using Sobel:
    //
    //           -1 0 1 
    //           -2 0 2
    //           -1 0 1

    float dX = -tl - 2.0f*l - bl + tr + 2.0f*r + br;

    // Compute dy using Sobel:
    //
    //           -1 -2 -1 
    //            0  0  0
    //            1  2  1

    float dY = -tl - 2.0f*t - tr + bl + 2.0f*b + br;

    // Compute the normalized Normal
    float4 N = float4(normalize(float3(dX, 1.0f / NormalStrength, dY)), 1.0f);

    // Convert (-1.0 , 1.0) to (0.0 , 1.0)
	return N * 0.5f + 0.5f;
}

technique ComputeNormals
{
	pass P0
    {
		pixelShader  = compile ps_2_0 ComputeNormalsPS();
    }
}