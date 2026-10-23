#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wobble.Editor
{
    // Explicit, repeatable rendering configuration. Never rebuilds or saves a scene.
    public static class FidelityBuilder
    {
        private const string Root = "Assets/Resources/Rendering";

        [MenuItem("WOBBLE/Rendering/Configure Fidelity Assets")]
        public static void Configure()
        {
            Directory.CreateDirectory(Root);
            AssetDatabase.Refresh();
            var quality = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
            var tiers = quality.FindProperty("m_QualitySettings");
            tiers.arraySize = 3;
            var names = new[] { "Low", "Medium", "High" };
            for (var i = 0; i < 3; i++) tiers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = names[i];
            quality.ApplyModifiedPropertiesWithoutUndo();
            for (var i = 0; i < 3; i++)
            {
                var asset = CreateTier(names[i], i);
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = asset;
                QualitySettings.antiAliasing = i == 0 ? 2 : 4;
                QualitySettings.shadowResolution = i == 2 ? UnityEngine.ShadowResolution.High : i == 1 ? UnityEngine.ShadowResolution.Medium : UnityEngine.ShadowResolution.Low;
                QualitySettings.shadowDistance = 48;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                QualitySettings.vSyncCount = 0;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.resolutionScalingFixedDPIFactor = 1f;
            }
            QualitySettings.SetQualityLevel(2, true);
            GraphicsSettings.defaultRenderPipeline = QualitySettings.renderPipeline;
            // URP 17 keeps this initialization helper internal; invoke its own
            // resource initializer rather than inventing global-settings YAML.
            typeof(UniversalRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings")?.GetMethod("Ensure", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, new object[] { true });
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.useHDRDisplay = false;
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var resolutionMode = player.FindProperty("resolutionScalingMode");
            if (resolutionMode != null) resolutionMode.intValue = 0;
            player.ApplyModifiedPropertiesWithoutUndo();
            quality.Update();
            var defaults = quality.FindProperty("m_PerPlatformDefaultQuality");
            for (var i = 0; i < defaults.arraySize; i++) defaults.GetArrayElementAtIndex(i).FindPropertyRelative("second").intValue = 2;
            quality.ApplyModifiedPropertiesWithoutUndo();
            CreateProfile();
            ImportFont();
            MarbleTextureBaker.Bake();
            PortraitGameViewPresets.Install();
            AssetDatabase.SaveAssets();
            Debug.Log("WOBBLE fidelity assets configured: URP, Linear, 1.0 render scale, High 4x MSAA.");
        }

        private static UniversalRenderPipelineAsset CreateTier(string name, int tier)
        {
            var rendererPath = $"{Root}/{name}Renderer.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            renderer.renderingMode = RenderingMode.Forward;
            renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if (tier > 0 && renderer.rendererFeatures.Count == 0)
            {
                var ao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                ao.name = "Subtle stone contact AO";
                AssetDatabase.AddObjectToAsset(ao, renderer);
                renderer.rendererFeatures.Add(ao);
                var settings = new SerializedObject(ao);
                var config = settings.FindProperty("m_Settings");
                config.FindPropertyRelative("Intensity").floatValue = tier == 2 ? .38f : .24f;
                config.FindPropertyRelative("Radius").floatValue = .13f;
                config.FindPropertyRelative("DirectLightingStrength").floatValue = .12f;
                config.FindPropertyRelative("Downsample").boolValue = true;
                config.FindPropertyRelative("Source").enumValueIndex = 0; // Reconstruct normals from depth.
                config.FindPropertyRelative("NormalSamples").enumValueIndex = 2;
                config.FindPropertyRelative("Samples").enumValueIndex = tier == 2 ? 1 : 2;
                config.FindPropertyRelative("AOMethod").enumValueIndex = 1;
                config.FindPropertyRelative("BlurQuality").enumValueIndex = tier == 2 ? 0 : 1;
                config.FindPropertyRelative("Falloff").floatValue = 45f;
                settings.ApplyModifiedPropertiesWithoutUndo();
                ao.Create();
                EditorUtility.SetDirty(ao);
            }
            renderer.SetDirty();
            if (!renderer.rendererFeatures.Exists(feature => feature is Wobble.CameraSystem.FidelityRenderProbe))
            {
                var probe = ScriptableObject.CreateInstance<Wobble.CameraSystem.FidelityRenderProbe>();
                probe.name = "Native resolution verification";
                AssetDatabase.AddObjectToAsset(probe, renderer);
                renderer.rendererFeatures.Add(probe);
            }
            EditorUtility.SetDirty(renderer);
            var path = $"{Root}/{name}.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, path);
            }
            pipeline.renderScale = 1f;
            pipeline.msaaSampleCount = tier == 0 ? 2 : 4;
            pipeline.supportsHDR = true;
            pipeline.supportsCameraDepthTexture = tier > 0;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.mainLightShadowmapResolution = tier == 2 ? 2048 : tier == 1 ? 1024 : 512;
            pipeline.shadowDistance = 48f;
            pipeline.shadowCascadeCount = 2;
            pipeline.cascade2Split = .6f;
            pipeline.shadowDepthBias = .35f;
            pipeline.shadowNormalBias = .3f;
            pipeline.useSRPBatcher = true;
            pipeline.maxAdditionalLightsCount = 1;
            var serialized = new SerializedObject(pipeline);
            serialized.FindProperty("m_SoftShadowsSupported").boolValue = true;
            serialized.FindProperty("m_SoftShadowQuality").enumValueIndex = tier == 2 ? 2 : 1;
            serialized.FindProperty("m_AdditionalLightShadowsSupported").boolValue = false;
            serialized.FindProperty("m_UseAdaptivePerformance").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            return pipeline;
        }

        private static void CreateProfile()
        {
            var path = $"{Root}/CarraraPost.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile != null) return;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(.15f);
            color.contrast.Override(3f);
            color.saturation.Override(-4f);
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.15f);
            bloom.intensity.Override(.12f);
            bloom.scatter.Override(.35f);
            bloom.highQualityFiltering.Override(true);
            foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(profile);
        }

        private static void ImportFont()
        {
            const string path = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path) == null)
            {
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_FontAsset).Assembly);
                AssetDatabase.ImportPackage(Path.Combine(package.resolvedPath, "Package Resources/TMP Essential Resources.unitypackage"), false);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            var fontPath = $"{Root}/WobbleSans.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath) != null) return;
            if (source == null) throw new InvalidOperationException("TMP's licensed Liberation Sans source font was not imported.");
            var font = TMP_FontAsset.CreateFontAsset(source, 96, 12, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
            font.name = "Wobble Sans SDF";
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ×.:-+!/?()", out var missing);
            if (!string.IsNullOrEmpty(missing)) throw new InvalidOperationException("Missing UI glyphs: " + missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font, fontPath);
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            EditorUtility.SetDirty(font);
        }
    }
}
#endif
