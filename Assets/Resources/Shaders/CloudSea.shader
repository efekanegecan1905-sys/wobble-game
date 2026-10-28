Shader "WOBBLE/CloudSea"
{
    Properties
    {
        _MainTex ("Cloud shape", 2D) = "white" {}
        _Color ("Cloud light", Color) = (1,1,1,1)
        _DeepColor ("Cloud shade", Color) = (.72,.80,.88,1)
        _Accent ("Sun accent", Color) = (1,.9,.7,1)
        _Opacity ("Opacity", Range(0,1)) = .85
        _Tiling ("Tiling", Float) = 5
        _SunDir ("Sun direction", Vector) = (-.5,.55,.4,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color, _DeepColor, _Accent;
                half _Opacity, _Tiling;
                float4 _SunDir;
            CBUFFER_END
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; float3 w:TEXCOORD1; };
            V vert(A i)
            {
                V o;
                o.w = TransformObjectToWorld(i.p.xyz);
                o.p = TransformWorldToHClip(o.w);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }
            half4 frag(V i):SV_Target
            {
                float2 drift = float2(_Time.y * .007, _Time.y * .0024);
                float a1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv * _Tiling + drift).a;
                float a2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv * _Tiling * 1.83 - drift * 1.35 + .37).a;
                float a = saturate(a1 * .70 + a2 * .42);
                float edge = smoothstep(0, .14, i.uv.x) * smoothstep(0, .14, 1 - i.uv.x)
                           * smoothstep(0, .10, i.uv.y) * smoothstep(0, .10, 1 - i.uv.y);
                half3 col = lerp(_DeepColor.rgb, _Color.rgb, a);
                // Punch a little more contrast into the puffs so the sea reads as clouds
                // rather than a flat white fog bank.
                col = saturate((col - .5h) * 1.12h + .5h);
                half sun = saturate(dot(normalize(i.w - _WorldSpaceCameraPos), normalize(_SunDir.xyz)));
                col += _Accent.rgb * pow(sun, 8) * .10;
                return half4(col, a * edge * _Opacity);
            }
            ENDHLSL
        }
    }
}
