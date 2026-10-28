Shader "WOBBLE/Spark"
{
    Properties
    {
        _MainTex ("Shape", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Intensity;
            CBUFFER_END
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            V vert(A i) { V o; o.p=TransformObjectToHClip(i.p.xyz); o.uv=TRANSFORM_TEX(i.uv,_MainTex); o.color=i.color; return o; }
            half4 frag(V i):SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half a = t.a * _Color.a * _Intensity * i.color.a;
                return half4(_Color.rgb * t.rgb * i.color.rgb * a, 1);
            }
            ENDHLSL
        }
    }
}
