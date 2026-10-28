using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wobble.Save;
using Wobble.Economy;
using Wobble.Audio;
using Wobble.Themes;

namespace Wobble.UI
{
    public sealed class MenuScreens : MonoBehaviour
    {
        private static readonly Color White = WobblePalette.TextPrimary;
        private static readonly Color Muted = WobblePalette.TextSecondary;
        private static readonly Color Charcoal = WobblePalette.Alpha(WobblePalette.SkyBottom,.40f);
        private SaveManager save;
        private EconomyManager economy;
        private AudioManager audioManager;
        private HapticManager haptics;
        private MenuLocalization loc;
        private MenuCatalog catalog;
        private TMP_FontAsset font;
        private readonly List<Action> refresh = new();
        private readonly List<Action> popupRefresh = new();
        private GameObject root;
        private GameObject popup;
        private float nextDailyRefresh;
        public MenuNavigation Navigation { get; private set; }
        public DailyRewardService Daily { get; private set; }
        public GameObject Root => root;
        public event Action<MenuScreen> SectionChanged;
        public event Action<ThemeDefinition> DemoThemeSelected;
        private bool unlockingTheme;

        public void Build(Transform parent, SaveManager save, EconomyManager economy, AudioManager audioManager,
            HapticManager haptics, MenuLocalization loc, Action play)
        {
            this.save=save; this.economy=economy; this.audioManager=audioManager; this.haptics=haptics; this.loc=loc;
            catalog=Resources.Load<MenuCatalog>("Menu/MenuCatalog");
            font=Resources.Load<TMP_FontAsset>("Menu/WobbleBody")??Resources.Load<TMP_FontAsset>("Menu/WobbleLocalizedSans");
            Daily=new DailyRewardService(save,catalog);
            root=Panel("Menu Root",parent,Color.clear);
            Navigation=root.AddComponent<MenuNavigation>();
            var home=Screen(MenuScreen.Home);
            var themes=Screen(MenuScreen.Themes);
            var shop=Screen(MenuScreen.Shop);
            var daily=Screen(MenuScreen.Daily);
            var settings=Screen(MenuScreen.Settings);
            BuildHome(home.transform,play);
            BuildThemes(Content(themes,"themes.title"));
            BuildShop(Content(shop,"shop.title"));
            BuildDaily(Content(daily,"daily.title"));
            BuildSettings(Content(settings,"settings.title"));
            BuildHeader();
            BuildNavigation();
            save.Changed+=Refresh;
            loc.Changed+=Refresh;
            Navigation.Show(MenuScreen.Home,true);
            Refresh();
        }

        private GameObject Screen(MenuScreen screen)
        {
            var panel=Panel(screen.ToString(),root.transform,screen==MenuScreen.Home?Color.clear:Charcoal);
            Navigation.Register(screen,panel);
            return panel;
        }
        private void BuildHeader()
        {
            var title=Text(root.transform,()=>loc.Text("app.title"),94,true,TextAlignmentOptions.TopLeft);
            Top(title.rectTransform,54,60,670,115);title.characterSpacing=3;title.color=WobblePalette.Surface;
            var shadow=title.gameObject.AddComponent<Shadow>();shadow.effectDistance=new Vector2(0,-5);shadow.effectColor=WobblePalette.Alpha(WobblePalette.TextPrimary,.35f);
            var subtitle=Text(root.transform,()=>loc.Text("home.collection"),23,false,TextAlignmentOptions.TopLeft);
            subtitle.color=WobblePalette.Surface;subtitle.characterSpacing=9;Top(subtitle.rectTransform,65,178,560,42);
            refresh.Add(()=>{title.gameObject.SetActive(Navigation.Current==MenuScreen.Home);subtitle.gameObject.SetActive(Navigation.Current==MenuScreen.Home);});
            for(var i=0;i<2;i++)
            {
                var chip=Panel(i==0?"Gem wallet":"Life wallet",root.transform,WobblePalette.Alpha(WobblePalette.TextPrimary,.45f));
                Top(chip.GetComponent<RectTransform>(),-45,64+i*67,200,57,true);CasualSurface.Style(chip.GetComponent<Image>(),false);
                var icon=Icon(chip.transform,i==0?MenuIcon.Shape.Gem:MenuIcon.Shape.Heart);Center(icon.rectTransform,.17f,.5f,44,44);
                var isGem=i==0;var count=Text(chip.transform,()=>isGem?(economy.DemoMode?"DEMO":economy.Gems.ToString()):economy.Lives.ToString(),31,true);
                count.color=WobblePalette.Surface;Region(count.rectTransform,.34f,0,.94f,1);
            }
        }
        private void BuildHome(Transform parent,Action play)
        {
            var tagline=Text(parent,()=>loc.Text("home.tagline"),24,false);tagline.color=WobblePalette.Alpha(WobblePalette.TextPrimary,.78f);
            tagline.characterSpacing=5;Center(tagline.rectTransform,.5f,.375f,820,52);
            var button=Button(parent,"home.play",()=>loc.Text("home.play"),play,true);
            Center(button.GetComponent<RectTransform>(),.5f,.29f,820,155);
            var gloss=Panel("Play gloss",button.transform,new Color(1,1,1,.16f));
            Region(gloss.GetComponent<RectTransform>(),.035f,.56f,.965f,.94f);
            CasualSurface.Style(gloss.GetComponent<Image>(),false);
            gloss.GetComponent<Image>().raycastTarget=false;
            var bestCard=Panel("Best run plaque",parent,WobblePalette.Alpha(WobblePalette.TextPrimary,.76f));
            Center(bestCard.GetComponent<RectTransform>(),.5f,.204f,640,110);CasualSurface.Style(bestCard.GetComponent<Image>());
            var bestLabel=Text(bestCard.transform,()=>loc.Text("home.best"),22,false);bestLabel.color=WobblePalette.Surface;
            Center(bestLabel.rectTransform,.5f,.73f,600,35);
            var best=Text(bestCard.transform,()=>save.Data.bestScore.ToString(),38,true);best.color=WobblePalette.Surface;
            Center(best.rectTransform,.5f,.32f,600,48);
        }
        private void BuildNavigation()
        {
            var ids=new[]{MenuScreen.Themes,MenuScreen.Shop,MenuScreen.Daily,MenuScreen.Settings};
            var keys=new[]{"nav.themes","nav.shop","nav.daily","nav.settings"};
            var icons=new[]{MenuIcon.Shape.Layers,MenuIcon.Shape.Shop,MenuIcon.Shape.Calendar,MenuIcon.Shape.Settings};
            for(var i=0;i<4;i++)
            {
                var id=ids[i]; var key=keys[i];
                var button=Button(root.transform,key,()=>string.Empty,()=>Open(id),false);
                var rect=button.GetComponent<RectTransform>();
                rect.anchorMin=new Vector2(.045f+i*.235f,0); rect.anchorMax=new Vector2(.25f+i*.235f,0);
                rect.pivot=new Vector2(.5f,0); rect.offsetMin=new Vector2(0,54); rect.offsetMax=new Vector2(0,240);
                var icon=Icon(button.transform,icons[i]); Center(icon.rectTransform,.5f,.66f,66,66);
                var label=Text(button.transform,()=>loc.Text(key),21,true); Center(label.rectTransform,.5f,.23f,200,40);
                refresh.Add(()=>button.gameObject.SetActive(Navigation.Current==MenuScreen.Home));
            }
        }
        private Transform Content(GameObject screen,string titleKey)
        {
            var back=Button(screen.transform,"back",()=>loc.Text("common.back"),Back,false);
            Top(back.GetComponent<RectTransform>(),40,63,140,70);
            var title=Text(screen.transform,()=>loc.Text(titleKey),40,true,TextAlignmentOptions.TopLeft);
            Top(title.rectTransform,195,68,575,90);title.color=WobblePalette.Surface;
            var viewport=Panel("Scroll viewport",screen.transform,Color.clear).GetComponent<RectTransform>();
            viewport.anchorMin=new Vector2(0,.035f); viewport.anchorMax=new Vector2(1,1);
            viewport.offsetMin=new Vector2(54,0); viewport.offsetMax=new Vector2(-54,-235);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content=Panel("Content",viewport,Color.clear).GetComponent<RectTransform>();
            content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(.5f,1);
            content.anchoredPosition=Vector2.zero; content.sizeDelta=Vector2.zero;
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing=18; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
            var fitter=content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport=viewport; scroll.content=content;
            scroll.horizontal=false; scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=35;
            viewport.GetComponent<Image>().raycastTarget=true;
            return content;
        }
        private void BuildThemes(Transform content)
        {
            RectTransform row=null;
            for(var i=0;i<catalog.themes.Length;i++)
            {
                if(i%3==0)row=Row(content,360,false);
                var theme=catalog.themes[i];
                var visuals=ThemeVisuals.Get((ThemeId)i);
                var card=Panel("Theme "+theme.id,row,Color.Lerp(theme.previewTint,WobblePalette.Surface,.82f)).GetComponent<RectTransform>();
                Region(card,(i%3)/3f+.008f,0,(i%3+1)/3f-.008f,1);CasualSurface.Style(card.GetComponent<Image>());
                var strip=Panel("Theme accent",card,WobblePalette.Alpha(visuals.accent,.92f));
                Region(strip.GetComponent<RectTransform>(),.07f,.953f,.93f,.982f);CasualSurface.Style(strip.GetComponent<Image>(),false);
                strip.GetComponent<Image>().raycastTarget=false;
                var stage=Panel("Theme stage",card,Color.Lerp(visuals.skyHorizon,WobblePalette.Surface,.38f));
                Region(stage.GetComponent<RectTransform>(),.065f,.345f,.935f,.94f);CasualSurface.Style(stage.GetComponent<Image>(),false);
                stage.GetComponent<Image>().raycastTarget=false;
                var preview=new GameObject("Material study "+theme.id,typeof(RectTransform),typeof(StonePreview)).GetComponent<StonePreview>();
                preview.transform.SetParent(card,false);preview.material=Resources.Load<Material>("Menu/MenuUI");preview.stone=theme.previewTint;preview.accent=visuals.accent;preview.raycastTarget=false;
                Region(preview.rectTransform,.065f,.345f,.935f,.94f);
                var label=Text(card,()=>loc.Text(theme.nameKey),28,true);Region(label.rectTransform,.03f,.22f,.97f,.35f);
                var status=Text(card,()=>economy.DemoMode ? (save.Data.selectedTheme==theme.id ? loc.Text("themes.selected") : save.Data.ownedSkins.Contains(theme.id) ? (loc.LocaleCode=="tr"?"SEÇ":"SELECT") : (loc.LocaleCode=="tr"?"KİLİDİ AÇ · ":"UNLOCK · ")+theme.gemPrice) : theme.available?loc.Text("themes.selected"):loc.Text("themes.requirement",theme.gemPrice),18,false);
                status.color=theme.available || (economy.DemoMode && save.Data.ownedSkins.Contains(theme.id)) ? WobblePalette.Success : Muted;Region(status.rectTransform,.04f,.025f,.96f,.21f);
                refresh.Add(()=>card.GetComponent<Image>().color=save.Data.selectedTheme==theme.id?Color.Lerp(WobblePalette.Surface,visuals.accent,.16f):Color.Lerp(theme.previewTint,WobblePalette.Surface,.82f));
                if(economy.DemoMode)card.gameObject.AddComponent<Button>().onClick.AddListener(()=>{
                    if(unlockingTheme)return;
                    if(save.Data.ownedSkins.Contains(theme.id)){SelectDemoTheme(theme);return;}
                    if(economy.TrySpendGems(theme.gemPrice))StartCoroutine(UnlockTheme(theme,card));
                });
                else if(theme.available)card.gameObject.AddComponent<Button>().onClick.AddListener(()=>{save.Data.selectedTheme=theme.id;save.Save();});
            }
            if(economy.DemoMode){var note=Text(content,()=>loc.LocaleCode=="tr"?"DEMO · Tüm temalar açık · İlerleme kaydedilmez":"DEMO · All themes unlocked · Progress is not saved",22,false);note.gameObject.AddComponent<LayoutElement>().preferredHeight=70;}
            else Note(content,"themes.coming");
        }
        private void SelectDemoTheme(ThemeDefinition theme)
        {
            save.Data.selectedTheme=theme.id;save.Save();DemoThemeSelected?.Invoke(theme);
        }
        private System.Collections.IEnumerator UnlockTheme(ThemeDefinition theme,RectTransform card)
        {
            unlockingTheme=true;
            var image=card.GetComponent<Image>();var original=image.color;
            try
            {
                audioManager.MenuSlide();
                for(var t=0f;t<.65f;t+=Time.unscaledDeltaTime)
                {
                    var pulse=Mathf.Sin(Mathf.Clamp01(t/.65f)*Mathf.PI);
                    card.localScale=Vector3.one*(1+.045f*pulse);
                    image.color=Color.Lerp(original,WobblePalette.Accent,pulse*.85f);
                    yield return null;
                }
                save.Data.ownedSkins.Add(theme.id);SelectDemoTheme(theme);
            }
            finally{card.localScale=Vector3.one;image.color=original;unlockingTheme=false;}
        }
        private void BuildShop(Transform content)
        {
            foreach(var product in catalog.products)if(product.id=="starter")
            {
                var featured=Row(content,260);featured.GetComponent<Image>().color=WobblePalette.Surface;
                var icon=Icon(featured,MenuIcon.Shape.Gem);Center(icon.rectTransform,.17f,.53f,150,150);
                var title=Text(featured,()=>loc.Text(product.nameKey),37,true,TextAlignmentOptions.MidlineLeft);Region(title.rectTransform,.33f,.63f,.96f,.94f);
                var reward=Text(featured,()=>RewardText(product.gems,product.lives),23,false,TextAlignmentOptions.MidlineLeft);Region(reward.rectTransform,.33f,.35f,.96f,.64f);
                var price=Text(featured,()=>product.placeholderPrice,30,true,TextAlignmentOptions.MidlineLeft);price.color=WobblePalette.PrimaryPressed;Region(price.rectTransform,.33f,.05f,.96f,.34f);
                featured.gameObject.AddComponent<Button>().onClick.AddListener(()=>{audioManager.MenuTap();ShowProduct(product);});
            }
            foreach(var section in new[]{ProductSection.Gems,ProductSection.Lives,ProductSection.Special})
            {
                Section(content,section==ProductSection.Lives?"shop.lives":section==ProductSection.Gems?"shop.gems":"shop.special");
                RectTransform row=null;var column=0;
                foreach(var product in catalog.products)
                {
                    if(product.section!=section||product.id=="starter")continue;
                    if(column%3==0)row=Row(content,300,false);
                    var card=Panel(product.id,row,WobblePalette.Surface).GetComponent<RectTransform>();
                    Region(card,column%3/3f+.008f,0,(column%3+1)/3f-.008f,1);CasualSurface.Style(card.GetComponent<Image>());
                    var name=Text(card,()=>loc.Text(product.nameKey),24,true);Region(name.rectTransform,.04f,.73f,.96f,.96f);
                    var icon=Icon(card,section==ProductSection.Lives?MenuIcon.Shape.Heart:MenuIcon.Shape.Gem);Center(icon.rectTransform,.5f,.50f,106,106);
                    var plate=Panel("Coral price",card,WobblePalette.Primary).GetComponent<RectTransform>();Region(plate,.07f,.055f,.93f,.255f);CasualSurface.Style(plate.GetComponent<Image>(),false);
                    var price=Text(plate,()=>product.placeholderPrice,27,true);price.color=Color.white;Region(price.rectTransform,0,0,1,1);
                    card.gameObject.AddComponent<Button>().onClick.AddListener(()=>{audioManager.MenuTap();ShowProduct(product);});column++;
                }
            }
            Note(content,"shop.placeholder");
        }
        private void BuildDaily(Transform content)
        {
            Note(content,"daily.description");
            RectTransform row=null;
            for(var i=0;i<7;i++)
            {
                if(i%3==0)row=Row(content,i==6?300:260,false);
                var index=i;var reward=catalog.dailyRewards[i];
                var card=Panel("Day "+(i+1),row,i==6?WobblePalette.Accent:WobblePalette.Surface).GetComponent<RectTransform>();
                Region(card,i==6?0:i%3/3f+.008f,0,i==6?1:(i%3+1)/3f-.008f,1);CasualSurface.Style(card.GetComponent<Image>());
                var day=Text(card,()=>loc.Text("daily.day",index+1),27,true);Region(day.rectTransform,.03f,.73f,.97f,.97f);
                var icon=Icon(card,reward.gems>0?MenuIcon.Shape.Gem:MenuIcon.Shape.Heart);Center(icon.rectTransform,.5f,.48f,i==6?120:85,i==6?120:85);
                var award=Text(card,()=>RewardText(reward.gems,reward.lives),22,false);Region(award.rectTransform,.04f,.04f,.96f,.25f);
                refresh.Add(()=>day.color=index==Daily.NextIndex?WobblePalette.PrimaryPressed:White);
            }
            var claimRow=Row(content,145,false);
            var claim=Button(claimRow,"daily.claim",()=>loc.Text(Daily.CanClaim?"daily.claim":"daily.claimed"),()=>Daily.Claim(),true);
            Region(claim.GetComponent<RectTransform>(),.05f,.08f,.95f,.92f);refresh.Add(()=>claim.interactable=Daily.CanClaim);
            Note(content,"daily.local_time");
        }
        private void BuildSettings(Transform content)
        {
            Setting(content,"settings.sound",()=>loc.Text(save.Data.audioEnabled?"common.on":"common.off"),()=>{
                save.Data.audioEnabled=!save.Data.audioEnabled;audioManager.SetEnabled(save.Data.audioEnabled);save.Save();});
            Setting(content,"settings.music",()=>loc.Text(save.Data.musicEnabled?"common.on":"common.off"),()=>{
                save.Data.musicEnabled=!save.Data.musicEnabled;audioManager.SetMusicEnabled(save.Data.musicEnabled);save.Save();});
            Setting(content,"settings.haptics",()=>loc.Text(save.Data.hapticsEnabled?"common.on":"common.off"),()=>{
                save.Data.hapticsEnabled=!save.Data.hapticsEnabled;haptics.SetEnabled(save.Data.hapticsEnabled);save.Save();});
            Setting(content,"settings.quality",()=>loc.Text(QualityKey(save.Data.graphicsQuality)),ShowQuality);
            Setting(content,"settings.language",()=>loc.Text(save.Data.selectedLocale=="tr"?"language.tr":"language.en"),ShowLanguage);
            Setting(content,"settings.credits",()=>string.Empty,ShowAudioCredits);
            Note(content,"settings.music_note");
        }
        private void ShowAudioCredits()
        {
            var body=Popup("settings.credits",1100);
            var credits=Text(body,()=>Resources.Load<TextAsset>("Audio/RecordedStoneCredits").text,20,false,TextAlignmentOptions.TopLeft,popupText:true);
            Region(credits.rectTransform,.07f,.12f,.93f,.77f);
        }
        public void Open(MenuScreen screen) { ClosePopup();Navigation.Show(screen);SectionChanged?.Invoke(screen);audioManager.MenuSlide();Refresh(); }
        public void Back() { if(popup!=null)ClosePopup();else Open(MenuScreen.Home); }
        public void ShowHome() { root.SetActive(true);ClosePopup();Navigation.Show(MenuScreen.Home,true);SectionChanged?.Invoke(MenuScreen.Home);Refresh(); }
        public void Hide() { ClosePopup();root.SetActive(false); }
        private void Update()
        {
            if(root==null||!root.activeInHierarchy)return;
            if(Input.GetKeyDown(KeyCode.Escape))Back();
            if(Navigation.Current==MenuScreen.Daily && Time.unscaledTime>=nextDailyRefresh)
            {nextDailyRefresh=Time.unscaledTime+1;Refresh();}
        }
        private void OnApplicationFocus(bool focus) { if(focus)Refresh(); }
        private void Refresh()
        {
            foreach(var action in refresh)action();
            foreach(var action in popupRefresh)action();
        }
        public void SelectLanguage(string code) { loc.Select(code); ClosePopup(); Refresh(); }
        private void ShowLanguage()
        {
            var body=Popup("settings.language",500);
            var tr=Button(body,"language.tr",()=>loc.Text("language.tr"),()=>SelectLanguage("tr"),false,true); Center(tr.GetComponent<RectTransform>(),.5f,.57f,720,95);
            var en=Button(body,"language.en",()=>loc.Text("language.en"),()=>SelectLanguage("en"),false,true); Center(en.GetComponent<RectTransform>(),.5f,.36f,720,95);
        }
        private void ShowQuality()
        {
            var body=Popup("settings.quality",620);
            for(var i=0;i<3;i++)
            {
                var quality=2-i;
                var button=Button(body,QualityKey(quality),()=>loc.Text(QualityKey(quality)),()=>{
                    save.Data.graphicsQuality=quality;QualitySettings.SetQualityLevel(quality,true);save.Save();ClosePopup();},false,true);
                Center(button.GetComponent<RectTransform>(),.5f,.65f-i*.19f,720,96);
            }
        }
        private void ShowProduct(ProductDefinition product)
        {
            var body=Popup(product.nameKey,650);
            var details=Text(body,()=>RewardText(product.gems,product.lives),31,true,popupText:true);
            Center(details.rectTransform,.5f,.66f,740,80);
            var notice=Text(body,()=>loc.Text(product.futureSkinId.Length>0?"shop.skin_hook":"shop.unavailable"),23,false,popupText:true);
            Center(notice.rectTransform,.5f,.46f,730,120);
            var price=Button(body,"purchase-unavailable",()=>product.placeholderPrice+"  ·  "+loc.Text("common.soon"),()=>{},true,true);
            Center(price.GetComponent<RectTransform>(),.5f,.22f,720,105);price.interactable=false;
        }
        private Transform Popup(string titleKey,float height)
        {
            ClosePopup();
            popup=Panel("Modal",root.transform,new Color(0,0,0,.72f));
            var body=Panel("Modal body",popup.transform,WobblePalette.Surface).GetComponent<RectTransform>(); Center(body,.5f,.5f,850,height);
            var title=Text(body,()=>loc.Text(titleKey),32,true,popupText:true);Center(title.rectTransform,.5f,.87f,740,75);
            var close=Button(popup.transform,"close",()=>loc.Text("common.close"),ClosePopup,false,true);
            Center(close.GetComponent<RectTransform>(),.5f,.5f,380,80);close.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-height/2-65);
            return body;
        }
        private void ClosePopup() { popupRefresh.Clear();if(popup!=null){popup.SetActive(false);Destroy(popup);popup=null;} }
        private static string QualityKey(int value)=>value==2?"quality.high":value==1?"quality.medium":"quality.low";
        private string RewardText(int gems,int lives)
        {
            if(gems>0&&lives>0)return loc.Text("reward.both",gems,lives);
            if(gems>0)return loc.Text("reward.gems",gems);
            if(lives>0)return loc.Text("reward.lives",lives);
            return loc.Text("shop.remove_ads_detail");
        }
        private void Setting(Transform content,string key,Func<string> value,Action action)
        {
            var row=Row(content,136);row.gameObject.name=key;
            var label=Text(row,()=>loc.Text(key),28,true,TextAlignmentOptions.MidlineLeft);Region(label.rectTransform,.05f,.1f,.59f,.9f);
            var status=Text(row,value,25,false,TextAlignmentOptions.MidlineRight);status.color=WobblePalette.PrimaryPressed;Region(status.rectTransform,.60f,.1f,.94f,.9f);
            row.gameObject.AddComponent<Button>().onClick.AddListener(()=>{audioManager.MenuTap();action();});
        }
        private void Section(Transform content,string key)
        {
            var row=Row(content,64,false);var label=Text(row,()=>loc.Text(key),21,true,TextAlignmentOptions.MidlineLeft);label.color=Muted;Region(label.rectTransform,.02f,0,1,1);
        }
        private void Note(Transform content,string key)
        {
            var row=Row(content,100,false);var label=Text(row,()=>loc.Text(key),21,false,TextAlignmentOptions.MidlineLeft);label.color=Muted;Region(label.rectTransform,.02f,.05f,.98f,.95f);
        }
        private RectTransform Row(Transform content,float height,bool separator=true)
        {
            var row=Panel("Row",content,separator?WobblePalette.Surface:Color.clear).GetComponent<RectTransform>();
            row.GetComponent<Image>().raycastTarget=true;
            var layout=row.gameObject.AddComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=height;
            if(separator)CasualSurface.Style(row.GetComponent<Image>());
            return row;
        }
        private TextMeshProUGUI Text(Transform parent,Func<string> value,int size,bool bold=false,TextAlignmentOptions alignment=TextAlignmentOptions.Center,bool popupText=false)
        {
            var text=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent,false);text.font=bold?(Resources.Load<TMP_FontAsset>("Menu/WobbleDisplay")??font):font;text.fontSize=size;text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
            text.color=White;text.alignment=alignment;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;
            Action update=()=>{if(text!=null)text.text=value();};
            (popupText?popupRefresh:refresh).Add(update);update();return text;
        }
        private Button Button(Transform parent,string id,Func<string> value,Action click,bool primary,bool modal=false)
        {
            var panel=Panel(id,parent,primary?WobblePalette.Primary:WobblePalette.Surface);panel.GetComponent<Image>().raycastTarget=true;
            var button=panel.AddComponent<Button>();button.targetGraphic=panel.GetComponent<Image>();
            panel.AddComponent<StoneButtonFeedback>();
            CasualSurface.Style(panel.GetComponent<Image>());
            var colors=button.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.88f,.86f,.83f);colors.disabledColor=new Color(.82f,.82f,.82f,.65f);button.colors=colors;
            var text=Text(panel.transform,value,primary?46:26,true,popupText:modal);text.color=primary?Color.white:White;Region(text.rectTransform,.025f,.02f,.975f,.98f);
            button.onClick.AddListener(()=>{audioManager.MenuTap();click();});return button;
        }
        private MenuIcon Icon(Transform parent,MenuIcon.Shape shape)
        {
            var icon=new GameObject(shape.ToString(),typeof(RectTransform),typeof(MenuIcon)).GetComponent<MenuIcon>();
            icon.transform.SetParent(parent,false);icon.material=Resources.Load<Material>("Menu/MenuUI");icon.shape=shape;icon.color=shape==MenuIcon.Shape.Gem?WobblePalette.GemColor:shape==MenuIcon.Shape.Heart?WobblePalette.LifeColor:White;icon.raycastTarget=false;return icon;
        }
        private static GameObject Panel(string name,Transform parent,Color color)
        {
            var panel=new GameObject(name,typeof(RectTransform),typeof(Image));panel.transform.SetParent(parent,false);
            Region(panel.GetComponent<RectTransform>(),0,0,1,1);panel.GetComponent<Image>().material=Resources.Load<Material>("Menu/MenuUI");panel.GetComponent<Image>().color=color;panel.GetComponent<Image>().raycastTarget=color.a>.1f;return panel;
        }
        private static void Region(RectTransform rect,float x0,float y0,float x1,float y1)
        {rect.anchorMin=new Vector2(x0,y0);rect.anchorMax=new Vector2(x1,y1);rect.offsetMin=rect.offsetMax=Vector2.zero;}
        private static void Center(RectTransform rect,float x,float y,float width,float height)
        {rect.anchorMin=rect.anchorMax=new Vector2(x,y);rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=Vector2.zero;}
        private static void Top(RectTransform rect,float x,float y,float width,float height,bool right=false)
        {rect.anchorMin=rect.anchorMax=new Vector2(right?1:0,1);rect.pivot=new Vector2(right?1:0,1);rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=new Vector2(x,-y);}
        private void OnDestroy(){if(save!=null)save.Changed-=Refresh;if(loc!=null)loc.Changed-=Refresh;}
    }
}
