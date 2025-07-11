sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity; // Noise distortion factor
float uSaturation; // Desired edge width IN PIXELS
float uTime;
float4 uSourceRect;
float3 uLightSource;
float2 uImageSize0; // Use this for pixel conversion
float2 uImageSize1;
float4 uShaderSpecificData; // xy: noise speed + direction, zw: screen position

float2 uNode0 = float2(-1., -1.);
float2 uNode1 = float2(-1., -1.);
float2 uNode2 = float2(-1., -1.);
float2 uNode3 = float2(-1., -1.);
float2 uNode4 = float2(-1., -1.);

float2 uLine0 = float2(0., 0.);
float2 uLine1 = float2(0., 0.); 
float2 uLine2 = float2(0., 0.);
float2 uLine3 = float2(0., 0.);
float2 uLine4 = float2(0., 0.);

float distanceToLine(float2 p, float2 a, float2 b, out float2 perp) {
    float2 ab = b - a;
    float2 dir = normalize(ab);
    perp = float2(-dir.y, dir.x);
    float2 ap = p - a;
    float denom = dot(ab, ab);
    denom = max(denom, 0.000001);  // Avoid division by zero
    float t = clamp(dot(ap, ab) / denom, 0.0, 1.0);
    float2 closest = a + t * ab;
    return dot(p - closest, p - closest);
}

float2 getNodePosition(float index, float2 iSize) {
    // Create branchless node selection using weights
    float w0 = saturate(1.0 - abs(index - 0.0));
    float w1 = saturate(1.0 - abs(index - 1.0));
    float w2 = saturate(1.0 - abs(index - 2.0));
    float w3 = saturate(1.0 - abs(index - 3.0));
    float w4 = saturate(1.0 - abs(index - 4.0));
    
    // Normalize weights
    float sum = w0 + w1 + w2 + w3 + w4;
    w0 /= sum; w1 /= sum; w2 /= sum; w3 /= sum; w4 /= sum;
    
    return w0*(uNode0*iSize) + w1*(uNode1*iSize) + w2*(uNode2*iSize) + w3*(uNode3*iSize) + w4*(uNode4*iSize);
}

void processLine(float2 p, float2 line, float2 iSize, inout float minDist, inout float2 bestPerp) {
    // Skip invalid lines using math instead of conditionals
    float valid = 1.0 - step(abs(line.x - line.y), 0.0001);
    
    float2 a = getNodePosition(line.x, iSize);
    float2 b = getNodePosition(line.y, iSize);
    
    float2 perp;
    float dist = distanceToLine(p, a, b, perp);
    
    // Update minDist without conditionals
    float closer = step(dist, minDist) * valid;
    minDist = lerp(minDist, dist, closer);
    
    // Update perpendicular using component-wise lerp
    bestPerp.x = lerp(bestPerp.x, perp.x, closer);
    bestPerp.y = lerp(bestPerp.y, perp.y, closer);
}

float minLineDist(float2 p, out float2 perp, float2 iSize) {
    float minDist = 1e6;
    perp = float2(0.0, 1.0);
    
    // Process all lines unconditionally
    processLine(p, uLine0, iSize, minDist, perp);
    processLine(p, uLine1, iSize, minDist, perp);
    processLine(p, uLine2, iSize, minDist, perp);
    processLine(p, uLine3, iSize, minDist, perp);
    processLine(p, uLine4, iSize, minDist, perp);
    
    return minDist;
}

float4 Fill(float2 coords : TEXCOORD0) : COLOR0
{
    float2 iSize = uShaderSpecificData.zw;
    float2 noiseSpeed = uColor.xy;
    float2 screenPos = uShaderSpecificData.xy;
    float3 tex = tex2D(uImage0, coords - screenPos).xyz;
    float2 noise = tex2D(uImage1, coords + uTime * noiseSpeed * 10.).xx;
    
    float2 perp;
    float _ = minLineDist(coords, perp, iSize);
    
    float2 distortion = (perp * noise - 0.5) * 2.0 * (uOpacity / iSize);
    float2 distortedCoords = coords + distortion;
    
    float mdist = minLineDist(distortedCoords, perp, iSize);
    float close = step(mdist, uSaturation);
    
    return float4(tex, 1.0) * close;
}

technique Technique1
{
    pass Fill
    {
        PixelShader = compile ps_3_0 Fill();
    }
}