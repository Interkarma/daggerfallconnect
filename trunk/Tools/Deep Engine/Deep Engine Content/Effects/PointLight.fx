//
// Renders a point light into GBuffer.
//

float4x4 World;					// World transformation matrix
float4x4 View;					// View transformation matrix
float4x4 Projection;			// Projection transformation matrix
float3 Color;					// Colour of the light
float3 CameraPosition;			// Position of the camera, for specular light
float4x4 InvertViewProjection;	// This is used to compute the world-position
float3 LightPosition;			// This is the position of the light
float LightRadius;				// How far does this light reach
float LightIntensity = 1.0f;	// Control the brightness of the light
texture ColorMap;				// Diffuse color, and specularIntensity in the alpha channel
texture NormalMap;				// Normals, and specularPower in the alpha channel
texture DepthMap;				// Depth
float2 HalfPixel;				// Buffer half pixel size

sampler colorSampler = sampler_state
{
    Texture = <ColorMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler normalSampler = sampler_state
{
    Texture = <NormalMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler depthSampler = sampler_state
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    // Processing geometry coordinates
    float4 worldPosition = mul(float4(input.Position,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.ScreenPosition = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;

    // Obtain textureCoordinates corresponding to the current pixel
    // The screen coordinates are in [-1,1]*[1,-1]
    // The texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x,-input.ScreenPosition.y) + 1);

    // Align texels to pixels
    texCoord += HalfPixel;

    // Get normal data from the normalMap
    float4 normalData = tex2D(normalSampler,texCoord);

    // Tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;

    // Get specular power
    float specularPower = normalData.a * 255;

    // Get specular intensity from the colorMap
    float specularIntensity = tex2D(colorSampler, texCoord).a;

    // Read depth
    float depthVal = tex2D(depthSampler,texCoord).r;

    // Compute screen-space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;

    // Transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    // Surface-to-light vector
    float3 lightVector = LightPosition - position;

    // Compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - length(lightVector)/LightRadius); 

    // Normalize light vector
    lightVector = normalize(lightVector); 

    // Compute diffuse light
    float NdL = max(0,dot(normal,lightVector));
    float3 diffuseLight = NdL * Color.rgb;

    // Reflection vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));

    // Camera-to-surface vector
    float3 directionToCamera = normalize(CameraPosition - position);

    // Compute specular light
    float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

    // Take into account attenuation and lightIntensity.
    return attenuation * LightIntensity * float4(diffuseLight.rgb,specularLight);
}

technique PointTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
