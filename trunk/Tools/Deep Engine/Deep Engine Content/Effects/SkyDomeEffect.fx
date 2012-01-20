//------- Technique: SkyDome --------

float4x4 View;
float4x4 Projection;
float4x4 World;
Texture CloudTexture;
Texture StarTexture;

float4 TopColor = float4(0.3f, 0.3f, 0.8f, 1);
float4 BottomColor = 1;

sampler CloudSampler = sampler_state
{
	texture   = (CloudTexture);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU  = mirror;
	AddressV  = mirror;
};

sampler StarSampler = sampler_state
{
	texture   = (StarTexture);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU  = mirror;
	AddressV  = mirror;
};

struct SDVertexToPixel
{    
    float4 Position         : POSITION;
    float2 TextureCoords    : TEXCOORD0;
    float4 ObjectPosition    : TEXCOORD1;
};

struct SDPixelToFrame
{
    float4 Color : COLOR0;
};

SDVertexToPixel SkyDomeVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{    
    SDVertexToPixel Output = (SDVertexToPixel)0;
    float4x4 preViewProjection = mul (View, Projection);
    float4x4 preWorldViewProjection = mul (World, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.TextureCoords = inTexCoords;
    Output.ObjectPosition = inPos;
    
    return Output;    
}

SDPixelToFrame SkyDomePS(SDVertexToPixel PSIn)
{
    SDPixelToFrame Output = (SDPixelToFrame)0;

	float4 baseColor = lerp(BottomColor, TopColor, saturate((PSIn.ObjectPosition.y)/0.4f));

	float4 cloudValue = tex2D(CloudSampler, PSIn.TextureCoords).r;
	float4 starValue = tex2D(StarSampler, PSIn.TextureCoords).r * 0.75f;

	// Stars dimmer behind darker clouds
	if (cloudValue.r > 0.12f)
	{
		starValue *= 0.1f;
	}
    
    Output.Color = lerp(baseColor, 1, cloudValue + starValue);

    return Output;
}

technique SkyDome
{
	// First pass draws stars
    pass Pass0
    {
        VertexShader = compile vs_1_1 SkyDomeVS();
        PixelShader = compile ps_2_0 SkyDomePS();
    }
}