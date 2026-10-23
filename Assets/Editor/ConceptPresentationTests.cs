#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using TMPro;
using Wobble.Themes;
namespace Wobble.Editor.Tests
{
    public sealed class ConceptPresentationTests
    {
        [Test] public void ConceptPaletteAndTypeArePresent()
        {
            Assert.That(WobblePalette.SkyTop.b,Is.GreaterThan(WobblePalette.SkyTop.r));
            Assert.That(WobblePalette.Surface.grayscale,Is.GreaterThan(.85f));
            Assert.That(WobblePalette.Primary.r,Is.GreaterThan(WobblePalette.Primary.b));
            Assert.That(Resources.Load<TMP_FontAsset>("Menu/WobbleDisplay"),Is.Not.Null);
            Assert.That(Resources.Load<TMP_FontAsset>("Menu/WobbleBody"),Is.Not.Null);
        }
        [Test] public void EnvironmentRemainsBrightAcrossAllFourLevels()
        {
            var previousSky=RenderSettings.skybox;var previousFog=RenderSettings.fogColor;
            var previousAmbient=RenderSettings.ambientSkyColor;var equator=RenderSettings.ambientEquatorColor;var ground=RenderSettings.ambientGroundColor;
            var fogEnabled=RenderSettings.fog;var fogMode=RenderSettings.fogMode;var ambientMode=RenderSettings.ambientMode;
            var near=RenderSettings.fogStartDistance;var far=RenderSettings.fogEndDistance;
            var theme=new ThemeManager();var runtimeSky=RenderSettings.skybox;
            try
            {
                for(var level=1;level<=4;level++)
                {
                    theme.ApplyEnvironment(level,true);
                    Assert.That(RenderSettings.fogColor.grayscale,Is.GreaterThan(.7f));
                    Assert.That(RenderSettings.skybox.GetColor("_Top").b,Is.GreaterThan(.6f));
                }
            }
            finally
            {
                RenderSettings.skybox=previousSky;RenderSettings.fogColor=previousFog;RenderSettings.ambientSkyColor=previousAmbient;
                RenderSettings.ambientEquatorColor=equator;RenderSettings.ambientGroundColor=ground;RenderSettings.ambientMode=ambientMode;
                RenderSettings.fog=fogEnabled;RenderSettings.fogMode=fogMode;RenderSettings.fogStartDistance=near;RenderSettings.fogEndDistance=far;
                Object.DestroyImmediate(theme.TopMaterial);Object.DestroyImmediate(theme.BlockMaterial);Object.DestroyImmediate(runtimeSky);
            }
        }
    }
}
#endif
