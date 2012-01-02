//
// Renders a directional light into GBuffer.
//

float3 LightDirection;			// Direction of the light
float LightIntensity;			// Intensity of the light
float3 Color;					// Colour of the light
float3 CameraPosition;			// Position of the camera, for specular light
float4x4 InvertViewProjection;	// This is used to compute the world-position
texture ColorMap;				// Diffuse color, and specularIntensity in the alpha channel
texture NormalMap;				// Normals, and specularPower in the alpha channel
texture DepthMap;				// Depth
float2 GBufferTextureSize;		// GBuffer size

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
    Texture = <DepthMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position		: POSITION0;
    float2 TexCoord		: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position		: POSITION0;
    float2 TexCoord		: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position,1);
	output.TexCoord = input.TexCoord - float2(-0.5f / GBufferTextureSize.xy);

    return output;
}

// Manually Linear Sample
float4 manualSample(sampler Sampler, float2 UV, float2 textureSize)
{
	float2 texelpos = textureSize * UV; 
	float2 lerps = frac(texelpos); 
	float texelSize = 1.0 / textureSize;                 
 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(Sampler, UV); 
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize)); 
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize));   
         
	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x), lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y); 

	return interpolated;
}

// Phong Shader
float4 Phong(float3 Position, float3 N, float SpecularIntensity, float SpecularPower)
{
	float3 L = LightDirection;

	// Calculate Reflection vector
	float3 R = normalize(reflect(L, N));

	// Calculate Eye vector
	float3 E = normalize(CameraPosition - Position.xyz);
	
	// Calculate N.L
	float NL = dot(N, -L);

	// Calculate Diffuse
	float3 Diffuse = NL * Color.xyz;

	// Calculate Specular
	float Specular = SpecularIntensity * pow(saturate(dot(R, E)), SpecularPower);

	// Calculate Final Product
	return LightIntensity * float4(Diffuse.rgb, Specular);
}

// Decoding of GBuffer Normals
float3 decode(float3 enc)
{
	return (2.0f * enc.xyz - 1.0f);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Get normal data from the normalMap
    float4 normalData = tex2D(normalSampler,input.TexCoord);

    // Tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz);

	// Get specular intensity from the colorMap
    float specularIntensity = tex2D(colorSampler, input.TexCoord).w;

    // Get specular power, and get it into [0,255] range]
    float specularPower = normalData.w * 255;
    
    // Read depth
	float depthVal = manualSample(depthSampler, input.TexCoord, GBufferTextureSize).x;

    // Compute screen-space position
    float4 position = 1.0f;
    position.x = input.TexCoord.x * 2.0f - 1.0f;
    position.y = -(input.TexCoord.x * 2.0f - 1.0f);
    position.z = depthVal;

    // Transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
    
    return Phong(position.xyz, normal, specularIntensity, specularPower);
}

technique DirectionalTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}