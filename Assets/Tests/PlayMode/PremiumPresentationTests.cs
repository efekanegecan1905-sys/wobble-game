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
    public class PremiumPresentationTests
    {
        private float originalScale;
        private GameManager game;private string saved;private bool had;
#if UNITY_EDITOR
        private UnityEditor.EditorWindow captureView;private PropertyInfo captureSize;private int originalSize;
#endif
        private T Field<T>(string name)=>(T)typeof(GameManager).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            originalScale=Time.timeScale;
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
        {Time.timeScale=originalScale;Object.Destroy(game.gameObject);yield return null;
#if UNITY_EDITOR
            if(captureView!=null){captureSize.SetValue(captureView,originalSize);captureView.Repaint();yield return null;}
#endif
            if(had)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();}
        private void Perfect()
        {var s=Field<StackManager>("stack");s.ActiveBlock.transform.position=s.Top.transform.position+Vector3.up*StackManager.BlockHeight;s.PlaceCurrent();}
        private void Miss()
        {var s=Field<StackManager>("stack");s.ActiveBlock.transform.position=s.Top.transform.position+s.ActiveBlock.Axis*30;s.PlaceCurrent();}
        private IEnumerator Preview(string name,int width=1080,int height=1920,bool checkMenu=true,float delay=.25f)
        {
            System.IO.Directory.CreateDirectory("Artifacts/PremiumArcade");
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
            for(float elapsed=0;elapsed<delay;elapsed+=Time.unscaledDeltaTime){view.Repaint();yield return null;}
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
                    if(label.font==GameplayTypography.Hud||label.font==GameplayTypography.Display)
                        Assert.AreEqual(label.font.atlasTexture,label.fontSharedMaterial.mainTexture,"Wrong gameplay atlas: "+label.name);
                    foreach(char c in label.GetParsedText())if(!char.IsWhiteSpace(c))Assert.IsTrue(label.font.HasCharacter(c),"Unbaked arcade glyph: "+c);
                }
            }
            var previous=RenderTexture.active;var output=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32);
            if(SystemInfo.graphicsUVStartsAtTop)Graphics.Blit(target,output,new Vector2(1,-1),new Vector2(0,1));else Graphics.Blit(target,output);
            RenderTexture.active=output;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            System.IO.File.WriteAllBytes("Artifacts/PremiumArcade/"+name+".png",image.EncodeToPNG());RenderTexture.active=previous;
            Object.Destroy(image);RenderTexture.ReleaseTemporary(output);
#if !UNITY_EDITOR
            target.Release();Object.Destroy(target);
#endif
        }
        [UnityTest] public IEnumerator ExpiryRejectsLatePlacementAndTrialRewardsAreOnceOnly()
        {
            var saves=Field<SaveManager>("saves");saves.Data.trialStars[0]=0;
            game.SelectMode(ArcadeMode.Rush);Time.timeScale=0;
            game.Arcade.Tick(59.99f);Perfect();int earned=game.Score;Assert.Greater(earned,0);
            game.Arcade.Tick(.02f);
            var placed=typeof(GameManager).GetMethod("OnPlaced",BindingFlags.NonPublic|BindingFlags.Instance);
            placed.Invoke(game,new object[]{true,Vector3.zero});Assert.AreEqual(earned,game.Score);
            yield return null;Assert.AreEqual(GameState.GameOver,game.State);
            int count=game.GetComponent<RunCelebration>().ResultCount;
            placed.Invoke(game,new object[]{true,Vector3.zero});yield return null;
            Assert.AreEqual(earned,game.Score);Assert.AreEqual(count,game.GetComponent<RunCelebration>().ResultCount);
            var camera=Object.FindFirstObjectByType<Wobble.CameraSystem.CameraController>();
            for(int i=0;i<5;i++)yield return null;
            Assert.IsFalse(float.IsNaN(camera.transform.position.x),"Paused camera must stay finite");
            int gems=saves.Data.gems;game.SelectMode(ArcadeMode.Trials,0);
            for(int i=0;i<6;i++)Perfect();Assert.IsTrue(game.TrialFirstClear);Assert.AreEqual(gems+5,saves.Data.gems);
            typeof(GameManager).GetMethod("FinishArcade",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,null);
            Assert.AreEqual(gems+5,saves.Data.gems);
            yield return Preview("Trials-first-clear-reward",1080,2400,true,.8f);
            game.SelectMode(ArcadeMode.Trials,0);game.Arcade.Tick(45);yield return null;
            yield return Preview("Trials-time-up",1080,2400,true,.18f);
            Assert.IsFalse(game.LastRecord.Improved);Assert.AreEqual(gems+5,saves.Data.gems);
            Assert.AreEqual(3,new SaveManager().Data.trialStars[0]);
        }
        [UnityTest] public IEnumerator FontCandidatesInActualGameplay()
        {
#if UNITY_EDITOR
            var candidate=UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Editor/FontComparisons/Fredoka.asset");
            var faces=new[]{Resources.Load<TMPro.TMP_FontAsset>("Menu/WobbleDisplay"),candidate,GameplayTypography.Hud,GameplayTypography.Display};
            var names=new[]{"Nunito-Inter","Fredoka-Rubik","Rubik-Rubik","Bricolage-Rubik"};
            for(int i=0;i<faces.Length;i++)
            {
                game.SelectMode(ArcadeMode.Rush);Perfect();Time.timeScale=0;
                var root=game.transform.Find("Arcade interface/Dedicated arcade HUD");
                foreach(var label in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>())label.font=i==0?Resources.Load<TMPro.TMP_FontAsset>("Menu/WobbleBody"):GameplayTypography.Hud;
                var presentation=game.GetComponent<RunCelebration>();presentation.Placement(true,8);
                var cue=game.transform.Find("Gameplay celebrations/Safe gameplay celebrations/Nonblocking gameplay cue/Countdown and achievement").GetComponent<TMPro.TextMeshProUGUI>();cue.font=faces[i];cue.fontSharedMaterial=faces[i].material;
                yield return Preview("Font-"+names[i],1080,1920,true,.18f);
            }
#endif
        }
        [UnityTest] public IEnumerator CountdownAndResultsFitEveryPortraitAndLocale()
        {
            var saves=Field<SaveManager>("saves");saves.Data.rushBest=100;
            foreach(var locale in new[]{"tr","en"})
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1170,2532),new Vector2Int(1080,2400)})
            {
                saves.Data.selectedLocale=locale;game.SelectMode(ArcadeMode.Rush);Perfect();Time.timeScale=0;
                foreach(int second in new[]{15,10,5,3,2,1,0})
                {
                    game.Arcade.Tick(Mathf.Max(0,game.Arcade.Remaining-second));yield return null;
                    yield return Preview("Rush-"+second+"-"+locale+"-"+size.y,size.x,size.y,true,.18f);
                }
                Assert.AreEqual(GameState.GameOver,game.State);Assert.IsTrue(game.Arcade.TimedOut);
                yield return Preview("Rush-result-"+locale+"-"+size.y,size.x,size.y,true,.8f);
                int results=game.GetComponent<RunCelebration>().ResultCount;
                typeof(GameManager).GetMethod("FinishArcade",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,null);
                Assert.AreEqual(results,game.GetComponent<RunCelebration>().ResultCount);
            }
        }
        [UnityTest] public IEnumerator RecordsRewardsRecoveryAndReplayAreIndependent()
        {
            var saves=Field<SaveManager>("saves");saves.Data.bestScore=0;saves.Data.balanceBest=0;saves.Data.rushBest=100;saves.Data.trialStars[0]=1;int gems=saves.Data.gems;
            game.SelectMode(ArcadeMode.Classic);for(int i=0;i<5;i++)Perfect();yield return Preview("Classic-gameplay");Miss();yield return new WaitForSecondsRealtime(.9f);yield return Preview("Classic-new-best",1080,1920,true,.8f);
            Assert.IsTrue(game.LastRecord.Improved);Assert.AreEqual(0,game.LastRecord.Previous);
            game.SelectMode(ArcadeMode.Balance);var stack=Field<StackManager>("stack");
            stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+Vector3.right*1.4f;stack.PlaceCurrent();
            yield return Preview("Balance-danger",1170,2532);Perfect();
            stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+Vector3.left*1.1f;stack.PlaceCurrent();
            yield return Preview("Balance-recovery",1170,2532,true,.18f);Assert.Less(game.Arcade.Balance.magnitude,.35f);
            Miss();yield return Preview("Balance-new-best",1170,2532,true,.8f);Assert.IsTrue(game.LastRecord.Improved);
            gems=saves.Data.gems;game.SelectMode(ArcadeMode.Trials,0);for(int i=0;i<5;i++)Perfect();yield return Preview("Trials-objectives",1080,2400);
            Perfect();yield return Preview("Trials-improved-stars",1080,2400,true,.8f);
            Assert.IsTrue(game.LastRecord.Improved);Assert.AreEqual(1,game.LastRecord.Previous);Assert.AreEqual(3,saves.Data.trialStars[0]);Assert.IsFalse(game.TrialFirstClear);Assert.AreEqual(gems,saves.Data.gems);
            var retry=game.transform.Find("Gameplay celebrations/Safe gameplay celebrations/Run result/Retry run").GetComponent<UnityEngine.UI.Button>();retry.onClick.Invoke();
            Assert.AreEqual(GameState.Playing,game.State);Assert.AreEqual(0,game.Score);
            for(int i=0;i<6;i++)Perfect();Assert.IsFalse(game.LastRecord.Improved);Assert.AreEqual(gems,saves.Data.gems);
            yield return Preview("Trials-repeat-no-false-record",1080,2400,true,.8f);
            Assert.AreEqual(100,saves.Data.rushBest);
        }
        [UnityTest] public IEnumerator GameplayTypographyFitsBothLanguagesAndAllThemes()
        {
            var saves=Field<SaveManager>("saves");
            foreach(var theme in new[]{"Carrara","Obsidian","AncientStone","Neon","Space"})
            {
                saves.Data.selectedTheme=theme;
                foreach(var locale in new[]{"tr","en"})
                {
                    saves.Data.selectedLocale=locale;
                    for(int mode=0;mode<4;mode++)
                    {
                        game.SelectMode((ArcadeMode)mode);Perfect();Time.timeScale=0;
                        yield return Preview("Gameplay-"+theme+"-"+mode+"-"+locale,1080,1920,true,.18f);
                    }
                }
            }
        }
    }
}
