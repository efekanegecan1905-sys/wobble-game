Shader "WOBBLE/Atmosphere"
{
    Properties
    {
        _Top ("Upper atmosphere", Color) = (.025,.033,.043,1)
        _Horizon ("Soft horizon", Color) = (.13,.14,.145,1)
        _Bottom ("Lower atmosphere", Color) = (.04,.047,.055,1)
        _Accent ("Atmosphere accent", Color) = (1,1,1,1)
        _AccentStrength ("Atmosphere accent strength", Range(0,.15)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "ConceptSky.hlsl"
            struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
        float4 _Top, _Horizon, _Bottom;
            float4 _Accent;
            float _AccentStrength;
            v2f vert(float4 vertex:POSITION) { v2f o; o.pos=UnityObjectToClipPos(vertex); o.dir=vertex.xyz; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                return float4(WobbleSky(normalize(i.dir),_Top.rgb,_Horizon.rgb,_Bottom.rgb,_Accent.rgb,_AccentStrength),1);
            }
            ENDCG
        }
    }
}
