using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wobble.Audio;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.UI;
namespace Wobble.Tests
{
    public sealed class ComboAudioTests
    {
        [UnityTest] public IEnumerator ProfilesClampBreakAndRestartThroughRealPlacements()
        {
            var hadSave=PlayerPrefs.HasKey("wobble.save.v1");var saved=PlayerPrefs.GetString("wobble.save.v1");
            var old=Object.FindFirstObjectByType<GameManager>();if(old!=null)Object.Destroy(old.gameObject);yield return null;
            var host=new GameObject("Combo audio integration");
            try
            {
                var game=host.AddComponent<GameManager>();yield return new WaitUntil(()=>host.GetComponent<UIManager>().Localization.Ready);
                typeof(GameManager).GetMethod("StartGame",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,null);
                var audio=(AudioManager)typeof(GameManager).GetField("audio",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);audio.SetEnabled(true);
                var stack=(StackManager)typeof(GameManager).GetField("stack",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game);
                var pattern=new[]{false,true,true,true,true,true,true,true,false,false,true};
                var expected=new[]{0,1,2,3,4,5,6,7,0,0,1};
                var pitches=new System.Collections.Generic.List<string>();
                for(var i=0;i<pattern.Length;i++)
                {
                    stack.ActiveBlock.transform.position=stack.Top.transform.position+Vector3.up*StackManager.BlockHeight+(pattern[i]?Vector3.zero:stack.ActiveBlock.Axis*.2f);
                    var before=audio.ImpactCount;stack.PlaceCurrent();Assert.That(audio.ImpactCount,Is.EqualTo(before+1));
                    Assert.That(audio.LastProfileIndex,Is.EqualTo(expected[i]));
                    var expectedPitch=1+expected[i]*.07f;
                    Assert.That(audio.LastPitch,Is.EqualTo(expectedPitch).Within(.0051f));
                    Assert.That(audio.LastPitch,Is.LessThanOrEqualTo(1.49f));
                    var primary=host.GetComponents<AudioSource>()[(i%2)*4];
                    Assert.That(primary.pitch,Is.EqualTo(audio.LastPitch).Within(.0001f));
                    Assert.That(primary.clip.name,Does.StartWith("PercussivePlacement_"));
                    pitches.Add($"step={i} perfect={pattern[i]} comboIndex={expected[i]} actualPitch={primary.pitch:F4} clip={primary.clip.name}");
                    if(!pattern[i])
                    {
                        var voices=host.GetComponents<AudioSource>();
                        for(var j=0;j<8;j++)if(j%4!=0 && j!=(i%2)*4+1)Assert.That(voices[j].isPlaying,Is.False,"Combo tail survived break");
                        Assert.That(voices[(i%2)*4+1].isPlaying,Is.False);
                        Assert.That(audio.LastPitch,Is.EqualTo(1));
                    }
                    foreach(var voice in host.GetComponents<AudioSource>().Take(8))
                        if(voice.isPlaying)Assert.That(voice.clip.name,Does.StartWith("PercussivePlacement_"),"Old stone placement leak");
                    yield return new WaitForSeconds(.09f);
                }
                Assert.That(audio.PlacementSettings.Pitch(true,8),Is.EqualTo(audio.PlacementSettings.Pitch(true,100)));
                var folder=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../Artifacts/Audio"));System.IO.Directory.CreateDirectory(folder);
                System.IO.File.WriteAllLines(System.IO.Path.Combine(folder,"PercussiveRuntimePitch.txt"),pitches);
                Assert.That(host.GetComponents<AudioSource>().Length,Is.EqualTo(11));
                audio.SetEnabled(false);var count=audio.ImpactCount;audio.Impact(true,5);Assert.That(audio.ImpactCount,Is.EqualTo(count));
            }
            finally
            {Object.Destroy(host);if(hadSave)PlayerPrefs.SetString("wobble.save.v1",saved);else PlayerPrefs.DeleteKey("wobble.save.v1");PlayerPrefs.Save();}
            yield return null;
        }
        [UnityTest] public IEnumerator InspectorTuningControlsLayersAndTailDuration()
        {
            var asset=Object.Instantiate(Resources.Load<PercussivePlacementSettings>("Audio/PercussivePlacementSettings"));
            var host=new GameObject("Tuned impact");var audio=new AudioManager(host,null,null,asset);
            try
            {
                asset.RandomPitchRange=asset.RandomVolumeRange=0;asset.BasePitch=1;asset.ComboPitchStep=.04f;asset.MaxComboPitch=1.28f;
                asset.Volume=.4f;asset.Duration=.08f;asset.ResonanceAmount=.01f;asset.HighComboBodyCompensation=.02f;
                audio.Impact(true,99);var sources=host.GetComponents<AudioSource>();
                Assert.That(sources[0].pitch,Is.EqualTo(1.28f).Within(.001));
                Assert.That(sources[1].pitch,Is.EqualTo(1).Within(.001));
                Assert.That(sources[1].clip,Is.SameAs(sources[0].clip));
                Assert.That(sources[1].volume,Is.EqualTo(.03f*AudioManager.EffectsHeadroom).Within(.001));
                yield return new WaitForSecondsRealtime(.16f);Assert.That(sources[0].isPlaying||sources[1].isPlaying,Is.False);
                audio.Impact(false,99);Assert.That(audio.LastProfileIndex,Is.Zero);
                Assert.That(sources[5].isPlaying||sources[6].isPlaying||sources[7].isPlaying,Is.False);
                Assert.That(sources[4].pitch,Is.EqualTo(1));
                Assert.That(sources[4].volume,Is.EqualTo(.4f*AudioManager.EffectsHeadroom).Within(.001));
                for(var i=0;i<80;i++)audio.Impact(true,99);
                Assert.That(host.GetComponents<AudioSource>().Length,Is.EqualTo(11));
                foreach(var voice in sources.Take(8))if(voice.isPlaying)Assert.That(voice.clip.name,Does.StartWith("PercussivePlacement_"));
                audio.Cut(.5f);Assert.That(sources[8].isPlaying||sources[9].isPlaying,Is.False,"Cut added an old sound at lock-in");
            }
            finally{audio.Dispose();Object.Destroy(host);Object.Destroy(asset);}
            yield return null;
        }
    }
}
