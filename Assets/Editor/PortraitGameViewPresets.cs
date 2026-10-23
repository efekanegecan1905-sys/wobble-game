#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

namespace Wobble.Editor
{
    public static class PortraitGameViewPresets
    {
        [MenuItem("WOBBLE/Rendering/Install Portrait Game View Presets")]
        public static void Install()
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var kind = assembly.GetType("UnityEditor.GameViewSizeType");
            var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
            foreach (var groupName in new[] { "Standalone", "iOS", "Android" })
            {
                var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, groupName) });
                var display = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null);
                foreach (var resolution in new[] { (1080, 1920), (1170, 2532), (1080, 2400) })
                {
                    var label = $"WOBBLE {resolution.Item1} × {resolution.Item2}";
                    if (Array.Exists(display, text => text.StartsWith(label))) continue;
                    var size = Activator.CreateInstance(sizeType, new[] { Enum.ToObject(kind, 1), (object)resolution.Item1, resolution.Item2, label });
                    group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
                }
            }
            sizesType.GetMethod("SaveToHDD").Invoke(sizes, null);
        }
    }
}
#endif
