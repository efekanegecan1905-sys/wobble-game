using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Wobble.Core;
using Wobble.Save;
using Wobble.UI;
using Wobble.Gameplay;

namespace Wobble.Tests
{
    public sealed class MenuPlayModeTests
    {
        [UnityTest] public IEnumerator AllThemesDemoCanSelectAndPlayEveryTheme()
        {
            if (Resources.Load<TextAsset>("Demo/AllThemes") == null) Assert.Ignore("Only runs in the all-themes demo build copy.");
            var economy=(Wobble.Economy.EconomyManager)typeof(GameManager).GetField("economy",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
            var themes=(Wobble.Themes.ThemeManager)typeof(GameManager).GetField("themes",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
            Assert.That(economy.DemoMode,Is.True);
            foreach(var id in new[]{"Carrara","Obsidian","AncientStone","Neon","Space"})
            {
                Assert.That(Save.Data.ownedSkins,Does.Contain(id));
                ui.Menus.Open(MenuScreen.Themes);
                yield return null;
                var button=ui.Menus.Root.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Theme "+id);
                button.onClick.Invoke();
                Assert.That(Save.Data.selectedTheme,Is.EqualTo(id));
                Assert.That(themes.Current.id.ToString(),Is.EqualTo(id));
                ui.Menus.Back();
                yield return null;
                ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="home.play").onClick.Invoke();
                yield return new WaitForSecondsRealtime(.8f);
                Assert.That(game.State,Is.EqualTo(GameState.Playing));
                typeof(GameManager).GetMethod("GoHome",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,null);
                yield return null;
            }
        }
        [UnityTest] public IEnumerator DemoUnlocksAreAnimatedUnlimitedAndDoNotWritePlayerPrefs()
        {
            var before=PlayerPrefs.GetString("wobble.save.v1");
            var demoSave=new SaveManager(new DemoSaveStore());
            demoSave.Data.ownedSkins=new System.Collections.Generic.List<string>{"Carrara"};
            demoSave.Data.selectedTheme="Carrara";
            var balance=demoSave.Data.gems;
            var demoEconomy=new Wobble.Economy.EconomyManager(demoSave,true);
            var menu=new GameObject("Demo test menu").AddComponent<MenuScreens>();
            var audio=(Wobble.Audio.AudioManager)typeof(GameManager).GetField("audio",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
            var haptics=(Wobble.Audio.HapticManager)typeof(GameManager).GetField("haptics",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
            var themes=(Wobble.Themes.ThemeManager)typeof(GameManager).GetField("themes",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
            menu.Build(ui.Menus.Root.transform.parent,demoSave,demoEconomy,audio,haptics,ui.Localization,()=>{});
            menu.DemoThemeSelected+=theme=>themes.PreviewSurface(theme.id=="Carrara"?(Color?)null:theme.previewTint);
            menu.Open(MenuScreen.Themes);yield return new WaitForSecondsRealtime(.3f);
            foreach(var id in new[]{"Obsidian","AncientStone","Neon","Space"})
            {
                var button=menu.Root.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Theme "+id);
                button.onClick.Invoke();button.onClick.Invoke();
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(button.transform.localScale.x,Is.GreaterThan(1));
                Assert.That(demoSave.Data.ownedSkins.Contains(id),Is.False);
                yield return new WaitForSecondsRealtime(.6f);
                Assert.That(demoSave.Data.ownedSkins.Count(x=>x==id),Is.EqualTo(1));
                Assert.That(demoSave.Data.selectedTheme,Is.EqualTo(id));
                Assert.That(button.transform.localScale,Is.EqualTo(Vector3.one));
                var tint=themes.TopMaterial.color;themes.ApplyEnvironment(4,true);
                Assert.That(themes.TopMaterial.color,Is.EqualTo(tint));
            }
            Assert.That(demoEconomy.Gems,Is.EqualTo(int.MaxValue));
            Assert.That(demoSave.Data.gems,Is.EqualTo(balance));
            Assert.That(PlayerPrefs.GetString("wobble.save.v1"),Is.EqualTo(before));
            Object.Destroy(menu.Root);Object.Destroy(menu.gameObject);
        }
        private GameManager game;private UIManager ui;private bool hadSave;private string saved;private int quality;
        [UnitySetUp] public IEnumerator Setup()
        {
            hadSave=PlayerPrefs.HasKey("wobble.save.v1");saved=PlayerPrefs.GetString("wobble.save.v1");quality=QualitySettings.GetQualityLevel();
            var old=Object.FindFirstObjectByType<GameManager>();if(old!=null)Object.Destroy(old.gameObject);
            yield return null;PlayerPrefs.DeleteKey("wobble.save.v1");
            game=new GameObject("Menu integration test").AddComponent<GameManager>();ui=game.GetComponent<UIManager>();
            var until=Time.realtimeSinceStartup+15;
            while(!ui.Localization.Ready&&Time.realtimeSinceStartup<until)yield return null;
            Assert.That(ui.Localization.Ready,Is.True);
            ui.SkipIntro();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null)Object.Destroy(game.gameObject);yield return null;
            if(hadSave)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");
            PlayerPrefs.Save();QualitySettings.SetQualityLevel(quality,true);
        }
        private SaveManager Save => (SaveManager)typeof(GameManager).GetField("saves",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
        [UnityTest] public IEnumerator TurkishDefaultEnglishPersistsAndEveryScreenNavigates()
        {
            Assert.That(ui.Localization.LocaleCode,Is.EqualTo("tr"));Assert.That(ui.Localization.Text("home.play"),Is.EqualTo("OYNA"));
            foreach(var screen in new[]{MenuScreen.Themes,MenuScreen.Shop,MenuScreen.Daily,MenuScreen.Settings})
            {
                ui.Menus.Open(screen);yield return new WaitForSecondsRealtime(.3f);Assert.That(ui.Menus.Navigation.Current,Is.EqualTo(screen));
                ui.Menus.Back();yield return new WaitForSecondsRealtime(.3f);Assert.That(ui.Menus.Navigation.Current,Is.EqualTo(MenuScreen.Home));
            }
            ui.Menus.SelectLanguage("en");yield return null;
            Assert.That(ui.Localization.Text("home.play"),Is.EqualTo("PLAY"));Assert.That(new SaveManager().Data.selectedLocale,Is.EqualTo("en"));
            foreach(var key in new[]{"themes.title","shop.title","daily.title","settings.title","gameover.retry","currency.lives"})
                Assert.That(ui.Localization.Text(key),Is.Not.Empty);
            Object.Destroy(game.gameObject);yield return null;
            game=new GameObject("Relaunch").AddComponent<GameManager>();ui=game.GetComponent<UIManager>();
            yield return new WaitUntil(()=>ui.Localization.Ready);
            Assert.That(ui.Localization.LocaleCode,Is.EqualTo("en"));Assert.That(ui.Localization.Text("gameover.retry"),Is.EqualTo("RETRY"));
        }
        [UnityTest] public IEnumerator PlayRetryAndHomePreserveLivesAndApprovedGame()
        {
            var lives=Save.Data.lives;var gems=Save.Data.gems;
            var play=ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="home.play");play.onClick.Invoke();yield return new WaitForSecondsRealtime(.65f);
            Assert.That(game.State,Is.EqualTo(GameState.Playing));Assert.That(Save.Data.lives,Is.EqualTo(lives));Assert.That(Save.Data.gems,Is.EqualTo(gems));
            Assert.That(ui.Presentation.Camera.gameObject.activeInHierarchy,Is.False);
            var stack=(StackManager)typeof(GameManager).GetField("stack",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
            Assert.That(stack.ActiveBlock.Direction,Is.EqualTo(MoveDirection.PositiveX));
            stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight;stack.PlaceCurrent();
            Assert.That(game.Score,Is.EqualTo(2));Assert.That(Save.Data.bestScore,Is.EqualTo(2));
            ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="gameover.home").onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);
            Assert.That(game.State,Is.EqualTo(GameState.Home));Assert.That(ui.Menus.Navigation.Current,Is.EqualTo(MenuScreen.Home));
            Assert.That(ui.Presentation.Camera.enabled,Is.True);Assert.That(Save.Data.lives,Is.EqualTo(lives));
            ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="gameover.retry").onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.State,Is.EqualTo(GameState.Playing));Assert.That(Save.Data.lives,Is.EqualTo(lives));
        }
        [UnityTest] public IEnumerator CaptureEveryMenuInBothLanguagesAndPortraitSizes()
        {
            foreach(var language in new[]{"tr","en"})
            {
                ui.Menus.SelectLanguage(language);yield return null;
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1170,2532),new Vector2Int(1080,2400)})
                foreach(var screen in new[]{MenuScreen.Home,MenuScreen.Themes,MenuScreen.Shop,MenuScreen.Daily,MenuScreen.Settings})
                {
                    ui.Menus.Open(screen);yield return new WaitForSecondsRealtime(.26f);
                    yield return Capture(language+"-"+screen+"-"+size.x+"x"+size.y,size);
                }
            }
        }
        [UnityTest] public IEnumerator SettingsDailyAndPlaceholderActionsUseExistingSave()
        {
            ui.Menus.Open(MenuScreen.Settings);yield return new WaitForSecondsRealtime(.3f);
            foreach(var name in new[]{"settings.sound","settings.music","settings.haptics"})Click(name);
            Click("settings.quality");Click("quality.medium");
            Click("settings.language");Click("language.en");yield return null;
            var data=new SaveManager().Data;
            Assert.That(data.audioEnabled,Is.False);Assert.That(data.musicEnabled,Is.False);Assert.That(data.hapticsEnabled,Is.False);
            Assert.That(data.graphicsQuality,Is.EqualTo(1));Assert.That(QualitySettings.GetQualityLevel(),Is.EqualTo(1));
            Assert.That(data.selectedLocale,Is.EqualTo("en"));
            ui.Menus.Open(MenuScreen.Daily);yield return new WaitForSecondsRealtime(.3f);
            var gems=data.gems;Click("daily.claim");Click("daily.claim");
            Assert.That(new SaveManager().Data.gems,Is.EqualTo(gems+20));
            Assert.That(ui.GetComponentsInChildren<Button>().Single(b=>b.name=="daily.claim").interactable,Is.False);
            ui.Menus.Open(MenuScreen.Shop);yield return new WaitForSecondsRealtime(.3f);
            var product=ui.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.text==ui.Localization.Text("shop.starter_pack"));
            product.transform.parent.GetComponent<Button>().onClick.Invoke();yield return null;
            var purchase=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="purchase-unavailable");
            Assert.That(purchase.interactable,Is.False);purchase.onClick.Invoke();
            Assert.That(new SaveManager().Data.gems,Is.EqualTo(gems+20));Assert.That(new SaveManager().Data.lives,Is.EqualTo(data.lives));
        }
        private void Click(string name)=>ui.GetComponentsInChildren<Button>(true).Single(b=>b.name==name).onClick.Invoke();
        private IEnumerator Capture(string name,Vector2Int size)
        {
            var camera=ui.Presentation.Camera;var canvas=ui.GetComponentInChildren<Canvas>();
            var target=new RenderTexture(size.x,size.y,24){antiAliasing=4};camera.targetTexture=target;camera.aspect=size.x/(float)size.y;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            yield return null;Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.ForceMeshUpdate();
                Assert.That(text.text,Does.Not.Contain("No translation found"));
                Assert.That(text.isTextOverflowing,Is.False,"Overflow: "+name+" / "+text.text);
            }
            var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
            foreach(var icon in canvas.GetComponentsInChildren<MenuIcon>())
            {
                var corners=new Vector3[4];icon.rectTransform.GetWorldCorners(corners);
                var min=camera.WorldToScreenPoint(corners[0]);var max=camera.WorldToScreenPoint(corners[2]);
                var bright=false;
                for(var y=Mathf.Max(0,(int)min.y);y<Mathf.Min(size.y,(int)max.y);y++)
                for(var x=Mathf.Max(0,(int)min.x);x<Mathf.Min(size.x,(int)max.x);x++)
                    bright |= image.GetPixel(x,y).grayscale>.25f;
                Assert.That(bright,Is.True,"Invisible icon: "+name+" / "+icon.shape);
            }
            var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../Artifacts/Menu/Previews"));Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory,name+".png"),image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            Object.Destroy(image);target.Release();Object.Destroy(target);
        }
    }
}
