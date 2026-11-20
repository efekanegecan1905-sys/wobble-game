using TMPro;
using UnityEngine;
namespace Wobble.UI
{
    // Separate assets: approved menu typography and shared font materials are untouched.
    public static class GameplayTypography
    {
        private static TMP_FontAsset display,hud;
        public static TMP_FontAsset Display=>display!=null?display:display=Resources.Load<TMP_FontAsset>("Gameplay/CelebrationFont");
        public static TMP_FontAsset Hud=>hud!=null?hud:hud=Resources.Load<TMP_FontAsset>("Gameplay/HudFont");
        public static void Apply(TextMeshProUGUI label,bool expressive=false)
        {
            if(label==null)return;
            // Reuse TMP's cached instance: outline setters restore it even after a
            // shared-material assignment. Refresh the atlas and metrics on that instance.
            var instance=label.fontMaterial;
            label.font=expressive?Display:Hud;
            instance.CopyPropertiesFromMaterial(label.font.material);
            label.fontSharedMaterial=instance;
            label.fontStyle=FontStyles.Normal;label.enableAutoSizing=false;
        }
    }
}
