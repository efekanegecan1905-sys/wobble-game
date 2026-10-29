using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Wobble.Save;
using Wobble.Economy;
using Wobble.Audio;
using Wobble.Themes;
using System.Collections.Generic;

namespace Wobble.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        public event Action PlayPressed;
        public event Action RetryPressed;
        public event Action HomePressed;
        public event Action ContinuePressed;

        private TMP_FontAsset font;
        public MenuScreens Menus { get; private set; }
        public MenuLocalization Localization { get; private set; }
        public HomePresentation Presentation { get; private set; }
        private readonly List<Action> localizedLabels = new();
        private int lastScore, lastBest, lastLevel = 1, lastPerfects, lastMaximum, lastCombo = 2;
        private GameObject hud;
        private GameObject gameOver;
        private Button continueButton;
        private TextMeshProUGUI continueLabel;
        private EconomyManager economy;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI levelText;
        private TextMeshProUGUI bestText;
        private TextMeshProUGUI feedbackText;
        private TextMeshProUGUI resultsText;
        private TextMeshProUGUI tapHint;
        private Coroutine feedbackRoutine;
        private TextMeshProUGUI levelFlash;
        private Coroutine levelRoutine;
        private RectTransform safeArea;
        private Canvas canvas;
        private AudioManager audioManager;
        private ThemeManager themeManager;
        private TextMeshProUGUI[] hudLabels;
        private CanvasGroup menuGroup,resultGroup;
        private Image curtain;
        private Coroutine introRoutine,transitionRoutine;
        private Coroutine resultRoutine;
        public bool IntroPlaying {get;private set;}
        public bool Transitioning {get;private set;}
        private Rect previousSafeArea;
        private Vector3 feedbackWorld;
        private float feedbackRise;
        private int lastBlocks, lastGems;
        private RectTransform resultCardRect;
        private readonly RectTransform[] resultChips=new RectTransform[4];
        private bool countingResult;
        private TextMeshProUGUI resultBest;
        private TextMeshProUGUI resultBlocks, resultPerfects, resultCombo, resultGems;
        private TextMeshProUGUI resultScoreCaption, resultBestCaption, resultBlockCaption, resultPerfectCaption, resultComboCaption, resultGemCaption;
        private TextMeshProUGUI shareLabel;
        private Image flash;
        private Coroutine flashRoutine, shareRoutine;
        private static Sprite verticalFade;
        private static readonly Vector3[] TravelAxes = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        public void Build(SaveManager saves, EconomyManager economy, AudioManager audioManager, HapticManager haptics, ThemeManager themes)
        {
            this.audioManager=audioManager;
            themeManager=themes;
            this.economy=economy;
            font = Resources.Load<TMP_FontAsset>("Menu/WobbleBody")??Resources.Load<TMP_FontAsset>("Menu/WobbleLocalizedSans");
            Localization = gameObject.AddComponent<MenuLocalization>();
            Localization.Initialize(saves);
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(transform);
            var canvasGo = new GameObject("WOBBLE UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;
            safeArea = new GameObject("Safe Area", typeof(RectTransform)).GetComponent<RectTransform>();
            safeArea.SetParent(canvasGo.transform, false);
            Stretch(safeArea);
            ApplySafeArea();

            Menus = gameObject.AddComponent<MenuScreens>();
            Menus.Build(safeArea, saves, economy, audioManager, haptics, Localization, () => EnterRun(false));
            Menus.DemoThemeSelected += theme => themes.SelectTheme(theme.id);
            menuGroup=Menus.Root.AddComponent<CanvasGroup>();
            Presentation = gameObject.AddComponent<HomePresentation>();
            Presentation.Build(themes, UnityEngine.Camera.main);
            Menus.SectionChanged+=Presentation.SetSection;
            BuildHud(safeArea);
            BuildGameOver(safeArea);
            Localization.Changed += RefreshLocalization;
            ShowHome(saves.Data.bestScore);
            curtain=Panel("Identity fade",canvasGo.transform,Color.black).GetComponent<Image>();
            curtain.raycastTarget=true;
            introRoutine=StartCoroutine(Startup());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--wobble-skip-intro")>=0)SkipIntro();
#endif
        }

        private IEnumerator Startup()
        {
            IntroPlaying=true;menuGroup.alpha=0;menuGroup.interactable=menuGroup.blocksRaycasts=false;
            var signature=false;var placedLetters=0;var atmosphereDust=false;
            const float duration=5.8f;
            for(var t=0f;t<duration;t+=Time.unscaledDeltaTime)
            {
                Presentation.SetIntro(t/duration);
                curtain.color=WobblePalette.Alpha(WobblePalette.TextPrimary,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.6f,t)));
                menuGroup.alpha=Mathf.SmoothStep(0,1,Mathf.InverseLerp(5.0f,duration,t));
                ((RectTransform)menuGroup.transform).anchoredPosition=Vector2.down*(24*(1-menuGroup.alpha));
                if(!atmosphereDust&&t>=.6f){Presentation.AssemblyDust(-1);atmosphereDust=true;}
                while(placedLetters<6&&t>=2.15f+placedLetters*.15f)
                {audioManager.IntroPlacement();Presentation.AssemblyDust(placedLetters);placedLetters++;}
                if(!signature&&t>=4.2f){audioManager.Signature();Presentation.SignatureDust();signature=true;}
                yield return null;
            }
            FinishIntro();introRoutine=null;
        }
        private void FinishIntro()
        {
            IntroPlaying=false;Presentation.SetIntro(1);menuGroup.alpha=1;
            ((RectTransform)menuGroup.transform).anchoredPosition=Vector2.zero;
            menuGroup.interactable=menuGroup.blocksRaycasts=true;curtain.gameObject.SetActive(false);
        }
        public void SkipIntro()
        {
            if(introRoutine!=null){StopCoroutine(introRoutine);introRoutine=null;}
            if(curtain!=null)FinishIntro();
        }
        private void EnterRun(bool retry)
        {
            if(Transitioning||IntroPlaying)return;
            transitionRoutine=StartCoroutine(EnterAnimation(retry));
        }
        private IEnumerator EnterAnimation(bool retry)
        {
            Transitioning=true;audioManager.PlayTransition();
            var group=retry?resultGroup:menuGroup;
            group.blocksRaycasts=false;
            var duration=retry?.32f:.58f;
            for(var t=0f;t<duration;t+=Time.unscaledDeltaTime)
            {
                var progress=Mathf.SmoothStep(0,1,t/duration);group.alpha=1-progress;
                if(!retry)Presentation.SetEntry(progress);
                yield return null;
            }
            Transitioning=false;transitionRoutine=null;
            if(retry)RetryPressed?.Invoke();else PlayPressed?.Invoke();
        }

        private void BuildHud(Transform canvas)
        {
            hud = Panel("HUD", canvas, Color.clear);
            var scrim = Panel("HUD top scrim", hud.transform, Color.clear).GetComponent<Image>();
            scrim.sprite = VerticalFade();
            scrim.color = new Color(.02f, .045f, .085f, .58f);
            scrim.raycastTarget = false;
            var scrimRect = scrim.rectTransform;
            scrimRect.anchorMin = new Vector2(0, 1); scrimRect.anchorMax = new Vector2(1, 1); scrimRect.pivot = new Vector2(.5f, 1);
            scrimRect.sizeDelta = new Vector2(0, 330); scrimRect.anchoredPosition = Vector2.zero;
            var brand = Label("app.title", hud.transform, 34, FontStyles.Bold, TextAlignmentOptions.TopLeft, Color.white);
            SetRectTop(brand.rectTransform, 54, 54, 430, 60, false);
            levelText = Label("hud.level", hud.transform, 23, FontStyles.Bold, TextAlignmentOptions.TopLeft, WobblePalette.Surface);
            SetRectTop(levelText.rectTransform, 54, 112, 350, 50, false);
            scoreText = Label("hud.score", hud.transform, 58, FontStyles.Bold, TextAlignmentOptions.TopRight, Color.white);
            SetRectTop(scoreText.rectTransform, 54, 48, 400, 80, true);
            bestText = Label("hud.best", hud.transform, 22, FontStyles.Normal, TextAlignmentOptions.TopRight, WobblePalette.TextPrimary);
            SetRectTop(bestText.rectTransform, 54, 120, 400, 50, true);
            feedbackText = Label("hud.perfect", hud.transform, 40, FontStyles.Bold, TextAlignmentOptions.Center, WobblePalette.Success);
            SetRect(feedbackText.rectTransform, .5f, .60f, 560, 140);
            feedbackText.lineSpacing = -18f;
            feedbackText.canvasRenderer.SetAlpha(0);
            levelFlash = Label("hud.level", hud.transform, 32, FontStyles.Bold, TextAlignmentOptions.Center, new Color(.88f, .85f, .77f));
            SetRect(levelFlash.rectTransform, .5f, .76f, 500, 80);
            levelFlash.canvasRenderer.SetAlpha(0);
            tapHint = Label("hud.tap", hud.transform, 25, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1, 1, 1, .65f));
            SetRect(tapHint.rectTransform, .5f, .10f, 500, 70);
            bestText.color = WobblePalette.Surface;
            hudLabels = new[] { brand, levelText, scoreText, bestText, feedbackText, levelFlash, tapHint };
            foreach (var label in hudLabels)
            {
                label.outlineColor = new Color(.06f, .08f, .13f, .95f);
                label.outlineWidth = .20f;
                var shadow = label.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(.08f, .10f, .16f, .65f);
                shadow.effectDistance = new Vector2(1, -2);
            }
        }

        private void BuildGameOver(Transform canvas)
        {
            gameOver = Panel("Game Over", canvas, WobblePalette.Alpha(WobblePalette.TextPrimary,.58f));
            resultGroup=gameOver.AddComponent<CanvasGroup>();
            var title = Label("gameover.title", gameOver.transform, 46, FontStyles.Bold, TextAlignmentOptions.Center, WobblePalette.Surface);
            title.characterSpacing=8;
            Center(title.rectTransform, .5f, .862f, 880, 76);
            var kicker = Panel("Result accent rule", gameOver.transform, WobblePalette.Alpha(WobblePalette.Primary,.95f));
            Center(kicker.GetComponent<RectTransform>(), .5f, .822f, 150, 9);
            CasualSurface.Style(kicker.GetComponent<Image>(), false);
            kicker.GetComponent<Image>().raycastTarget = false;
            var card=Panel("Result card",gameOver.transform,WobblePalette.Surface);
            resultCardRect=card.GetComponent<RectTransform>(); Center(resultCardRect,.5f,.556f,920,780); CasualSurface.Style(card.GetComponent<Image>());
            var accentBar=Panel("Result accent",card.transform,WobblePalette.Primary);
            Region(accentBar.GetComponent<RectTransform>(),.35f,.958f,.65f,.976f);CasualSurface.Style(accentBar.GetComponent<Image>(),false);
            accentBar.GetComponent<Image>().raycastTarget=false;
            resultScoreCaption=Caption("result.score",card.transform,20,WobblePalette.TextSecondary);
            Center(resultScoreCaption.rectTransform,.5f,.885f,420,40);
            resultsText = Label("result.value", card.transform, 126, FontStyles.Bold, TextAlignmentOptions.Center, WobblePalette.TextPrimary);
            Center(resultsText.rectTransform,.5f,.742f,840,246);
            resultBest = Label("hud.best", card.transform, 25, FontStyles.Normal, TextAlignmentOptions.Center, WobblePalette.PrimaryPressed);
            Center(resultBest.rectTransform,.5f,.621f,540,58);
            var divider=Panel("Result divider",card.transform,WobblePalette.Alpha(WobblePalette.TextPrimary,.14f));
            Region(divider.GetComponent<RectTransform>(),.10f,.552f,.90f,.555f);divider.GetComponent<Image>().raycastTarget=false;
            resultBlocks=StatChip(card.transform,.285f,.445f,"result.blocks",out resultBlockCaption);
            resultPerfects=StatChip(card.transform,.715f,.445f,"result.perfects",out resultPerfectCaption);
            resultCombo=StatChip(card.transform,.285f,.235f,"result.combo",out resultComboCaption);
            resultGems=StatChip(card.transform,.715f,.235f,"result.gems",out resultGemCaption);
            resultChips[0]=(RectTransform)resultBlocks.transform.parent;
            resultChips[1]=(RectTransform)resultPerfects.transform.parent;
            resultChips[2]=(RectTransform)resultCombo.transform.parent;
            resultChips[3]=(RectTransform)resultGems.transform.parent;
            var retry = Button("gameover.retry", gameOver.transform, WobblePalette.Primary, Color.white);
            Center(retry.GetComponent<RectTransform>(), .5f, .295f, 820, 120);
            retry.gameObject.AddComponent<StoneButtonFeedback>();
            retry.onClick.AddListener(() => EnterRun(true));
            var homeButton = Button("gameover.home", gameOver.transform, WobblePalette.Surface, WobblePalette.TextPrimary);
            Center(homeButton.GetComponent<RectTransform>(), .315f, .19f, 390, 96);
            homeButton.gameObject.AddComponent<StoneButtonFeedback>();
            homeButton.onClick.AddListener(() => {if(!Transitioning){audioManager.MenuTap();StartCoroutine(ReturnHome());}});
            var share = Button("gameover.share", gameOver.transform, WobblePalette.Surface, WobblePalette.TextPrimary);
            Center(share.GetComponent<RectTransform>(), .685f, .19f, 390, 96);
            share.gameObject.AddComponent<StoneButtonFeedback>();
            shareLabel = share.GetComponentInChildren<TextMeshProUGUI>();
            share.onClick.AddListener(() =>
            {
                if (Transitioning) return;
                audioManager.MenuTap();
                GUIUtility.systemCopyBuffer = Localization.Text("gameover.share_text", lastScore, lastBest);
                if (shareRoutine != null) StopCoroutine(shareRoutine);
                shareRoutine = StartCoroutine(ShareFeedback());
            });
            continueButton = Button("gameover.continue", gameOver.transform, WobblePalette.Success, Color.white);
            Center(continueButton.GetComponent<RectTransform>(), .5f, .078f, 780, 96);
            continueButton.gameObject.AddComponent<StoneButtonFeedback>();
            continueLabel = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            continueButton.onClick.AddListener(() =>
            {
                if (!Transitioning && economy.CanContinueWithLife())
                { audioManager.MenuTap(); ContinuePressed?.Invoke(); }
            });
            flash = Panel("Impact flash", canvas, Color.clear).GetComponent<Image>();
            flash.color = new Color(1, 1, 1, 0);
            flash.raycastTarget = false;
        }

        private TextMeshProUGUI Caption(string key, Transform parent, int size, Color color)
        {
            var label = Label(key, parent, size, FontStyles.Normal, TextAlignmentOptions.Center, color);
            label.characterSpacing = 3;
            localizedLabels.Add(() => label.text = Localization.Text(key));
            label.text = Localization.Text(key);
            return label;
        }

        private TextMeshProUGUI StatChip(Transform parent, float x, float y, string key, out TextMeshProUGUI caption)
        {
            var chip = Panel("Stat "+key, parent, WobblePalette.Alpha(WobblePalette.TextPrimary,.055f));
            Center(chip.GetComponent<RectTransform>(), x, y, 372, 156);
            var image = chip.GetComponent<Image>();
            CasualSurface.Style(image, false);
            image.raycastTarget = false;
            caption = Caption(key, chip.transform, 18, WobblePalette.TextSecondary);
            Center(caption.rectTransform, .5f, .74f, 330, 38);
            var value = Label("result.value", chip.transform, 42, FontStyles.Bold, TextAlignmentOptions.Center, WobblePalette.TextPrimary);
            Center(value.rectTransform, .5f, .33f, 340, 88);
            return value;
        }

        private IEnumerator ShareFeedback()
        {
            shareLabel.text = Localization.Text("gameover.copied");
            yield return new WaitForSecondsRealtime(1.4f);
            shareLabel.text = Localization.Text("gameover.share");
            shareRoutine = null;
        }

        public void FailFlash()
        {
            if (flash == null) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(new Color(.92f, .34f, .30f), .22f));
        }

        private void PerfectFlash()
        {
            if (flash == null) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(themeManager != null ? themeManager.Accent : WobblePalette.Accent, .10f));
        }

        private IEnumerator FlashRoutine(Color color, float strength)
        {
            const float duration = .34f;
            for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                var p = t / duration;
                var a = strength * Mathf.Sin(p * Mathf.PI);
                flash.color = new Color(color.r, color.g, color.b, a);
                yield return null;
            }
            flash.color = new Color(color.r, color.g, color.b, 0);
            flashRoutine = null;
        }

        public void ShowHome(int best)
        {
            if(menuGroup!=null){menuGroup.alpha=1;menuGroup.interactable=menuGroup.blocksRaycasts=true;}
            lastBest = best;
            Menus.ShowHome(); hud.SetActive(false); gameOver.SetActive(false);
            Presentation.Show(true);
        }

        private IEnumerator ReturnHome()
        {
            Transitioning=true;resultGroup.blocksRaycasts=false;
            for(var t=0f;t<.24f;t+=Time.unscaledDeltaTime){resultGroup.alpha=1-Mathf.SmoothStep(0,1,t/.24f);yield return null;}
            Transitioning=false;HomePressed?.Invoke();
        }

        public void ShowGame(int best)
        {
            SkipIntro();
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            if (levelRoutine != null) StopCoroutine(levelRoutine);
            feedbackText.canvasRenderer.SetAlpha(0);
            levelFlash.canvasRenderer.SetAlpha(0);
            Menus.Hide(); Presentation.Show(false); hud.SetActive(true); gameOver.SetActive(false);
            tapHint.gameObject.SetActive(best < 10);
            UpdateHud(0, best, 1);
        }

        public void ShowGameOver(int score, int best, int perfects, int maxCombo, int blocks = 0, int gems = 0)
        {
            hud.SetActive(false); gameOver.SetActive(true);
            continueButton.gameObject.SetActive(economy.CanContinueWithLife());
            lastScore=score; lastBest=best; lastPerfects=perfects; lastMaximum=maxCombo;
            lastBlocks=blocks; lastGems=gems;
            if (shareRoutine != null) { StopCoroutine(shareRoutine); shareRoutine = null; }
            if (shareLabel != null) shareLabel.text = Localization.Text("gameover.share");
            RefreshLocalization();
            if (resultRoutine != null) StopCoroutine(resultRoutine);
            resultRoutine = StartCoroutine(ResultEntrance());
        }
        public void ShowContinuedGame(int score, int best, int level)
        {
            if (resultRoutine != null) { StopCoroutine(resultRoutine); resultRoutine = null; }
            gameOver.SetActive(false);
            hud.SetActive(true);
            UpdateHud(score, best, level);
        }
        private IEnumerator ResultEntrance()
        {
            // Alpha + raycast gating only: toggling CanvasGroup.interactable makes
            // Selectables paint their disabled tint, which leaves the result buttons
            // washed out until the next pointer event.
            resultGroup.interactable=true;
            resultGroup.blocksRaycasts=false;
            countingResult=true;
            resultGroup.alpha=0f;
            resultCardRect.localScale=Vector3.one*.90f;
            resultCardRect.anchoredPosition=new Vector2(0,64);
            for(var i=0;i<resultChips.Length;i++) if(resultChips[i]!=null) resultChips[i].localScale=Vector3.zero;
            var shown=-1;
            const float duration=.64f;
            for(var t=0f;t<duration;t+=Time.unscaledDeltaTime)
            {
                var p=Mathf.Clamp01(t/duration);
                resultGroup.alpha=Mathf.Clamp01(p*3.4f);
                // The card lands with a small overshoot, then the four stat tiles pop
                // in sequence. The score rolls up so the run's total feels earned.
                var card=Mathf.Clamp01(t/.42f);
                resultCardRect.localScale=Vector3.one*Mathf.Lerp(.90f,1f,EaseOutBack(card));
                resultCardRect.anchoredPosition=new Vector2(0,Mathf.Lerp(64,0,Mathf.SmoothStep(0,1,card)));
                for(var i=0;i<resultChips.Length;i++)
                {
                    if(resultChips[i]==null)continue;
                    var cp=Mathf.Clamp01((t-.28f-i*.055f)/.32f);
                    resultChips[i].localScale=Vector3.one*Mathf.Lerp(.80f,1f,EaseOutBack(cp));
                }
                var target=Mathf.RoundToInt(lastScore*Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.10f)/.46f)));
                if(target!=shown){shown=target;resultsText.text=shown.ToString();}
                yield return null;
            }
            resultGroup.alpha=1f;resultCardRect.localScale=Vector3.one;resultCardRect.anchoredPosition=Vector2.zero;
            for(var i=0;i<resultChips.Length;i++) if(resultChips[i]!=null) resultChips[i].localScale=Vector3.one;
            resultsText.text=lastScore.ToString();
            countingResult=false;
            resultGroup.interactable=resultGroup.blocksRaycasts=true;
            resultRoutine=null;
        }

        public void UpdateHud(int score, int best, int level)
        {
            lastScore=score; lastBest=best; lastLevel=level;
            scoreText.text = score.ToString(); bestText.text = Localization.Text("hud.best", best); levelText.text = Localization.Text("hud.level", level);
            if (score > 2) tapHint.gameObject.SetActive(false);
        }

        public void Feedback(bool perfect, int combo, Vector3 position)
        {
            if (!perfect) return;
            PerfectFlash();
            feedbackWorld = position;
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedbackRoutine = StartCoroutine(FeedbackAnimation(combo));
        }

        public void LevelUp(int level)
        {
            if (levelRoutine != null) StopCoroutine(levelRoutine);
            levelRoutine = StartCoroutine(LevelAnimation(level));
        }

        private IEnumerator FeedbackAnimation(int combo)
        {
            lastCombo = Mathf.Max(2, combo);
            feedbackText.text = Localization.Text("hud.combo", lastCombo);
            // Milestones hold the badge a little longer so x5 and x8 read as events.
            yield return AnimateFeedback(lastCombo >= 8 ? .84f : lastCombo >= 5 ? .70f : .58f, lastCombo);
        }

        private IEnumerator LevelAnimation(int level)
        {
            levelFlash.text = Localization.Text("hud.level", level);
            for (var t = 0f; t < .95f; t += Time.unscaledDeltaTime)
            {
                levelFlash.canvasRenderer.SetAlpha(Mathf.Min(t / .1f, (.95f - t) / .3f));
                yield return null;
            }
            levelFlash.canvasRenderer.SetAlpha(0);
        }

        private IEnumerator AnimateFeedback(float hold, int combo = 0)
        {
            var punch = combo >= 8 ? .30f : combo >= 5 ? .17f : .07f;
            for (var elapsed = 0f; elapsed < hold; elapsed += Time.unscaledDeltaTime)
            {
                var t = elapsed / hold;
                feedbackRise = t * 38f;
                // Overshoot in, settle out: the label snaps up past its final size for
                // higher combos, which gives each milestone a new physical weight.
                var pop = EaseOutBack(Mathf.Clamp01(t * 4.4f));
                feedbackText.rectTransform.localScale = Vector3.one * Mathf.Lerp(.82f, 1f + punch, pop);
                feedbackText.canvasRenderer.SetAlpha(Mathf.Clamp01(Mathf.Min(t * 9f, (1f - t) * 3f)));
                yield return null;
            }
            feedbackText.canvasRenderer.SetAlpha(0);
        }

        private void LateUpdate()
        {
            if (previousSafeArea != Screen.safeArea) ApplySafeArea();
            if (hudLabels != null && themeManager != null)
            {
                var accent = themeManager.Accent;
                // Keep the badge mostly cream: on gold/sand destinations an accent-tinted
                // label would sink into the sky. Escalation lives in size and glow.
                var heat = lastCombo >= 8 ? .30f : lastCombo >= 5 ? .22f : .14f;
                feedbackText.color = Color.Lerp(WobblePalette.Surface, accent, heat);
                feedbackText.outlineWidth = lastCombo >= 5 ? .26f : .20f;
                levelFlash.color = Color.Lerp(WobblePalette.Surface, accent, .16f);
            }
            if (feedbackText == null || !hud.activeSelf || UnityEngine.Camera.main == null || feedbackText.canvasRenderer.GetAlpha() <= .001f) return;
            // Normalized coordinates work identically for native overlay UI and
            // offscreen portrait review targets, independent of the editor window.
            var camera = UnityEngine.Camera.main;
            var viewport = camera.WorldToViewportPoint(feedbackWorld + Vector3.up * .55f);
            var highest = viewport.y;
            foreach (var axis in TravelAxes)
            for (var x = -1; x <= 1; x += 2)
            for (var z = -1; z <= 1; z += 2)
            {
                var p = camera.WorldToViewportPoint(feedbackWorld + axis * Wobble.Gameplay.StackManager.TravelBoundary + new Vector3(x * 1.65f, .6f, z * 1.65f));
                highest = Mathf.Max(highest, p.y);
            }
            var normalized = new Vector2(
                (viewport.x - safeArea.anchorMin.x) / (safeArea.anchorMax.x - safeArea.anchorMin.x),
                (highest - safeArea.anchorMin.y) / (safeArea.anchorMax.y - safeArea.anchorMin.y));
            var point = new Vector2(safeArea.rect.xMin + normalized.x * safeArea.rect.width,
                safeArea.rect.yMin + normalized.y * safeArea.rect.height + 72f + feedbackRise);
            point.x = Mathf.Clamp(point.x, safeArea.rect.xMin + 270, safeArea.rect.xMax - 270);
            point.y = Mathf.Clamp(point.y, safeArea.rect.yMin + safeArea.rect.height * .36f, safeArea.rect.yMax - 300);
            feedbackText.rectTransform.anchorMin = feedbackText.rectTransform.anchorMax = new Vector2(.5f, .5f);
            feedbackText.rectTransform.anchoredPosition = point;
        }

        private void ApplySafeArea()
        {
            previousSafeArea = Screen.safeArea;
            safeArea.anchorMin = new Vector2(previousSafeArea.xMin / Screen.width, previousSafeArea.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(previousSafeArea.xMax / Screen.width, previousSafeArea.yMax / Screen.height);
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        }

        private GameObject Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false); Stretch(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = color;
            go.GetComponent<Image>().material = Resources.Load<Material>("Menu/MenuUI");
            go.GetComponent<Image>().raycastTarget = color.a > .01f;
            return go;
        }

        private TextMeshProUGUI Label(string text, Transform parent, int size, FontStyles style, TextAlignmentOptions anchor, Color color)
        {
            var go = new GameObject(text.Length > 20 ? "Label" : text, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>(); label.font = style==FontStyles.Bold ? (Resources.Load<TMP_FontAsset>("Menu/WobbleDisplay") ?? font) : font; label.text = string.Empty; label.fontSize = size; label.fontStyle = style; label.alignment = anchor; label.color = color;
            label.raycastTarget = false;
            if (text == "app.title" || text == "hud.tap" || text.StartsWith("gameover.") && text != "gameover.summary")
                localizedLabels.Add(() => label.text = Localization.Text(text));
            return label;
        }

        private Button Button(string text, Transform parent, Color background, Color foreground)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false); go.GetComponent<Image>().color = background; CasualSurface.Style(go.GetComponent<Image>());
            go.GetComponent<Image>().material = Resources.Load<Material>("Menu/MenuUI");
            var button = go.GetComponent<Button>();
            var colors = button.colors; colors.highlightedColor = Color.Lerp(background, Color.white, .18f); colors.pressedColor = background * .78f; button.colors = colors;
            var label = Label(text, go.transform, 31, FontStyles.Bold, TextAlignmentOptions.Center, foreground); Stretch(label.rectTransform);
            return button;
        }

        private void RefreshLocalization()
        {
            foreach (var update in localizedLabels) update();
            if (continueLabel != null) continueLabel.text = Localization.Text("gameover.continue") + "   ♥ 1";
            if (shareLabel != null && shareRoutine == null) shareLabel.text = Localization.Text("gameover.share");
            if (scoreText == null) return;
            scoreText.text = lastScore.ToString();
            bestText.text = Localization.Text("hud.best", lastBest);
            levelText.text = Localization.Text("hud.level", lastLevel);
            resultsText.text = countingResult ? resultsText.text : lastScore.ToString();
            resultBest.text = Localization.Text("hud.best", lastBest);
            resultBlocks.text = lastBlocks.ToString();
            resultPerfects.text = lastPerfects.ToString();
            resultCombo.text = "×" + lastMaximum;
            resultGems.text = "+" + lastGems;
            feedbackText.text = Localization.Text("hud.combo", lastCombo);
            levelFlash.text = Localization.Text("hud.level", lastLevel);
        }

        private void OnDestroy() { if (Localization != null) Localization.Changed -= RefreshLocalization; }

        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        private static void Center(RectTransform rect, float x, float y, float width, float height) { rect.anchorMin = rect.anchorMax = new Vector2(x, y); rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = Vector2.zero; }
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            t -= 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }
        private static void Region(RectTransform rect, float x0, float y0, float x1, float y1) { rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero; }

        private static Sprite VerticalFade()
        {
            if (verticalFade != null) return verticalFade;
            var texture = new Texture2D(4, 64, TextureFormat.RGBA32, false)
            {
                name = "HUD top fade", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[4 * 64];
            for (var y = 0; y < 64; y++)
            {
                var a = (byte)(Mathf.SmoothStep(0, 1, y / 63f) * 255f);
                for (var x = 0; x < 4; x++) pixels[y * 4 + x] = new Color32(255, 255, 255, a);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            verticalFade = Sprite.Create(texture, new Rect(0, 0, 4, 64), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
            return verticalFade;
        }
        private static void SetRect(RectTransform rect, float x, float y, float width, float height) { rect.anchorMin = rect.anchorMax = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = Vector2.zero; }
        private static void SetRectTop(RectTransform rect, float margin, float top, float width, float height, bool right)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(right ? 1 : 0, 1); rect.pivot = new Vector2(right ? 1 : 0, 1); rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = new Vector2(right ? -margin : margin, -top);
        }
    }
}
