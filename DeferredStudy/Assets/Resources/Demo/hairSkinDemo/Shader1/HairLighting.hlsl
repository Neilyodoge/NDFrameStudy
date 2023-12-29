#ifndef PG_HAIR_LIGHTING_INCLUDE
#define PG_HAIR_LIGHTING_INCLUDE
// Marschner functions from UE4
float Hair_g(float B, float Theta)
{
    return exp(-0.5 * Pow2(Theta) / (B * B)) / (sqrt(2 * PI) * B);
}

float Hair_F(float CosTheta)
{
    const float n = 1.55;
    const float F0 = Pow2((1 - n) / (1 + n));
    return F0 + (1 - F0) * Pow5(1 - CosTheta);
}
// Reference: A Practical and Controllable Hair and Fur Model for Production Path Tracing.
float3 HairColorToAbsorption(float3 C, float B = 0.3f)
{
    const float b2 = B * B;
    const float b3 = B * b2;
    const float b4 = b2 * b2;
    const float b5 = B * b4;
    const float D = (5.969f - 0.215f * B + 2.532f * b2 - 10.73f * b3 + 5.574f * b4 + 0.245f * b5);
    return (log(C) / D)*(log(C) / D);
}

// N指向发根
float3 KajiyaKayDiffuseAttenuation(float3 albedo, float3 L, float3 V, half3 N, float Shadow)
{
    // Use soft Kajiya Kay diffuse attenuation
    float KajiyaDiffuse = 1 - abs(dot(N, L));

    float3 FakeNormal = normalize(V - N * dot(V, N));
    //N = normalize( DiffuseN + FakeNormal * 2 );
    N = FakeNormal;

    // Hack approximation for multiple scattering.
    float Wrap = 1;
    float NoL = saturate( (dot(N, L) + Wrap) / ((1 + Wrap)*(1 + Wrap)) );
    
    float DiffuseScatter = (1 / PI) * lerp(NoL, KajiyaDiffuse, 0.33) * saturate(_Scatter);

    //偏色效果
    float Luma = Luminance(albedo);
    //这里直接用power会出错
    float3 ScatterTint = SafePositivePow_float(albedo / Luma, 1 - Shadow);
    return sqrt(albedo) * DiffuseScatter * ScatterTint;
}

//math from UE4 source FastMath.ush
float acosFast(float inX) 
{
    float x = abs(inX);
    float res = -0.156583f * x + (0.5 * PI);
    res *= sqrt(1.0f - x);
    return (inX >= 0) ? res : PI - res;
}

float asinFast( float x )
{
    return (0.5 * PI) - acosFast(x);
}

// TODO: this is wrong, need farther investigation
// Main Hair Shading function from UE
float3 HairShading(float3 albedo, float3 L, float3 V, float3 N, float shadow, float roughnessBSq,
    float Area, float InBacklit)
{
    // 运算准备 //
    roughnessBSq = clamp(roughnessBSq, 1/(255.0*255.0), 1);
    //min(InBacklit, HairTransmittance.bUseBacklit ? GBuffer.CustomData.z : 1);
    const float Backlit	= InBacklit;
    
    const float VoL       = dot(V,L);                                                      
    const float SinThetaL = clamp(dot(N,L), -1.f, 1.f);
    const float SinThetaV = clamp(dot(N,V), -1.f, 1.f);
    float CosThetaD = cos( 0.5 * abs( asinFast( SinThetaV ) - asinFast( SinThetaL ) ) );

    const float3 Lp = L - SinThetaL * N;
    const float3 Vp = V - SinThetaV * N;
    const float CosPhi = dot(Lp,Vp) * rsqrt( dot(Lp,Lp) * dot(Vp,Vp) + 1e-4 );
    const float CosHalfPhi = sqrt( saturate( 0.5 + 0.5 * CosPhi ) );

    // 高光参数 //
    float n = 1.55;
    float n_prime = 1.19 / CosThetaD + 0.36 * CosThetaD;

    //TODO: 为了看着更相似改了这里，不过应该是不对的
    float Shift = -0.035;
    float Alpha[] =
    {
        -Shift * 2,
        Shift,
        Shift * 4,
    };	
    float B[] =
    {
        Area + roughnessBSq,
        Area + roughnessBSq / 2,
        Area + roughnessBSq * 2,
    };
    
    float3 S = 0;

    //计算R项高光
    {
        const float sa = sin(Alpha[0]);
        const float ca = cos(Alpha[0]);
        float Shift = 2 * sa * (ca * CosHalfPhi * sqrt(1 - SinThetaV * SinThetaV) + sa * SinThetaV);
        //HairTransmittance.bUseSeparableR ? sqrt(2.0) * CosHalfPhi : 1;
        float BScale =  sqrt(2.0) * CosHalfPhi;
        float Mp = Hair_g(B[0] * BScale, SinThetaL + SinThetaV - Shift);
        float Np = 0.25 * CosHalfPhi;
        float Fp = Hair_F(sqrt(saturate(0.5 + 0.5 * VoL)));
        //S += Mp * Np * Fp * (GBuffer.Specular * 2) * lerp(1, Backlit, saturate(-VoL));
        S += Mp * Np * Fp * (0.5 * 2) * lerp(1, Backlit, saturate(-VoL)) * _SpecularR_Intensity;
    }
    
    //计算TRT项
    {
        float Mp = Hair_g( B[2], SinThetaL + SinThetaV - Alpha[2] );
		
        //float h = 0.75;
        float f = Hair_F( CosThetaD * 0.5 );
        float Fp = Pow2(1 - f) * f;
        //float3 Tp = pow( GBuffer.BaseColor, 1.6 / CosThetaD );
        float3 Tp = pow( albedo, 0.8 / CosThetaD );

        //float s = 0.15;
        //float Np = 0.75 * exp( Phi / s ) / ( s * Pow2( 1 + exp( Phi / s ) ) );
        float Np = exp( 17 * CosPhi - 16.78 );

        S += Mp * Np * Fp * Tp * _SpecularTRT_Intensity;
    }

    //计算TT项
    {
        float Mp = Hair_g( B[1], SinThetaL + SinThetaV - Alpha[1] );

        float a = 1 / n_prime;
        //float h = CosHalfPhi * rsqrt( 1 + a*a - 2*a * sqrt( 0.5 - 0.5 * CosPhi ) );
        //float h = CosHalfPhi * ( ( 1 - Pow2( CosHalfPhi ) ) * a + 1 );
        float h = CosHalfPhi * ( 1 + a * ( 0.6 - 0.8 * CosPhi ) );
        //float h = 0.4;
        //float yi = asinFast(h);
        //float yt = asinFast(h / n_prime);
		
        float f = Hair_F( CosThetaD * sqrt( saturate( 1 - h*h ) ) );
        float Fp = (1 - f)*(1 - f);
        //float3 Tp = pow( GBuffer.BaseColor, 0.5 * ( 1 + cos(2*yt) ) / CosThetaD );
        //float3 Tp = pow( GBuffer.BaseColor, 0.5 * cos(yt) / CosThetaD );
        float3 Tp = 0;
        {
            // Compute absorption color which would match user intent after multiple scattering
            const float3 AbsorptionColor = HairColorToAbsorption(albedo);
            Tp = exp(-AbsorptionColor * 2 * abs(1 - Pow2(h * a) / CosThetaD));
        }

        //float t = asin( 1 / n_prime );
        //float d = ( sqrt(2) - t ) / ( 1 - t );
        //float s = -0.5 * PI * (1 - 1 / n_prime) * log( 2*d - 1 - 2 * sqrt( d * (d - 1) ) );
        //float s = 0.35;
        //float Np = exp( (Phi - PI) / s ) / ( s * Pow2( 1 + exp( (Phi - PI) / s ) ) );
        //float Np = 0.71 * exp( -1.65 * Pow2(Phi - PI) );
        float Np = exp( -3.65 * CosPhi - 3.98 );

        S += Mp * Np * Fp * Tp * Backlit * _SpecularTT_Intensity;
    }
    
    //计算散射项
    {
        S += KajiyaKayDiffuseAttenuation(albedo, L, V, N, shadow);
        //S += KajiyaKayDiffuseAttenuation(albedo, L, V, N, 1);
    }

    return S;
}
#endif
