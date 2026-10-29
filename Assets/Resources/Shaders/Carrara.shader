Shader "WOBBLE/Carrara"
{
    Properties
    {
        [MainColor] _Color ("Stone tint", Color) = (.92,.9,.85,1)
        [MainTexture] _MainTex ("Veins and roughness", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = .27
        _Metallic ("Metallic", Range(0,1)) = 0
        _Flash ("Perfect flash", Float) = 0
        _Sweep ("Perfect sweep", Float) = -1
        _RimColor ("Theme rim", Color) = (0,0,0,1)
        _RimStrength ("Theme rim strength", Float) = 0
        _StoneOrigin ("Stone coordinates", Vector) = (0,0,0,0)
        _StoneSize ("Metric size", Vector) = (1,1,1,0)
        _EdgeLight ("Edge light", Range(0,1)) = .16
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _Color, _StoneOrigin, _StoneSize;
            float4 _RimColor;
            float _Glossiness, _Metallic, _Flash, _Sweep, _RimStrength, _EdgeLight;
        CBUFFER_END
        TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 stone : TEXCOORD2;
            float3 normalOS : TEXCOORD3;
            float2 localTop : TEXCOORD4;
            half fog : TEXCOORD5;
            float localY : TEXCOORD6;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings vert(Attributes v)
        {
            Varyings o = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
            o.positionCS = p.positionCS;
            o.positionWS = p.positionWS;
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            // Object-attached metric coordinates preserve scale through cuts and falls.
            o.stone = (v.positionOS.xyz * _StoneSize.xyz + _StoneOrigin.xyz) * .18;
            o.normalOS = v.normalOS;
            o.localTop = v.positionOS.xz + .5;
            o.localY = v.positionOS.y + .5;
            o.fog = ComputeFogFactor(p.positionCS.z);
            return o;
        }
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            half4 frag(Varyings i) : SV_Target
            {
                float3 weight = pow(abs(normalize(i.normalOS)), 6);
                weight /= max(dot(weight, float3(1,1,1)), .0001);
                half4 stone = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.stone.zy) * weight.x
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.stone.xz) * weight.y
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.stone.xy) * weight.z;
                InputData data = (InputData)0;
                data.positionWS = i.positionWS;
                data.normalWS = normalize(i.normalWS);
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                data.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                data.bakedGI = SampleSH(data.normalWS);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                data.shadowMask = half4(1,1,1,1);
                SurfaceData surface = (SurfaceData)0;
                // Face separation: the cap catches more sky, the sides read as a
                // slightly richer, darker stone so each slab has a clear silhouette.
                half upFacing = saturate(normalize(i.normalOS).y);
                half3 albedo = stone.rgb * _Color.rgb;
                albedo *= lerp(half(0.93), half(1.06), upFacing);
                albedo *= lerp(half(0.965), half(1.035), saturate(i.localY));
                surface.albedo = albedo;
                surface.metallic = 0;
                surface.specular = half3(.04,.04,.04);
                surface.smoothness = _Glossiness * (.80 + .18 * dot(stone.rgb, half3(.333,.333,.334)));
                surface.occlusion = 1;
                surface.alpha = 1;
                surface.normalTS = half3(0,0,1);
                float sweep = (1 - smoothstep(.005, .028, abs(i.localTop.x - _Sweep))) * step(0, _Sweep);
                // One cheap Fresnel term gives dark themes a readable neon edge with no lights.
                half fresnel = pow(1 - saturate(dot(normalize(i.normalWS), GetWorldSpaceNormalizeViewDir(i.positionWS))), 4);
                // The chamfer ring is 45 degrees, so a narrow band around that normal
                // gives the slab a lit soft bevel without extra geometry.
                half chamfer = smoothstep(.50, .70, abs(i.normalOS.y)) * (1 - smoothstep(.84, .98, abs(i.normalOS.y)));
                surface.emission = half3(1,.95,.86) * (sweep * 2.2 + _Flash * .2) * upFacing
                    + _RimColor.rgb * fresnel * _RimStrength
                    + half3(1,.97,.91) * chamfer * _EdgeLight * (0.55 + 0.45 * upFacing);
                half4 color = UniversalFragmentPBR(data, surface);
                color.rgb = MixFog(color.rgb, i.fog);
                return color;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment depthFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 _LightDirection;
            float3 _LightPosition;
            float4 shadowVert(Attributes v) : SV_POSITION
            {
                float3 p = TransformObjectToWorld(v.positionOS.xyz);
                float3 n = TransformObjectToWorldNormal(v.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDir = normalize(_LightPosition - p);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                float4 clip = TransformWorldToHClip(ApplyShadowBias(p, n, lightDir));
                #if UNITY_REVERSED_Z
                    clip.z = min(clip.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    clip.z = max(clip.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return clip;
            }
            half4 depthFrag() : SV_Target { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment depthFrag
            half depthFrag(Varyings i) : SV_Target { return i.positionCS.z; }
            ENDHLSL
        }
    }
}
