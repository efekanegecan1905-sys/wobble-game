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
        private T Field<T>(string name)=>(T)typeof(GameManager).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            had=PlayerPrefs.HasKey("wobble.save.v1");saved=PlayerPrefs.GetString("wobble.save.v1");
            var old=Object.FindFirstObjectByType<GameManager>();if(old!=null)Object.Destroy(old.gameObject);yield return null;
            game=new GameObject("Arcade QA").AddComponent<GameManager>();
            yield return new WaitUntil(()=>game.GetComponent<UIManager>().Localization.Ready);game.GetComponent<UIManager>().SkipIntro();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {Object.Destroy(game.gameObject);yield return null;if(had)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();}
        private void Perfect()
        {var s=Field<StackManager>("stack");s.ActiveBlock.transform.position=s.Top.transform.position+Vector3.up*StackManager.BlockHeight;s.PlaceCurrent();}
        private void Miss()
        {var s=Field<StackManager>("stack");s.ActiveBlock.transform.position=s.Top.transform.position+s.ActiveBlock.Axis*30;s.PlaceCurrent();}
        private IEnumerator Preview(string name,int width=1080,int height=1920,bool checkMenu=false)
        {
            System.IO.Directory.CreateDirectory("Artifacts/Arcade");yield return new WaitForSecondsRealtime(.3f);
            var camera=game.GetComponentInChildren<Camera>();var target=new RenderTexture(width,height,24);target.Create();
            var canvases=game.GetComponentsInChildren<Canvas>();
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            camera.targetTexture=target;yield return null;Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
            if(checkMenu)
            {
                var modeUI=game.GetComponent<ArcadeModeUI>();
                foreach(var button in modeUI.GetComponentsInChildren<UnityEngine.UI.Button>())
                {
                    var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
                    foreach(var corner in corners){var p=camera.WorldToViewportPoint(corner);Assert.That(p.x,Is.InRange(0f,1f),button.name);Assert.That(p.y,Is.InRange(0f,1f),button.name);}
                }
                foreach(var label in modeUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
                {
                    label.ForceMeshUpdate();Assert.IsFalse(label.isTextOverflowing,label.text);
                    foreach(char c in label.GetParsedText())if(!char.IsWhiteSpace(c))Assert.IsTrue(label.font.HasCharacter(c),"Unbaked arcade glyph: "+c);
                }
            }
            var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            System.IO.File.WriteAllBytes("Artifacts/Arcade/"+name+".png",image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;
            foreach(var canvas in canvases)canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            Object.Destroy(image);target.Release();Object.Destroy(target);
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
                yield return Preview("Selection-"+locale,1080,2340,true);
                var trialButton=System.Array.Find(ui.GetComponentsInChildren<UnityEngine.UI.Button>(),b=>b.name.StartsWith("TOWER TRIALS"));
                trialButton.onClick.Invoke();yield return Preview("TrialLevels-"+locale,1080,1920,true);
                ui.Open();yield return Preview("Selection-tablet-"+locale,1536,2048,true);
            }
        }
        [UnityTest] public IEnumerator BalanceLeanNeverChangesStackingCollider()
        {
            game.SelectMode(ArcadeMode.Balance);var stack=Field<StackManager>("stack");
            var top=stack.Top;var box=top.GetComponent<BoxCollider>();var bounds=box.bounds;
            top.ArcadeBalance=new Vector2(.9f,-.7f);yield return null;Physics.SyncTransforms();
            Assert.AreEqual(Quaternion.identity,top.transform.localRotation);
            Assert.AreEqual(bounds.center,box.bounds.center);Assert.AreEqual(bounds.size,box.bounds.size);
            game.SelectMode(ArcadeMode.Classic);Perfect();Assert.AreEqual(2,game.Score);
            Assert.AreEqual(Vector2.zero,stack.Top.ArcadeBalance);
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
