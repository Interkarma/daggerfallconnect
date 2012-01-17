//------- Technique: PerlinNoise --------

Texture Texture;
float Time;
float Overcast;
float Brightness = 1.0f;

sampler TextureSampler = sampler_state
{
	texture   = (Texture);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU  = mirror;
	AddressV  = mirror;
};

struct PNVertexToPixel
{    
    float4 Position         : POSITION;
    float2 TextureCoords    : TEXCOORD0;
};

struct PNPixelToFrame
{
    float4 Color : COLOR0;
};

PNVertexToPixel PerlinVS(float4 inPos : POSITION, float2 inTexCoords: TEXCOORD)
{    
    PNVertexToPixel Output = (PNVertexToPixel)0;
     
    Output.Position = inPos;
    Output.TextureCoords = inTexCoords;
    
    return Output;    
}

PNPixelToFrame PerlinPS(PNVertexToPixel PSIn)
{
    PNPixelToFrame Output = (PNPixelToFrame)0;    
    
    float2 move = float2(0,1);
    float4 perlin = tex2D(TextureSampler, (PSIn.TextureCoords)+Time*move)/2;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*2+Time*move)/4;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*4+Time*move)/8;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*8+Time*move)/16;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*16+Time*move)/32;
    perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*32+Time*move)/32;    
    
    Output.Color.rgb = Brightness * (1.0f-pow(abs(perlin.r), Overcast)*2.0f);
    Output.Color.a =1;

    return Output;
}

technique PerlinNoise
{
	pass Pass0
    {
		VertexShader = compile vs_1_1 PerlinVS();
        PixelShader = compile ps_2_0 PerlinPS();
    }
}
