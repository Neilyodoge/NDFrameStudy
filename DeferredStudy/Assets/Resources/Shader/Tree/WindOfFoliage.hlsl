#ifndef WINDOFFOLIAGE
#define WINDOFFOLIAGE

#if _WINDFIELD
#include "Packages/com.pwrd.windfield/Runtime/Shader/WindFieldCommon.hlsl"
#endif

/// 储存植被相关的风算法

#define UNITY_PI            3.14159265359f
#define UNITY_2_PI			6.28318530718f


float3 _Wind(float vertexColor, float3 worldPos, float offset,  float offsetScaler, float timeOffset, out float debug)
{
    float3 windOffset = float3(0, 0, 0);
    // x z 方向上的世界位置在映射到风的吹拂方向上的周期位置
    float spaceOffset = -worldPos.x * _WindDirection.x - worldPos.z * _WindDirection.z;
    // 获得相移(0,1)
    float weightedTime = fmod( (spaceOffset * _ModelScaleCorrection + _Time.y * _Frequency + timeOffset ), UNITY_2_PI);

    // 相移带入周期函数，计算随时间变化的风的波动
    float sway = sin(weightedTime);
    sway = (sway * 0.5 + 0.5) * _WindSineIntensity + offset ;
    //将风的波动分别带入
    //float yoffset = abs(cos(sway)) * _WindDirection.y;

    float horizontalOffset = sway;
    float xoffset = (horizontalOffset) * _WindDirection.x;
    float zoffset = (horizontalOffset) * _WindDirection.z;
    float yoffset = - pow(saturate(sway) + 0.001, 1.5);

    debug = sway * 0.5;

    //使用顶点色计算定点的偏移系数 effect ratio
    windOffset += float3(xoffset, yoffset , zoffset) * vertexColor * 0.2 * offsetScaler;
    //返回世界坐标的偏移量
    return windOffset;
}

// // 老版本的风
// float3 _Wind(float vertexColor, float3 worldPos, float offsetScaler)
// {
//     float3 windOffset = float3(0, 0, 0);
//     // x z 方向上的世界位置在映射到风的吹拂方向上的周期位置
//     float spaceOffset = -worldPos.x * _WindDirection.x - worldPos.z * _WindDirection.z;
//     // 获得相移(0,1)
//     float weightedTime = fmod(_Frequency * (spaceOffset * _ModelScaleCorrection + _Time.y), UNITY_PI);
//     // 相移带入周期函数，计算随时间变化的风的波动
//     //max(sin(t + 0.5 sin(t)), sin(t + π + sin(t + π)))
//     float sway = max(sin(weightedTime + 0.5 * sin(weightedTime)), sin(weightedTime + UNITY_PI + sin(weightedTime + UNITY_PI))) - 0.9;
//     //将风的波动分别带入
//     float yoffset = abs(cos(sway)) * _WindDirection.y;
//     float horizontalOffset = sin(sway);
//     float xoffset = horizontalOffset * _WindDirection.x;
//     float zoffset = horizontalOffset * _WindDirection.z;

//     //使用顶点色计算定点的偏移系数 effect ratio
//     windOffset += float3(xoffset, yoffset , zoffset) * vertexColor * _ModelScaleCorrection * offsetScaler;
//     //返回世界坐标的偏移量
//     return windOffset;
// }

//

// 计算风的世界坐标
float3 ApplyWind(float windMask, float3 worldPos, float windSineOffset, out float2 worldRotUV, out float windSpeed, out float debug)
{
    float2 windDir = normalize(_WindDirection.xz);
    windSpeed = length(_WindDirection.xz)  * 0.01;
    float2 worldUV = worldPos.xz;
    worldUV = float2(worldUV.x * windDir.x + worldUV.y * windDir.y,
                     worldUV.x * windDir.y - worldUV.y * windDir.x);

    worldRotUV = worldUV;

    worldUV += float2(-_Time.y  * _WindTexMoveSpeed, 0);
    worldUV *= windSpeed;

    worldUV /= UNITY_PI;
    worldUV *= _WindTexScale;


    float windTexOffset = SAMPLE_TEXTURE2D_LOD(_WindTex, sampler_WindTex, worldUV, 0).r;
    windTexOffset *= _WindTexIntensity;
    //float windSineOffset = length(positionOS.xz);



   // 把风权重图混合到风的计算中
    float3 ambientWindOffset = _Wind(windMask, worldPos, windTexOffset, _Magnitude, windSineOffset, debug);

#if _WINDFIELD
    float3 dynamicWindOffset = ApplyDynamicWind(worldPos, windMask);
    float3 grassCollisionOffset = ApplyGrassCollision(worldPos, windMask);
    worldPos += dynamicWindOffset;
    worldPos += grassCollisionOffset;
#endif
    return worldPos + ambientWindOffset;
}

#endif
