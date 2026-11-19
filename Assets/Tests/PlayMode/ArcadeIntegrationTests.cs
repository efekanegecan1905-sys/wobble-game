using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Save;
using Wobble.UI;
namespace Wobble.Tests
{
    public class ArcadeIntegrationTests
    {
        private GameManager game;private string saved;private bool had;
#if UNITY_EDITOR
        private UnityEditor.EditorWindow captureView;private PropertyInfo captureSize;private int originalSize;
#endif
        private T Field<T>(string name)=>(T)typeof(GameManager).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            had=PlayerPrefs.HasKey("wobble.save.v1");saved=PlayerPrefs.GetString("wobble.save.v1");
#if UNITY_EDITOR
            var viewType=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            captureView=UnityEditor.EditorWindow.GetWindow(viewType,false,null,false);
            captureSize=viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            originalSize=(int)captureSize.GetValue(captureView);
#endif
            var old=Object.FindFirstObjectByType<GameManager>();if(old!=null)Object.Destroy(old.gameObject);yield return null;
            game=new GameObject("Arcade QA").AddComponent<GameManager>();
            yield return new WaitUntil(()=>game.GetComponent<UIManager>().Localization.Ready);game.GetComponent<UIManager>().SkipIntro();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {Object.Destroy(game.gameObject);yield return null;
#if UNITY_EDITOR
            if(captureView!=null){captureSize.SetValue(captureView,originalSize);captureView.Repaint();yield return null;}
#endif
            if(had)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();}
        private void Perfect()
        {var s=Field<StackManager>("stack");s.ActiveBlock.transform.position=s.Top.transform.position+Vector3.up*StackManager.BlockHeight;s.PlaceCurrent();}
        private void Miss()
        {var s=Field<StackManager>("stack");s.ActiveBlock.transform.position=s.Top.transform.position+s.ActiveBlock.Axis*30;s.PlaceCurrent();}
        private IEnumerator Preview(string name,int width=1080,int height=1920,bool checkMenu=false)
        {
            System.IO.Directory.CreateDirectory("Artifacts/ArcadeV2");
#if UNITY_EDITOR
            // Capture the composited Game View: overlay UI must bypass world tone mapping.
            var assembly=typeof(UnityEditor.Editor).Assembly;
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
            var groupType=sizesType.GetProperty("currentGroupType",flags).GetValue(sizes);
            var sizeGroup=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});
            string captureLabel="WOBBLE QA "+width+"x"+height;
            var labels=(string[])sizeGroup.GetType().GetMethod("GetDisplayTexts").Invoke(sizeGroup,null);
            int index=System.Array.FindIndex(labels,x=>x.StartsWith(captureLabel));
            if(index<0)
            {
                var kind=assembly.GetType("UnityEditor.GameViewSizeType");
                var size=System.Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),new[]{System.Enum.ToObject(kind,1),(object)width,height,captureLabel});
                sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{size});index=labels.Length;
            }
            var viewType=assembly.GetType("UnityEditor.GameView");
            var view=captureView;view.Show();
            if((int)captureSize.GetValue(view)!=index)captureSize.SetValue(view,index);
            for(float elapsed=0;elapsed<.65f;elapsed+=Time.unscaledDeltaTime){view.Repaint();yield return null;}
            Canvas.ForceUpdateCanvases();
            var target=(RenderTexture)assembly.GetType("UnityEditor.PlayModeView").GetField("m_TargetTexture",flags).GetValue(view);
            Assert.IsNotNull(target,"Game View did not render");Assert.AreEqual(width,target.width);Assert.AreEqual(height,target.height);
#else
            yield return new WaitForEndOfFrame();
            var target=new RenderTexture(Screen.width,Screen.height,24);target.Create();ScreenCapture.CaptureScreenshotIntoRenderTexture(target);
            width=target.width;height=target.height;
#endif
            if(checkMenu)
            {
                var modeUI=game.GetComponent<ArcadeModeUI>();
                foreach(var button in modeUI.GetComponentsInChildren<UnityEngine.UI.Button>())
                {
                    var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
                    foreach(var corner in corners){var p=RectTransformUtility.WorldToScreenPoint(null,corner);p=new Vector2(p.x/Screen.width,p.y/Screen.height);Assert.That(p.x,Is.InRange(0f,1f),button.name);Assert.That(p.y,Is.InRange(0f,1f),button.name);}
                }
                foreach(var label in modeUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
                {
                    label.ForceMeshUpdate();Assert.IsFalse(label.isTextOverflowing,label.text);
                    foreach(char c in label.GetParsedText())if(!char.IsWhiteSpace(c))Assert.IsTrue(label.font.HasCharacter(c),"Unbaked arcade glyph: "+c);
                }
            }
            var previous=RenderTexture.active;var output=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32);
            if(SystemInfo.graphicsUVStartsAtTop)Graphics.Blit(target,output,new Vector2(1,-1),new Vector2(0,1));else Graphics.Blit(target,output);
            RenderTexture.active=output;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            System.IO.File.WriteAllBytes("Artifacts/ArcadeV2/"+name+".png",image.EncodeToPNG());RenderTexture.active=previous;
            Object.Destroy(image);RenderTexture.ReleaseTemporary(output);
#if !UNITY_EDITOR
            target.Release();Object.Destroy(target);
#endif
        }
        [UnityTest] public IEnumerator RushHasFreeMissesAndSeparateRecord()
        {
            var s=Field<SaveManager>("saves");int lives=s.Data.lives,best=s.Data.bestScore;game.SelectMode(ArcadeMode.Rush);
            Perfect();Assert.Greater(game.Score,0);yield return Preview("Rush");
            Miss();Miss();Assert.AreEqual(GameState.Playing,game.State);Miss();Assert.AreEqual(GameState.GameOver,game.State);
            Assert.AreEqual(lives,s.Data.lives);Assert.AreEqual(best,s.Data.bestScore);Assert.GreaterOrEqual(s.Data.rushBest,game.Score);
        }
        [UnityTest] public IEnumerator BalanceUsesActualPlacementOffset()
        {
            game.SelectMode(ArcadeMode.Balance);var s=Field<StackManager>("stack");
            s.ActiveBlock.transform.position=s.Top.transform.position+Vector3.up*StackManager.BlockHeight+Vector3.right*.4f;s.PlaceCurrent();
            Assert.Greater(game.Arcade.Balance.x,0);float before=game.Arcade.Balance.magnitude;Perfect();Assert.Less(game.Arcade.Balance.magnitude,before);
            yield return Preview("Balance");Miss();Assert.AreEqual(GameState.GameOver,game.State);
        }
        [UnityTest] public IEnumerator LocalizedMenusFitPortraitAndTablet()
        {
            var saves=Field<SaveManager>("saves");var ui=game.GetComponent<ArcadeModeUI>();
            foreach(var locale in new[]{"tr","en"})
            {
                saves.Data.selectedLocale=locale;ui.Open();
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1170,2532),new Vector2Int(1080,2400)})
                for(int mode=0;mode<4;mode++)
                {ui.SelectPreview(mode);yield return new WaitForSecondsRealtime(1.1f);yield return Preview("Showcase-"+mode+"-"+locale+"-"+size.y,size.x,size.y,true);}
                ui.SelectPreview(3);ui.PlaySelected();yield return Preview("TrialLevels-"+locale,1080,1920,true);
                ui.Open();yield return Preview("Selection-tablet-"+locale,1536,2048,true);
            }
        }
        [UnityTest] public IEnumerator BalanceLeanNeverChangesStackingCollider()
        {
            game.SelectMode(ArcadeMode.Balance);var stack=Field<StackManager>("stack");
            var top=stack.Top;var box=top.GetComponent<BoxCollider>();var bounds=box.bounds;
            stack.SetBalance(new Vector2(.9f,-.7f));yield return new WaitForSeconds(.25f);Physics.SyncTransforms();
            Assert.Greater(stack.VisualBalance.magnitude,.2f);
            Assert.AreEqual(Quaternion.identity,top.transform.localRotation);
            Assert.AreEqual(bounds.center,box.bounds.center);Assert.AreEqual(bounds.size,box.bounds.size);
            game.SelectMode(ArcadeMode.Classic);Perfect();Assert.AreEqual(2,game.Score);
            Assert.AreEqual(Vector2.zero,stack.Top.ArcadeBalance);
        }
        [UnityTest] public IEnumerator ShowcaseRapidNavigationReusesPreviewAndResetsMotion()
        {
            var ui=game.GetComponent<ArcadeModeUI>();ui.Open();var preview=game.GetComponentInChildren<ArcadePreview>();
            for(int i=0;i<24;i++)ui.Navigate(1);
            yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(ArcadeMode.Classic,ui.SelectedMode);Assert.AreSame(preview,game.GetComponentInChildren<ArcadePreview>());
            Assert.AreEqual(1,game.GetComponentsInChildren<ArcadePreview>().Length);
            var eventData=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=new Vector2(Screen.width*.8f,300)};
            ui.OnBeginDrag(eventData);eventData.position=new Vector2(Screen.width*.2f,300);ui.OnEndDrag(eventData);
            Assert.AreEqual(ArcadeMode.Rush,ui.SelectedMode);var play=System.Array.Find(ui.GetComponentsInChildren<UnityEngine.UI.Button>(),b=>b.name=="Play selected mode");
            float scale=Time.timeScale;Time.timeScale=0;
            try {play.onClick.Invoke();play.onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);}
            finally {Time.timeScale=scale;}
            Assert.AreEqual(ArcadeMode.Rush,game.Mode);Assert.AreEqual(GameState.Playing,game.State);
        }
        [UnityTest] public IEnumerator UrgencyRecoveryAndObjectivesHaveDedicatedPresentation()
        {
            var ui=game.GetComponent<ArcadeModeUI>();
            game.SelectMode(ArcadeMode.Rush);Perfect();game.Arcade.Tick(50.2f);ui.Refresh();yield return Preview("Rush-final10",1170,2532,true);
            game.SelectMode(ArcadeMode.Balance);var stack=Field<StackManager>("stack");
            stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+Vector3.right*1.4f;stack.PlaceCurrent();
            yield return Preview("Balance-danger",1170,2532,true);float before=stack.VisualBalance.magnitude;
            Perfect(); // Z-axis turn: preserve the X correction for the next X-axis block.
            Assert.AreEqual(Vector3.right,stack.ActiveBlock.Axis);
            stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+Vector3.left*1.1f;stack.PlaceCurrent();
            yield return Preview("Balance-recovery",1170,2532,true);Assert.AreEqual(GameState.Playing,game.State,"Corrective placement must keep the run playable");Assert.Less(stack.VisualBalance.magnitude,before);
            game.SelectMode(ArcadeMode.Trials,1);Perfect();Perfect();yield return Preview("Trials-objectives",1170,2532,true);
            for(int i=0;i<6;i++)Perfect();yield return Preview("Trials-completion",1170,2532,true);
        }
        [UnityTest] public IEnumerator DedicatedHudsFitBothLanguagesAndPortraitSizes()
        {
            var saves=Field<SaveManager>("saves");
            foreach(var locale in new[]{"tr","en"})
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1170,2532),new Vector2Int(1080,2400)})
            for(int mode=1;mode<4;mode++)
            {
                saves.Data.selectedLocale=locale;game.SelectMode((ArcadeMode)mode);Perfect();
                yield return Preview("HUD-"+mode+"-"+locale+"-"+size.y,size.x,size.y,true);
                Assert.AreEqual(GameState.Playing,game.State);
            }
        }
        [UnityTest] public IEnumerator EveryWorldThemeSupportsEveryMode()
        {
            var saves=Field<SaveManager>("saves");
            foreach(var theme in new[]{"Carrara","Obsidian","AncientStone","Neon","Space"})
            {
                saves.Data.selectedTheme=theme;
                for(int mode=0;mode<4;mode++)
                {game.SelectMode((ArcadeMode)mode);Perfect();yield return null;Assert.AreEqual(GameState.Playing,game.State);Assert.Greater(game.Score,0);Assert.AreEqual(theme,Field<Wobble.Themes.ThemeManager>("themes").Current.id.ToString());}
            }
        }
        [UnityTest] public IEnumerator TrialRewardsExactlyOnceAndClassicStillScores()
        {
            var s=Field<SaveManager>("saves");s.Data.trialStars[0]=0;int gems=s.Data.gems;
            for(int run=0;run<2;run++){game.SelectMode(ArcadeMode.Trials,0);for(int i=0;i<6;i++)Perfect();Assert.IsTrue(game.Arcade.Won);Assert.AreEqual(gems+5,s.Data.gems);}
            yield return Preview("Trials");game.SelectMode(ArcadeMode.Classic);Perfect();Assert.AreEqual(2,game.Score);yield return Preview("Classic");
            game.ReturnToMenu();game.GetComponent<ArcadeModeUI>().Open();yield return Preview("Selection");
        }
    }
}
