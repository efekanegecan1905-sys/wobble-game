#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Wobble.Themes;

namespace Wobble.Editor
{
    public static class ProjectBuilder
    {
        [MenuItem("WOBBLE/Configure Project")]
        public static void Configure()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Resources/Themes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Wobble.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Wobble.unity", true) };

            PlayerSettings.productName = "WOBBLE";
            PlayerSettings.companyName = "WOBBLE Studio";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.wobble.game");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.wobble.game");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.iOS.targetOSVersionString = "13.0";

            CreateTheme("Carrara", ThemeId.Carrara, new Color(.92f,.90f,.85f), new Color(.89f,.87f,.825f), new Color(.025f,.03f,.045f), new Color(.94f,.88f,.75f));
            CreateTheme("Obsidian", ThemeId.Obsidian, new Color(.07f,.075f,.09f), new Color(.025f,.027f,.034f), new Color(.005f,.008f,.014f), new Color(.68f,.32f,1f));
            CreateTheme("Ancient Stone", ThemeId.AncientStone, new Color(.53f,.48f,.39f), new Color(.31f,.28f,.23f), new Color(.055f,.045f,.035f), new Color(1f,.64f,.26f));
            CreateTheme("Neon", ThemeId.Neon, new Color(.08f,.14f,.19f), new Color(.025f,.05f,.08f), new Color(.005f,.01f,.025f), new Color(.1f,1f,.82f));
            CreateTheme("Space", ThemeId.Space, new Color(.25f,.28f,.36f), new Color(.10f,.11f,.18f), new Color(.002f,.004f,.015f), new Color(.55f,.65f,1f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("WOBBLE project configured successfully.");
        }

        private static void CreateTheme(string displayName, ThemeId id, Color top, Color side, Color background, Color accent)
        {
            var fileName = id == ThemeId.AncientStone ? "AncientStone" : id.ToString();
            var path = $"Assets/Resources/Themes/{fileName}.asset";
            var theme = AssetDatabase.LoadAssetAtPath<WobbleTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<WobbleTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }
            theme.id = id; theme.displayName = displayName; theme.blockTop = top; theme.blockSide = side;
            theme.background = background; theme.fog = Color.Lerp(background, Color.gray, .12f); theme.accent = accent;
            EditorUtility.SetDirty(theme);
        }
    }
}
#endif
