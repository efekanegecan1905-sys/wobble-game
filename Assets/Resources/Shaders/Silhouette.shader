Shader "WOBBLE/Silhouette"
{
    Properties { [MainColor] _Color ("Color", Color) = (.09,.095,.1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            struct V { float4 pos:SV_POSITION; half fog:TEXCOORD0; };
            V vert(float4 p:POSITION) { V o; o.pos=TransformObjectToHClip(p.xyz); o.fog=ComputeFogFactor(o.pos.z); return o; }
            half4 frag(V i):SV_Target { return half4(MixFog(_Color.rgb,i.fog),.12); }
            ENDHLSL
        }
    }
}
