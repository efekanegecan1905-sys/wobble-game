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
        private IEnumerator Preview(string name)
        {
            System.IO.Directory.CreateDirectory("Artifacts/Arcade");yield return new WaitForSecondsRealtime(.3f);
            var camera=game.GetComponentInChildren<Camera>();var target=new RenderTexture(1080,1920,24);target.Create();
            var canvases=game.GetComponentsInChildren<Canvas>();
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            camera.targetTexture=target;yield return null;Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
            var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1080,1920,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1080,1920),0,0);image.Apply();
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
        [UnityTest] public IEnumerator TrialRewardsExactlyOnceAndClassicStillScores()
        {
            var s=Field<SaveManager>("saves");s.Data.trialStars[0]=0;int gems=s.Data.gems;
            for(int run=0;run<2;run++){game.SelectMode(ArcadeMode.Trials,0);for(int i=0;i<6;i++)Perfect();Assert.IsTrue(game.Arcade.Won);Assert.AreEqual(gems+5,s.Data.gems);}
            yield return Preview("Trials");game.SelectMode(ArcadeMode.Classic);Perfect();Assert.AreEqual(2,game.Score);yield return Preview("Classic");
            game.ReturnToMenu();game.GetComponent<ArcadeModeUI>().Open();yield return Preview("Selection");
        }
    }
}
