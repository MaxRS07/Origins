sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uOffset;
float uScale;
float4 uShaderSpecificData;
float4 uSourceRect0;
float4 uSourceRect1;
float4 uAlphaMatrix0;
float4 uAlphaMatrix1;

const float PI = 3.14159265359;
float arctan2(float2 offset) {
	return offset.y / (sqrt(offset.x * offset.x + offset.y * offset.y) + offset.x);
}
float mod(float x) {
	return x - floor(x);
}

float4 Crown_Jewel_Caustics(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	float2 offset = (uv - float2(0.5, 0.5));
	float f = 0;
	float distance = sqrt(offset.x * offset.x + offset.y * offset.y) * 6;
	float angle = arctan2(offset) / (PI * 2) + 0.5;
	distance /= tex2D(uImage0, float2(mod(uTime * 0.05), angle)).r + 1;
	return color
	* tex2D(uImage0, float2(mod(uv.x * 2 + uTime * 0.05), mod(uv.y * 2))).r
	* tex2D(uImage0, float2(mod(uv.x * 2), mod(uv.y * 2 + uTime * 0.05))).r
	* max(1 - distance, 0);
}

technique Technique1 {
	pass Crown_Jewel_Caustics {
		PixelShader = compile ps_3_0 Crown_Jewel_Caustics();
	}
}
