 #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl" 

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
#include "Assets/Shader/ShaderLab/HLSL/Utility.hlsl"
#define RAYMARCH_MAXSTEP 100
#define RAYMARCH_SKINTHICK 0.01
#define RAYMARCH_MAXDIST 2
#define NORMAL_EPSILON 0.002

float SphereSDF (float3 pos,float r){
    return distance(pos,float3(0,0,0)) - r;
}

float SDF (float3 pos,float3 posOS,float offset){
    return distance(pos,posOS) - offset;
}

float sdCutHollowSphere(float3 p, float r, float h, float t )
{
  // sampling independent computations (only depend on shape)
  float w = sqrt(r*r-h*h);
  
  // sampling dependant computations
  float2 q = float2( length(p.xz), p.y );
  return ((h*q.x<w*q.y) ? length(q-float2(w,h)) : 
                          abs(length(q)-r) ) - t;
}

float CombinedSDF (float3 pos) 
{
    //return sdCutHollowSphere (pos, 1,0.9,0.01);
    return SphereSDF(pos,0.5);
}

half3 GetNormal(float3 pos, int world){
	float changeX = CombinedSDF(pos + float3(NORMAL_EPSILON, 0, 0)) - CombinedSDF(pos - float3(NORMAL_EPSILON, 0, 0));
	float changeY = CombinedSDF(pos + float3(0, NORMAL_EPSILON, 0)) - CombinedSDF(pos - float3(0, NORMAL_EPSILON, 0));
	float changeZ = CombinedSDF(pos + float3(0, 0, NORMAL_EPSILON)) - CombinedSDF(pos - float3(0, 0, NORMAL_EPSILON));
	float3 surfaceNormal = float3(changeX, changeY, changeZ);
	//float3 worldNormal = mul(UNITY_MATRIX_M, float4(surfaceNormal, 0)).xyz;
    return surfaceNormal;
	//return world == 1? normalize(worldNormal):normalize(surfaceNormal);
}

struct Attributes{
    float3 positionOS : POSITION;     
    float2 uv : TEXCOORD0;
};

struct Interpolators{
    float4 positionCS : SV_POSITION;
    float3 positionOS : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
    float3 viewDirectionOS :TEXCOORD5;
    float3 cameraDirectionWS :TEXCOORD6;
    float2 uv : TEXCOORD0;  
};

Interpolators Vert(Attributes input){
    Interpolators output;
    VertexPositionInputs posInputs = GetVertexPositionInputs (input.positionOS);
    output.positionOS = input.positionOS;
    output.positionCS = posInputs.positionCS;
    output.positionWS = posInputs.positionWS;
    output.cameraDirectionWS =_WorldSpaceCameraPos - output.positionWS ;
    float3 objectSpaceCameraPos = mul (UNITY_MATRIX_M,float4 (_WorldSpaceCameraPos,1)).xyz;
    output.viewDirectionOS = output.positionOS - objectSpaceCameraPos;
    output.uv = input.uv;
    return output;
};


float3 Raymarch (float3 ro, float3 rd){
    float dist = 0;
    for (int i = 0; i < RAYMARCH_MAXSTEP; i++){
        float hit = CombinedSDF(ro + rd * dist);
        dist += hit;
        if (hit < RAYMARCH_SKINTHICK ) {
            return  ro + rd * dist;
        }
        if (dist > RAYMARCH_MAXDIST) 
        {
            clip(0.9);
            return 0;
        }
    }   
    return  ro + rd * dist;
}


half4 Frag(Interpolators input): SV_TARGET {

    float3 camRayDirOS = normalize( input.viewDirectionOS);
    float3 posOS = input.positionOS;

    half4 finalColor = half4 (1,1,1,1);
    float3 rmNormalOS = GetNormal(Raymarch(posOS, camRayDirOS),0);
    
    finalColor = rmNormalOS.xyzz;
    return finalColor;
};