using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.UI;

namespace Wobble.Tests
{
    public sealed class ProductPresentationTests
    {
        private GameManager game;private UIManager ui;private bool hadSave;private string saved;
        [UnitySetUp] public IEnumerator Setup()
        {
            hadSave=PlayerPrefs.HasKey("wobble.save.v1");saved=PlayerPrefs.GetString("wobble.save.v1");
            var old=Object.FindFirstObjectByType<GameManager>();if(old!=null)Object.Destroy(old.gameObject);yield return null;
            game=new GameObject("Product presentation test").AddComponent<GameManager>();ui=game.GetComponent<UIManager>();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(game.gameObject);yield return null;
            if(hadSave)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();
        }
        [UnityTest] public IEnumerator StartupIsBoundedAndPlayRetryDoNotRepeatIt()
        {
            Assert.That(ui.IntroPlaying,Is.True);
            var started=Time.realtimeSinceStartup;
            foreach(var second in new[]{.7f,1.4f,2.4f,3.6f,4.5f,5.2f})
            {
                while(ui.Presentation.IntroProgress<second/5.8f)yield return null;
                Assert.That(ui.IntroPlaying,Is.True);
                yield return Capture("Intro-"+second.ToString("F1",System.Globalization.CultureInfo.InvariantCulture),Sizes[0],ui.Presentation.Camera);
            }
            while(ui.IntroPlaying&&Time.realtimeSinceStartup-started<6.5f)yield return null;
            Assert.That(ui.IntroPlaying,Is.False);Assert.That(game.State,Is.EqualTo(GameState.Home));
            Assert.That(Time.realtimeSinceStartup-started,Is.InRange(5.5f,6.5f));
            var audio=(Wobble.Audio.AudioManager)typeof(GameManager).GetField("audio",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            Assert.That(audio.IntroPlacementCount,Is.EqualTo(6));Assert.That(audio.SignatureCount,Is.EqualTo(1));
            foreach(var size in Sizes)yield return Capture("Home-after-intro",size,ui.Presentation.Camera);
            Click("home.play");Click("home.play");Assert.That(game.State,Is.EqualTo(GameState.Home));
            yield return new WaitForSecondsRealtime(.67f);
            Assert.That(game.State,Is.EqualTo(GameState.Playing));Assert.That(ui.Transitioning,Is.False);
            var stack=(StackManager)typeof(GameManager).GetField("stack",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            stack.ActiveBlock.transform.position+=Vector3.right*30;stack.PlaceCurrent();yield return new WaitForSecondsRealtime(1.4f);
            Assert.That(game.State,Is.EqualTo(GameState.GameOver));
            foreach(var language in new[]{"tr","en"})
            {
                ui.Menus.SelectLanguage(language);yield return null;
                foreach(var size in Sizes)yield return Capture(language+"-Results",size,game.GetComponentsInChildren<Camera>().First(c=>c.enabled));
            }
            Click("gameover.retry");yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.State,Is.EqualTo(GameState.Playing));Assert.That(game.Score,Is.Zero);Assert.That(ui.IntroPlaying,Is.False);
            Assert.That(stack.ActiveBlock.Direction,Is.EqualTo(MoveDirection.PositiveX));
            Click("gameover.home");yield return new WaitForSecondsRealtime(.3f);
            Assert.That(ui.IntroPlaying,Is.False);Assert.That(audio.SignatureCount,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator SkipEndsIntroWithoutDelayedSounds()
        {
            ui.SkipIntro();Assert.That(ui.IntroPlaying,Is.False);Assert.That(ui.Presentation.IntroProgress,Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(6);
            var audio=(Wobble.Audio.AudioManager)typeof(GameManager).GetField("audio",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            Assert.That(audio.IntroPlacementCount,Is.Zero);Assert.That(audio.SignatureCount,Is.Zero);
        }
        private static readonly Vector2Int[] Sizes={new(1080,1920),new(1170,2532),new(1080,2400)};
        private void Click(string name)=>ui.GetComponentsInChildren<Button>(true).Single(b=>b.name==name).onClick.Invoke();
        private IEnumerator Capture(string label,Vector2Int size,Camera camera)
        {
            var canvas=ui.GetComponentInChildren<Canvas>();var target=new RenderTexture(size.x,size.y,24){antiAliasing=4};camera.targetTexture=target;camera.aspect=size.x/(float)size.y;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            yield return null;Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
            {text.ForceMeshUpdate();Assert.That(text.isTextOverflowing,Is.False,label+": "+text.text);Assert.That(text.text,Does.Not.Contain("No translation found"));}
            var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
            var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../Artifacts/Product/Previews"));Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory,label+"-"+size.x+"x"+size.y+".png"),image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(image);target.Release();Object.Destroy(target);
        }
    }
}
