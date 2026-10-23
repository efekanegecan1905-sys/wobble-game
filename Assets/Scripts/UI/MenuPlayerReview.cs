#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wobble.Core;
using Wobble.Save;

namespace Wobble.UI
{
    // Explicit opt-in verification of bundled localization, never installed in release builds.
    public sealed class MenuPlayerReview : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(), "--wobble-menu-check")<0)return;
            Application.runInBackground=true;
            new GameObject("Menu player verification").AddComponent<MenuPlayerReview>();
        }
        private IEnumerator Start()
        {
            const string directory="/private/tmp/wobble-menu-player/review";
            Directory.CreateDirectory(directory);
            var original=new SaveManager();
            var originalLocale=original.Data.selectedLocale;
            yield return null;
            var game=FindFirstObjectByType<GameManager>();
            var ui=game.GetComponent<UIManager>();
            var deadline=Time.realtimeSinceStartup+30;
            while(!ui.Localization.Ready && Time.realtimeSinceStartup<deadline)yield return null;
            if(!ui.Localization.Ready){Debug.LogError("Menu player localization timeout");Application.Quit(1);yield break;}
            ui.SkipIntro();
            foreach(var locale in new[]{"tr","en"})
            {
                ui.Menus.SelectLanguage(locale);
                yield return new WaitForSecondsRealtime(.4f);
                var expected=locale=="tr"?"OYNA":"PLAY";
                if(ui.Localization.Text("home.play")!=expected || new SaveManager().Data.selectedLocale!=locale)
                {Debug.LogError("Menu player locale verification failed: "+locale);Application.Quit(1);yield break;}
                yield return new WaitForEndOfFrame();
                var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(directory,locale+"-player-home.png"),image.EncodeToPNG());
                if(image.GetPixels32().Any(p=>p.r>220 && p.g<40 && p.b>220))
                {Debug.LogError("Menu player contains shader-error magenta pixels.");ui.Menus.SelectLanguage(originalLocale);Application.Quit(1);yield break;}
                Destroy(image);
                yield return new WaitForSecondsRealtime(.3f);
            }
            ui.Menus.SelectLanguage(originalLocale);
            var before=new SaveManager().Data.lives;
            ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="home.play").onClick.Invoke();
            yield return new WaitForSecondsRealtime(.65f);
            var passed=game.State==GameState.Playing && new SaveManager().Data.lives==before;
            ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="gameover.home").onClick.Invoke();
            passed &= game.State==GameState.Home;
            File.WriteAllText(Path.Combine(directory,"verification.txt"),passed
                ? "PASS: bundled TR/EN String Tables, immediate language switch, saved locale, free PLAY, HOME return, no shader-error magenta pixels."
                : "FAIL: game navigation or life invariant.");
            Application.Quit(passed?0:1);
        }
    }
}
#endif
