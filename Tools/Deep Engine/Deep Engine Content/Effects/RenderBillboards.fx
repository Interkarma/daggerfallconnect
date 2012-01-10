///////////////////////////////////////////////////////////////////
// Parameters

float4x4 World;
float4x4 View;
float4x4 Projection;
texture Texture;
float3 Position = float3(0,0,0);
float2 Size = float2(0,0);

// 1 means we should only accept opaque pixels.
// -1 means only accept transparent pixels.
float AlphaTestDirection = 1;
float AlphaTestThreshold = 0.65;


///////////////////////////////////////////////////////////////////
// Samplers

sampler TextureSampler = sampler_state
{
    Texture = (Texture);
};


///////////////////////////////////////////////////////////////////
// Default technique inputs and outputs

struct Default_VSI
{
    float4 Position			: POSITION0;
    float3 Normal			: NORMAL0;
    float2 TexCoord			: TEXCOORD0;
};

struct Default_VSO
{
    float4 Position			: POSITION0;
	float3 Normal			: NORMAL0;
    float2 TexCoord			: TEXCOORD0;
	float2 Depth			: TEXCOORD1;
	float4 ScreenPosition	: TEXCOORD2;
};

struct Default_PSO
{
    half4 Color				: COLOR0;
    half4 Normal			: COLOR1;
	half4 Depth				: COLOR2;
};


///////////////////////////////////////////////////////////////////
// Default technique shaders

Default_VSO Default_VS(Default_VSI input)
{
    Default_VSO output;

	// Get position
	float3 center = mul(input.Position, World) + Position;

	// Work out what direction we are viewing the billboard from
    float3 viewDirection = View._m02_m12_m22;
	float3 rightVector = -normalize(cross(viewDirection, input.Normal));

	// Calculate the position of this billboard vertex
	float3 position = input.Position + center;

	// Offset to the left or right
    position += rightVector * (input.TexCoord.x - 0.5) * Size.x;

	// Offset upward if we are one of the top two vertices
    position += input.Normal * (0.5 - input.TexCoord.y) * Size.y;

	// Apply the camera transform
    float4 viewPosition = mul(float4(position, 1), View);

	// Set output
	output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
	output.TexCoord = input.TexCoord;
	output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;
	output.ScreenPosition = output.Position;

    return output;
}

Default_PSO Default_PS(Default_VSO input)
{
    Default_PSO output;

	// Sample texture
	float4 color = tex2D(TextureSampler, input.TexCoord);

	// Apply the alpha test
	clip((color.a - AlphaTestThreshold) * AlphaTestDirection);

	// Obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;

	// Obtain textureCoordinates corresponding to the current pixel
    // The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x,-input.ScreenPosition.y) + 1);

	// Colour
	output.Color.rgb = color.rgb;
	output.Color.a = 0;

	// Normal
	output.Normal.rgb = 0.5f * (input.Normal + 1.0f);
	output.Normal.a = 1;

	// Depth
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}


///////////////////////////////////////////////////////////////////
// Technique declarations

// Technique for colour pass of billboard
technique Default
{
    pass P0
    {
        VertexShader = compile vs_3_0 Default_VS();
        PixelShader = compile ps_3_0 Default_PS();
    }
}