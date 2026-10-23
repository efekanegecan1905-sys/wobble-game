using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Themes;
using Wobble.Audio;
using Wobble.UI;

namespace Wobble.Tests
{
    public sealed class FeelPlayModeTests
    {
        private GameManager game;private string saved;private bool hadSave;
        private T Field<T>(string name)=>(T)typeof(GameManager).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
        [UnitySetUp] public IEnumerator Setup()
        {
            hadSave=PlayerPrefs.HasKey("wobble.save.v1");saved=PlayerPrefs.GetString("wobble.save.v1");
            var old=Object.FindFirstObjectByType<GameManager>();if(old!=null)Object.Destroy(old.gameObject);yield return null;
            game=new GameObject("Feel test").AddComponent<GameManager>();
            yield return new WaitUntil(()=>game.GetComponent<UIManager>().Localization.Ready);
            typeof(GameManager).GetMethod("StartGame",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            Field<AudioManager>("audio").SetEnabled(true);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(game.gameObject);yield return null;
            if(hadSave)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();
        }
        [UnityTest] public IEnumerator NormalAndPerfectImpactAreSynchronousAndDoNotAlterCollider()
        {
            var stack=Field<StackManager>("stack");var audio=Field<AudioManager>("audio");
            for(var i=0;i<6;i++)
            {
                var moving=stack.ActiveBlock;var offset=i%2==0?Vector3.zero:moving.Axis*.2f;
                moving.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+offset;
                var count=audio.ImpactCount;stack.PlaceCurrent();var size=stack.Top.Size;
                Assert.That(audio.ImpactCount,Is.EqualTo(count+1));
                yield return new WaitForSeconds(.07f);
                Assert.That(stack.Top.transform.localScale,Is.EqualTo(size));
                Assert.That(stack.Top.GetComponent<BoxCollider>().size,Is.EqualTo(Vector3.one));
                Assert.That(game.GetComponentsInChildren<ParticleSystem>(),Is.Not.Empty);
            }
            var previous=-1;
            for(var i=0;i<50;i++)
            {
                audio.Impact(true,i+2);Assert.That(audio.LastVariation,Is.Not.EqualTo(previous));previous=audio.LastVariation;
                Assert.That(audio.LastPitch,Is.EqualTo(1+Mathf.Clamp(i+1,1,7)*.07f).Within(.0051));
            }
            Assert.That(game.GetComponents<AudioSource>().Length,Is.EqualTo(AudioManager.MaximumEffectVoices+1));
            audio.SetEnabled(false);var before=audio.ImpactCount;audio.Impact(true,5);Assert.That(audio.ImpactCount,Is.EqualTo(before));
        }
        [UnityTest] public IEnumerator FourStagesRemainColliderFreeAndRenderAllPortraitRatios()
        {
            var stack=Field<StackManager>("stack");var world=game.GetComponent<CarraraWorld>();
            Assert.That(world.GeometryCount,Is.EqualTo(24));
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1170,2532),new Vector2Int(1080,2400)})
            {
                typeof(GameManager).GetMethod("StartGame",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
                for(var stage=1;stage<=4;stage++)
                {
                    if(stage>1)for(var i=0;i<10;i++)
                    {stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight;stack.PlaceCurrent();yield return null;}
                    stack.ActiveBlock.Stop();yield return new WaitForSeconds(2.5f);
                    Assert.That(world.Stage,Is.EqualTo(stage));Assert.That(world.AmbientParticleLimit,Is.LessThanOrEqualTo(36));
                    var scenery=game.transform.Find("Carrara architecture — presentation only");
                    Assert.That(scenery.GetComponentsInChildren<Collider>(),Is.Empty);
                    var camera=game.GetComponentInChildren<Camera>();
                    foreach(var renderer in scenery.GetComponentsInChildren<MeshRenderer>())
                    {
                        if(!renderer.enabled)continue;
                        var forward=camera.transform.forward;var bounds=renderer.bounds;
                        var radius=Vector3.Dot(bounds.extents,new Vector3(Mathf.Abs(forward.x),Mathf.Abs(forward.y),Mathf.Abs(forward.z)));
                        Assert.That(Vector3.Dot(bounds.center-stack.Top.transform.position,forward)-radius,Is.GreaterThan(8.9f),"Architecture entered active travel depth.");
                    }
                    yield return Capture(stage,size);
                    // Resume the already existing block for the next sequence, without changing production code.
                    stack.ActiveBlock.Begin(stack.ActiveBlock.Direction,stack.Top.transform.position,StackManager.TravelBoundary,Field<LevelManager>("levels").Speed);
                }
            }
        }
        private IEnumerator Capture(int stage,Vector2Int size)
        {
            var camera=game.GetComponentInChildren<Camera>();var canvas=game.GetComponentInChildren<Canvas>();
            var rt=new RenderTexture(size.x,size.y,24){antiAliasing=4};camera.targetTexture=rt;camera.aspect=size.x/(float)size.y;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            yield return null;Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
            var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
            var dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Artifacts/Feel/Previews"));Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir,"stage-"+stage+"-"+size.x+"x"+size.y+".png"),image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(image);rt.Release();Object.Destroy(rt);
        }
    }
}
