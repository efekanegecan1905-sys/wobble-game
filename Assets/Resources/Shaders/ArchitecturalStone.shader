Shader "WOBBLE/ArchitecturalStone"
{
    Properties
    {
        _Color("Stone", Color)=(.22,.225,.22,1)
        _MainTex("Carrara grain",2D)="white" {}
        _Visibility("Visibility",Range(0,1))=1
        _SkyTop("Upper atmosphere",Color)=(.13,.14,.15,1)
        _SkyHorizon("Horizon",Color)=(.235,.245,.255,1)
        _SkyBottom("Lower atmosphere",Color)=(.16,.17,.18,1)
        _FogNear("Fog start",Float)=48
        _FogFar("Fog end",Float)=110
        _Accent("Atmosphere accent",Color)=(1,1,1,1)
        _AccentStrength("Atmosphere accent strength",Range(0,.15))=0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        ZWrite On
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "ConceptSky.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _SkyTop, _SkyHorizon, _SkyBottom;
                half4 _Accent;
                half _AccentStrength;
                half _Visibility;
                float _FogNear, _FogFar;
            CBUFFER_END
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            struct A { float4 p:POSITION;float3 n:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 p:SV_POSITION;float3 w:TEXCOORD0;half3 n:TEXCOORD1;half fog:TEXCOORD2; };
            V vert(A i) { UNITY_SETUP_INSTANCE_ID(i);V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o; }
            half4 frag(V i):SV_Target
            {
                half light=.68+.32*saturate(dot(normalize(i.n),normalize(half3(-.35,.8,.25))));
                half vein=sin(i.w.x*.32+i.w.y*.73+sin(i.w.z*.24))*.015;
                // Match the existing sky gradient: solid stone fades without ghost surfaces.
                half3 sky=WobbleSky(normalize(i.w-_WorldSpaceCameraPos),_SkyTop.rgb,_SkyHorizon.rgb,_SkyBottom.rgb,_Accent.rgb,_AccentStrength);
                half3 grain=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(i.w.x+i.w.z*.37,i.w.y)*.14).rgb;
                half3 stone=_Color.rgb*lerp(half3(1,1,1),grain,.42)*light+vein;
                float depth=-TransformWorldToView(i.w).z;
                float fogVisibility=saturate((_FogFar-depth)/max(1,_FogFar-_FogNear));
                return half4(lerp(sky,stone,_Visibility*fogVisibility),1);
            }
            ENDHLSL
        }
    }
}
