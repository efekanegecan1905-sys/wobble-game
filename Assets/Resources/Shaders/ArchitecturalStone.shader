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
        _SunColor("Sun colour",Color)=(1,.96,.86,1)
        _SunDir("Sun direction",Vector)=(-.5,.55,.4,0)
        _SunIntensity("Sun intensity",Range(0,2))=1
        _CloudTint("Cloud tint",Color)=(1,1,1,1)
        _CloudStrength("Cloud strength",Range(0,2))=1
        _Stars("Star strength",Range(0,2))=0
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
                half4 _Accent, _SunColor, _CloudTint;
                float4 _SunDir;
                half _AccentStrength, _Visibility, _SunIntensity, _CloudStrength, _Stars;
                float _FogNear, _FogFar;
            CBUFFER_END
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            struct A { float4 p:POSITION;float3 n:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 p:SV_POSITION;float3 w:TEXCOORD0;half3 n:TEXCOORD1;half fog:TEXCOORD2; };
            V vert(A i) { UNITY_SETUP_INSTANCE_ID(i);V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o; }
            half4 frag(V i):SV_Target
            {
                half3 n = normalize(i.n);
                half light = .70 + .30 * saturate(dot(n, normalize(half3(-.35,.8,.25))));
                // A cool sky bounce and a warm sun kiss give flat props believable form.
                half skyBounce = saturate(n.y * .5 + .5);
                half3 ambient = lerp(_Color.rgb * .62, _Color.rgb * 1.05, skyBounce);
                half sun = saturate(dot(n, normalize(_SunDir.xyz)));
                half3 lit = ambient * light + _SunColor.rgb * pow(sun, 2.2) * .16 * _SunIntensity;
                half vein = sin(i.w.x * .32 + i.w.y * .73 + sin(i.w.z * .24)) * .018;
                half3 sky = WobbleSky(normalize(i.w - _WorldSpaceCameraPos), _SkyTop.rgb, _SkyHorizon.rgb, _SkyBottom.rgb,
                    _Accent.rgb, _AccentStrength, _SunDir.xyz, _SunColor.rgb, _SunIntensity, _CloudTint.rgb, _CloudStrength, _Stars);
                half3 grain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.w.x + i.w.z * .37, i.w.y) * .14).rgb;
                half3 stone = lit * lerp(half3(1,1,1), grain, .42) + vein;
                float depth = -TransformWorldToView(i.w).z;
                float fogVisibility = saturate((_FogFar - depth) / max(1, _FogFar - _FogNear));
                return half4(lerp(sky, stone, _Visibility * fogVisibility), 1);
            }
            ENDHLSL
        }
    }
}
