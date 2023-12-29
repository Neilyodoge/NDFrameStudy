#ifndef CC_HAIR_COMMON
#define CC_HAIR_COMMON

float Square( float x )
{
    return x*x;
}

float2 Square( float2 x )
{
    return x*x;
}

float3 Square( float3 x )
{
    return x*x;
}

float4 Square( float4 x )
{
    return x*x;
}

float Pow2( float x )
{
    return x*x;
}

float2 Pow2( float2 x )
{
    return x*x;
}

float3 Pow2( float3 x )
{
    return x*x;
}

float4 Pow2( float4 x )
{
    return x*x;
}

float Pow5( float x )
{
    float xx = x*x;
    return xx * xx * x;
}

float2 Pow5( float2 x )
{
    float2 xx = x*x;
    return xx * xx * x;
}

float3 Pow5( float3 x )
{
    float3 xx = x*x;
    return xx * xx * x;
}

float4 Pow5( float4 x )
{
    float4 xx = x*x;
    return xx * xx * x;
}

float CheapContrast(float input, half contrast)
{
    float val = lerp(-contrast, 1+contrast, input);
    return saturate(val);   
}

float3 VisualizeGreyScale(half value)
{
    if(value<0.5)
    {
        return lerp(float3(0, 0, 1), float3(1, 1, 0), value*2);
    }
    else
    {
        return lerp(float3(1, 1, 0), float3(1, 0, 0), value*2-1); 
    }
}

float random (float3 uv)
{
    return frac(sin(dot(uv,float3(210.123123, 12.9898,78.233)))*43758.5453123);
}

float random (float2 uv)
{
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}

void GetDistDepthConversion(float3 worldSpaceDir, out float dist2depth, out float depth2dist)
{
    float3 camForward = (mul((float3x3)unity_CameraToWorld, float3(0,0,1)));
    dist2depth = dot(worldSpaceDir,  camForward);
    depth2dist = 1.0/dist2depth;
}

float LinearDepthToZBuffer(float linearDepth, float4 zBufferParam)
{
    float res = (1.0 / linearDepth - zBufferParam.w) / zBufferParam.z;
  
    return res;
}

static int DITHER_SIZE_UNITY = 4;
static half DITHER_THRESHOLDS_UNITY[16] =
{
    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
};

static int DITHER_SIZE_UE = 5;
static half DITHER_THRESHOLDS_UE[25] =
{
    2.0/6,  4.0/6,  1.0/6,  3.0/6,  5.0/6,
    1.0/6,  3.0/6,  5.0/6,  2.0/6,  4.0/6,
    5.0/6,  2.0/6,  4.0/6,  1.0/6,  3.0/6,
    4.0/6,  1.0/6,  3.0/6,  5.0/6,  2.0/6,
    3.0/6,  5.0/6,  2.0/6,  4.0/6,  1.0/6
};
half Dither_Unity(half opacity, float2 screenUV)
{
    half2 uv = screenUV.xy * _ScreenParams.xy;
    float rand = random(uv);
    int index = (int(uv.x) % DITHER_SIZE_UNITY) * DITHER_SIZE_UNITY + int(uv.y) % DITHER_SIZE_UNITY;
    
    return opacity - DITHER_THRESHOLDS_UNITY[index];
}
half Dither_UE(half opacity, float2 screenUV)
{
    half2 uv = screenUV.xy * _ScreenParams.xy;
    float rand = random(uv);
    int index = (int(uv.x) % DITHER_SIZE_UE) * DITHER_SIZE_UE + int(uv.y) % DITHER_SIZE_UE;
    
    return opacity - DITHER_THRESHOLDS_UE[index] + (rand*2-1)*1.0/14;
}
half Dither(half opacity, float2 screenUV)
{
    return Dither_UE(opacity, screenUV);
}

static int DITHER_SIZE_WS = 3;
static int DITHER_SIZE_WS_SLICE = 9;
static half DITHER_THRESHOLDS_WS[27] =
{
    1, 2, 3, 4, 5, 6, 7, 8, 9,
    10, 11, 12, 13, 14, 15, 16, 17, 18,
    19, 20, 21, 22, 23, 24, 25, 26, 27
};

half Dither_WS(half opacity, float3 screenUV)
{
    half2 uv = screenUV.xy * _ScreenParams.xy;
    int index = (int(uv.x) % DITHER_SIZE_WS) * DITHER_SIZE_WS + int(uv.y) % DITHER_SIZE_WS ;
    index += (int(screenUV.z*100) % DITHER_SIZE_WS_SLICE) * DITHER_SIZE_WS_SLICE;
    
    return opacity - DITHER_THRESHOLDS_WS[index]/28.0;
}

void AlphaDither(half alpha, float2 screenUV)
{
#ifdef _ALPHATEST_ON
    clip(Dither(alpha, screenUV));
#endif
}


#endif
