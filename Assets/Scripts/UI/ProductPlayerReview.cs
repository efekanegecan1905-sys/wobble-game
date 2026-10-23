#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Audio;
namespace Wobble.UI
{
    // Explicit development-only player QA, not a second menu or gameplay manager.
    public sealed class ProductPlayerReview : MonoBehaviour
    {
        private const string Folder="/private/tmp/wobble-concept-player/review";
        private readonly Vector2Int[] sizes={new(1080,1920),new(1170,2532),new(1080,2400)};
        private readonly List<string> report=new();
        private UIManager ui;private GameManager game;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] private static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--wobble-product-check")<0)return;
            Application.runInBackground=true;new GameObject("Product player verification").AddComponent<ProductPlayerReview>();
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(Folder);var hadSave=PlayerPrefs.HasKey("wobble.save.v1");var saved=PlayerPrefs.GetString("wobble.save.v1");
            yield return null;game=FindFirstObjectByType<GameManager>();ui=game.GetComponent<UIManager>();
            report.Add(SystemInfo.deviceModel+" / "+SystemInfo.graphicsDeviceName);report.Add("Development desktop samples; not mobile FPS certification.");
            yield return new WaitForSecondsRealtime(1.4f);
            foreach(var size in sizes)yield return Capture("Splash",size,ui.Presentation.Camera,false);
            while(ui.IntroPlaying)yield return null;
            var deadline=Time.realtimeSinceStartup+15;while(!ui.Localization.Ready&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!ui.Localization.Ready){report.Add("FAIL localization timeout");Finish(hadSave,saved);yield break;}
            Application.targetFrameRate=-1;
            foreach(var language in new[]{"tr","en"})
            {
                ui.Menus.SelectLanguage(language);
                foreach(var size in sizes)
                foreach(var screen in new[]{MenuScreen.Home,MenuScreen.Shop,MenuScreen.Themes,MenuScreen.Daily,MenuScreen.Settings})
                {
                    ui.Menus.Open(screen);yield return new WaitForSecondsRealtime(.3f);
                    yield return Capture(language+"-"+screen,size,ui.Presentation.Camera,screen==MenuScreen.Home&&language=="tr");
                    if(screen==MenuScreen.Settings)
                    {
                        Click("settings.credits");yield return null;
                        yield return Capture(language+"-AudioCredits",size,ui.Presentation.Camera,false);ui.Menus.Back();
                    }
                }
            }
            ui.Menus.Open(MenuScreen.Home);yield return new WaitForSecondsRealtime(.3f);Click("home.play");yield return new WaitForSecondsRealtime(.7f);
            var stack=Field<StackManager>("stack");var audio=Field<AudioManager>("audio");audio.SetEnabled(true);
            var peak=0f;var samples=new float[1024];
            for(var i=0;i<32;i++)
            {
                stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+(i==0||i==7?stack.ActiveBlock.Axis*.12f:Vector3.zero);stack.PlaceCurrent();
                for(var t=0f;t<.21f;t+=Time.unscaledDeltaTime)
                {AudioListener.GetOutputData(samples,0);foreach(var sample in samples)peak=Mathf.Max(peak,Mathf.Abs(sample));yield return null;}
                if(i==0||i==10||i==20||i==30)
                {
                    stack.ActiveBlock.Stop();yield return new WaitForSecondsRealtime(2.6f);
                    foreach(var size in sizes)yield return Capture("World-Level-"+(i/10+1),size,game.GetComponentsInChildren<Camera>().First(c=>c.enabled),false);
                    stack.ActiveBlock.Begin(stack.ActiveBlock.Direction,stack.Top.transform.position,StackManager.TravelBoundary,Field<LevelManager>("levels").Speed);
                }
            }
            report.Add("Audio listener observed peak="+peak.ToString("F4")+"; signal check only, not perceptual Foley approval.");
            stack.ActiveBlock.Stop();yield return new WaitForSecondsRealtime(2.5f);
            foreach(var size in sizes)yield return Capture("Gameplay",size,game.GetComponentsInChildren<Camera>().First(c=>c.enabled),true);
            stack.ActiveBlock.Begin(stack.ActiveBlock.Direction,stack.Top.transform.position,StackManager.TravelBoundary,Field<LevelManager>("levels").Speed);
            stack.ActiveBlock.transform.position+=stack.ActiveBlock.Axis*30;stack.PlaceCurrent();yield return new WaitForSecondsRealtime(1);
            foreach(var language in new[]{"tr","en"})
            {
                ui.Menus.SelectLanguage(language);
                foreach(var size in sizes)yield return Capture(language+"-Results",size,game.GetComponentsInChildren<Camera>().First(c=>c.enabled),false);
            }
            Click("gameover.retry");yield return new WaitForSecondsRealtime(.4f);
            report.Add(game.State==GameState.Playing&&!ui.IntroPlaying?"PASS fast retry without intro":"FAIL retry");
            Finish(hadSave,saved);
        }
        private T Field<T>(string name)=>(T)typeof(GameManager).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
        private void Click(string name)=>ui.GetComponentsInChildren<Button>(true).Single(b=>b.name==name).onClick.Invoke();
        private void Finish(bool hadSave,string saved)
        {
            if(hadSave)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();
            File.WriteAllLines(Path.Combine(Folder,"verification.txt"),report);Application.Quit(report.Any(s=>s.StartsWith("FAIL"))?1:0);
        }
        private IEnumerator Capture(string label,Vector2Int size,Camera camera,bool profile)
        {
            var canvas=ui.GetComponentInChildren<Canvas>();var target=new RenderTexture(size.x,size.y,24){antiAliasing=4};camera.targetTexture=target;camera.aspect=size.x/(float)size.y;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            if(profile)
            {
                for(var i=0;i<40;i++)yield return null;var timing=new List<float>();
                for(var i=0;i<120;i++){yield return null;timing.Add(Time.unscaledDeltaTime*1000);}timing.Sort();
                report.Add(label+" "+size.x+"x"+size.y+": mean="+timing.Average().ToString("F2")+"ms p95="+timing[114].ToString("F2")+"ms");
            }
            yield return null;Canvas.ForceUpdateCanvases();RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
            {text.ForceMeshUpdate();if(text.isTextOverflowing||text.text.Contains("No translation found"))report.Add("FAIL text: "+label+" / "+text.text);}
            var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
            if(image.GetPixels32().Any(p=>p.r>220&&p.g<40&&p.b>220))report.Add("FAIL shader magenta: "+label);
            File.WriteAllBytes(Path.Combine(Folder,label+"-"+size.x+"x"+size.y+".png"),image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Destroy(image);target.Release();Destroy(target);
        }
    }
}
#endif
