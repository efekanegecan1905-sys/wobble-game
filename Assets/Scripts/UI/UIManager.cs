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
            group.interactable=group.blocksRaycasts=false;
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
            var brand = Label("app.title", hud.transform, 34, FontStyles.Bold, TextAlignmentOptions.TopLeft, Color.white);
            SetRectTop(brand.rectTransform, 54, 54, 430, 60, false);
            levelText = Label("hud.level", hud.transform, 23, FontStyles.Bold, TextAlignmentOptions.TopLeft, WobblePalette.Surface);
            SetRectTop(levelText.rectTransform, 54, 112, 350, 50, false);
            scoreText = Label("hud.score", hud.transform, 58, FontStyles.Bold, TextAlignmentOptions.TopRight, Color.white);
            SetRectTop(scoreText.rectTransform, 54, 48, 400, 80, true);
            bestText = Label("hud.best", hud.transform, 22, FontStyles.Normal, TextAlignmentOptions.TopRight, WobblePalette.TextPrimary);
            SetRectTop(bestText.rectTransform, 54, 120, 400, 50, true);
            feedbackText = Label("hud.perfect", hud.transform, 36, FontStyles.Bold, TextAlignmentOptions.Center, WobblePalette.Success);
            SetRect(feedbackText.rectTransform, .5f, .60f, 520, 115);
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
                label.outlineColor = new Color(.08f, .10f, .16f, .8f);
                label.outlineWidth = .14f;
                var shadow = label.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(.08f, .10f, .16f, .65f);
                shadow.effectDistance = new Vector2(1, -2);
            }
        }

        private void BuildGameOver(Transform canvas)
        {
            gameOver = Panel("Game Over", canvas, WobblePalette.Alpha(WobblePalette.TextPrimary,.22f));
            resultGroup=gameOver.AddComponent<CanvasGroup>();
            var title = Label("gameover.title", gameOver.transform, 54, FontStyles.Bold, TextAlignmentOptions.Center, WobblePalette.Surface);
            title.characterSpacing=7;
            SetRect(title.rectTransform, .5f, .76f, 800, 85);
            var resultCard=Panel("Result card",gameOver.transform,WobblePalette.Alpha(WobblePalette.TextPrimary,.84f)); SetRect(resultCard.GetComponent<RectTransform>(),.5f,.55f,790,450); CasualSurface.Style(resultCard.GetComponent<Image>());
            resultsText = Label("gameover.summary", gameOver.transform, 29, FontStyles.Normal, TextAlignmentOptions.Center, new Color(.94f, .93f, .88f));
            resultsText.lineSpacing = 22f;
            SetRect(resultsText.rectTransform, .5f, .55f, 700, 420);
            var retry = Button("gameover.retry", gameOver.transform, WobblePalette.Primary, Color.white);
            SetRect(retry.GetComponent<RectTransform>(), .5f, .30f, 680, 125);
            retry.gameObject.AddComponent<StoneButtonFeedback>();
            retry.onClick.AddListener(() => EnterRun(true));
            var homeButton = Button("gameover.home", gameOver.transform, WobblePalette.Surface, WobblePalette.TextPrimary);
            SetRect(homeButton.GetComponent<RectTransform>(), .5f, .21f, 680, 90);
            homeButton.gameObject.AddComponent<StoneButtonFeedback>();
            homeButton.onClick.AddListener(() => {if(!Transitioning){audioManager.MenuTap();StartCoroutine(ReturnHome());}});
            continueButton = Button("gameover.continue", gameOver.transform, WobblePalette.Success, Color.white);
            SetRect(continueButton.GetComponent<RectTransform>(), .5f, .125f, 680, 90);
            continueButton.gameObject.AddComponent<StoneButtonFeedback>();
            continueLabel = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            continueButton.onClick.AddListener(() =>
            {
                if (!Transitioning && economy.CanContinueWithLife())
                { audioManager.MenuTap(); ContinuePressed?.Invoke(); }
            });
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
            Transitioning=true;resultGroup.interactable=resultGroup.blocksRaycasts=false;
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

        public void ShowGameOver(int score, int best, int perfects, int maxCombo)
        {
            hud.SetActive(false); gameOver.SetActive(true);
            continueButton.gameObject.SetActive(economy.CanContinueWithLife());
            lastScore=score; lastBest=best; lastPerfects=perfects; lastMaximum=maxCombo;
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
            resultGroup.interactable=resultGroup.blocksRaycasts=false;
            for(var t=0f;t<.28f;t+=Time.unscaledDeltaTime)
            {
                var p=Mathf.SmoothStep(0,1,t/.28f);resultGroup.alpha=p;resultsText.rectTransform.localScale=Vector3.one*Mathf.Lerp(.97f,1,p);yield return null;
            }
            resultGroup.alpha=1;resultsText.rectTransform.localScale=Vector3.one;resultGroup.interactable=resultGroup.blocksRaycasts=true;
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
            yield return AnimateFeedback(.58f);
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

        private IEnumerator AnimateFeedback(float hold)
        {
            for (var elapsed = 0f; elapsed < hold; elapsed += Time.unscaledDeltaTime)
            {
                var t = elapsed / hold;
                feedbackRise = t * 38f;
                feedbackText.rectTransform.localScale = Vector3.one * Mathf.Lerp(.86f, 1f, Mathf.Clamp01(t * 6f));
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
                var p = themeManager.Environment;
                var bright = (p.skyTop.grayscale + p.skyBottom.grayscale) * .5f > .6f;
                foreach (var label in hudLabels) label.color = bright ? WobblePalette.TextPrimary : WobblePalette.Surface;
                feedbackText.color = bright ? Color.Lerp(WobblePalette.TextPrimary, WobblePalette.Success, .3f) : WobblePalette.Success;
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
            if (continueLabel != null) continueLabel.text = Localization.Text("gameover.continue") + "  ♥ 1";
            if (scoreText == null) return;
            scoreText.text = lastScore.ToString();
            bestText.text = Localization.Text("hud.best", lastBest);
            levelText.text = Localization.Text("hud.level", lastLevel);
            resultsText.text = Localization.Text("gameover.summary", lastScore, lastBest, lastPerfects, lastMaximum);
            feedbackText.text = Localization.Text("hud.combo", lastCombo);
            levelFlash.text = Localization.Text("hud.level", lastLevel);
        }

        private void OnDestroy() { if (Localization != null) Localization.Changed -= RefreshLocalization; }

        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        private static void SetRect(RectTransform rect, float x, float y, float width, float height) { rect.anchorMin = rect.anchorMax = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = Vector2.zero; }
        private static void SetRectTop(RectTransform rect, float margin, float top, float width, float height, bool right)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(right ? 1 : 0, 1); rect.pivot = new Vector2(right ? 1 : 0, 1); rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = new Vector2(right ? -margin : margin, -top);
        }
    }
}
