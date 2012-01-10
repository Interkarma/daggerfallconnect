#define FxaaBool bool
#define FxaaDiscard clip(-1)
#define FxaaFloat float
#define FxaaFloat2 float2
#define FxaaFloat3 float3
#define FxaaFloat4 float4
#define FxaaHalf half
#define FxaaHalf2 half2
#define FxaaHalf3 half3
#define FxaaHalf4 half4
#define FxaaSat(x) saturate(x)

#define FxaaInt2 float2
#define FxaaTex sampler2D
#define FxaaTexTop(t, p) tex2D(t, float4(p, 0.0, 0.0))
#define FxaaTexOff(t, p, o, r) tex2D(t, float4(p + (o * r), 0, 0))

FxaaFloat FxaaLuma(FxaaFloat4 rgba) { 
    rgba.w = dot(rgba.rgb, FxaaFloat3(0.299, 0.587, 0.114));
return  rgba.w; }

/*============================================================================
                         FXAA3 CONSOLE - PC VERSION
============================================================================*/
FxaaFloat4 FxaaPixelShader(
    FxaaFloat2 pos,
    FxaaFloat4 fxaaConsolePosPos,
    FxaaTex tex,
    FxaaFloat4 fxaaConsoleRcpFrameOpt,
    FxaaFloat4 fxaaConsoleRcpFrameOpt2,
    FxaaFloat fxaaConsoleEdgeSharpness,
    FxaaFloat fxaaConsoleEdgeThreshold,
    FxaaFloat fxaaConsoleEdgeThresholdMin) {
    FxaaFloat lumaNw = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.xy));
    FxaaFloat lumaSw = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.xw));
    FxaaFloat lumaNe = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.zy));
    FxaaFloat lumaSe = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.zw));
    FxaaFloat4 rgbyM = FxaaTexTop(tex, pos.xy);
    #if (FXAA_GREEN_AS_LUMA == 0)
        FxaaFloat lumaM = rgbyM.w;
    #else
        FxaaFloat lumaM = rgbyM.y;
    #endif
    FxaaFloat lumaMaxNwSw = max(lumaNw, lumaSw);
    lumaNe += 1.0/384.0;
    FxaaFloat lumaMinNwSw = min(lumaNw, lumaSw);
    FxaaFloat lumaMaxNeSe = max(lumaNe, lumaSe);
    FxaaFloat lumaMinNeSe = min(lumaNe, lumaSe);
    FxaaFloat lumaMax = max(lumaMaxNeSe, lumaMaxNwSw);
    FxaaFloat lumaMin = min(lumaMinNeSe, lumaMinNwSw);
    FxaaFloat lumaMaxScaled = lumaMax * fxaaConsoleEdgeThreshold;
    FxaaFloat lumaMinM = min(lumaMin, lumaM);
    FxaaFloat lumaMaxScaledClamped = max(fxaaConsoleEdgeThresholdMin, lumaMaxScaled);
    FxaaFloat lumaMaxM = max(lumaMax, lumaM);
    FxaaFloat dirSwMinusNe = lumaSw - lumaNe;
    FxaaFloat lumaMaxSubMinM = lumaMaxM - lumaMinM;
    FxaaFloat dirSeMinusNw = lumaSe - lumaNw;
    if(lumaMaxSubMinM < lumaMaxScaledClamped) return rgbyM;
    FxaaFloat2 dir;
    dir.x = dirSwMinusNe + dirSeMinusNw;
    dir.y = dirSwMinusNe - dirSeMinusNw;
    FxaaFloat2 dir1 = normalize(dir.xy);
    FxaaFloat4 rgbyN1 = FxaaTexTop(tex, pos.xy - dir1 * fxaaConsoleRcpFrameOpt.zw);
    FxaaFloat4 rgbyP1 = FxaaTexTop(tex, pos.xy + dir1 * fxaaConsoleRcpFrameOpt.zw);
    FxaaFloat dirAbsMinTimesC = min(abs(dir1.x), abs(dir1.y)) * fxaaConsoleEdgeSharpness;
    FxaaFloat2 dir2 = clamp(dir1.xy / dirAbsMinTimesC, -2.0, 2.0);
    FxaaFloat4 rgbyN2 = FxaaTexTop(tex, pos.xy - dir2 * fxaaConsoleRcpFrameOpt2.zw);
    FxaaFloat4 rgbyP2 = FxaaTexTop(tex, pos.xy + dir2 * fxaaConsoleRcpFrameOpt2.zw);
    FxaaFloat4 rgbyA = rgbyN1 + rgbyP1;
    FxaaFloat4 rgbyB = ((rgbyN2 + rgbyP2) * 0.25) + (rgbyA * 0.25);
    #if (FXAA_GREEN_AS_LUMA == 0)
        FxaaBool twoTap = (rgbyB.w < lumaMin) || (rgbyB.w > lumaMax);
    #else
        FxaaBool twoTap = (rgbyB.y < lumaMin) || (rgbyB.y > lumaMax);
    #endif
    if(twoTap) rgbyB.xyz = rgbyA.xyz * 0.5;
    return rgbyB; 
}
/*==========================================================================*/

uniform extern float SCREEN_WIDTH;
uniform extern float SCREEN_HEIGHT;
uniform extern texture gScreenTexture;

sampler screenSampler = sampler_state
{
    Texture = <gScreenTexture>;
    /*MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;*/
};

float4 PixelShaderFunction(float2 tc : TEXCOORD0) : COLOR0
{
    float pixelWidth = (1 / SCREEN_WIDTH);
    float pixelHeight = (1 / SCREEN_HEIGHT);

    float2 pixelCenter = float2(tc.x - pixelWidth, tc.y - pixelHeight);
    float4 fxaaConsolePosPos = float4(tc.x, tc.y, tc.x + pixelWidth, tc.y + pixelHeight);

    return FxaaPixelShader(
        pixelCenter,
        fxaaConsolePosPos,
        screenSampler,
        float4(-0.50 / SCREEN_WIDTH, -0.50 / SCREEN_HEIGHT, 0.50 / SCREEN_WIDTH, 0.50 / SCREEN_HEIGHT),
        float4(-2.0 / SCREEN_WIDTH, -2.0 / SCREEN_HEIGHT, 2.0 / SCREEN_WIDTH, 2.0 / SCREEN_HEIGHT),
        8.0,
        0.125,
        0.05);
}

technique ppfxaa
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
