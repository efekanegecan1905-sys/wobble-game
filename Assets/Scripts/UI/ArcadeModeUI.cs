using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Save;

namespace Wobble.UI
{
    public sealed class ArcadeModeUI:MonoBehaviour
    {
        private GameManager game;private SaveManager saves;private GameObject panel;
        private Canvas canvas;private TMP_FontAsset font;private TextMeshProUGUI hud;
        private RectTransform content;
        private GameObject hudSurface;
        private UnityEngine.UI.Image balanceFill;
        private RectTransform balanceTrack;
        private void OnEnable()=>Canvas.willRenderCanvases+=LateUpdate;
        private void OnDisable()=>Canvas.willRenderCanvases-=LateUpdate;
        private void LateUpdate()
        {
            if(content==null)return;
            var bounds=((RectTransform)canvas.transform).rect;
            var safe=Screen.safeArea;
            var center=safe.center;
            content.anchorMin=content.anchorMax=new Vector2(center.x/Screen.width,center.y/Screen.height);
            content.localScale=Vector3.one*Mathf.Min(bounds.width*safe.width/Screen.width/1080f,bounds.height*safe.height/Screen.height/1900f);
        }
        private bool Turkish=>saves.Data.selectedLocale=="tr";
        private string L(string tr,string en)=>Turkish?tr:en;
        public void Initialize(GameManager g,SaveManager s,UIManager ui)
        {
            game=g;saves=s;font=Resources.Load<TMP_FontAsset>("Menu/WobbleDisplay")??Resources.Load<TMP_FontAsset>("Menu/WobbleBody");
            var root=new GameObject("Arcade interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);
            hudSurface=new GameObject("Arcade HUD",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            hudSurface.transform.SetParent(root.transform,false);
            var hudRect=(RectTransform)hudSurface.transform;hudRect.sizeDelta=new Vector2(900,170);hudRect.anchoredPosition=new Vector2(0,530);
            var background=hudSurface.GetComponent<UnityEngine.UI.Image>();background.color=new Color(.025f,.065f,.1f,.88f);background.raycastTarget=false;
            hud=Label(hudSurface.transform,"",Vector2.zero,new Vector2(860,150),38);
            balanceTrack=new GameObject("Balance meter",typeof(RectTransform),typeof(UnityEngine.UI.Image)).GetComponent<RectTransform>();
            balanceTrack.SetParent(hudRect,false);balanceTrack.sizeDelta=new Vector2(650,18);balanceTrack.anchoredPosition=new Vector2(0,-48);
            var track=balanceTrack.GetComponent<UnityEngine.UI.Image>();track.color=new Color(.25f,.32f,.38f);track.raycastTarget=false;
            balanceFill=new GameObject("Balance fill",typeof(RectTransform),typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
            balanceFill.transform.SetParent(balanceTrack,false);balanceFill.raycastTarget=false;
            var fill=balanceFill.rectTransform;fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;
            hudSurface.SetActive(false);
        }
        private TextMeshProUGUI Label(Transform parent,string value,Vector2 pos,Vector2 size,int points)
        {
            var go=new GameObject("Arcade label",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=size;rect.anchoredPosition=pos;
            var text=go.GetComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=points;text.enableAutoSizing=false;text.raycastTarget=false;text.alignment=TextAlignmentOptions.Center;text.color=Color.white;return text;
        }
        private void Surface(string title)
        {
            if(panel!=null)Destroy(panel);hud.text="";hudSurface.SetActive(false);
            panel=new GameObject("Arcade selection",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvas.transform,false);
            var rect=panel.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel.GetComponent<Image>().color=new Color(.035f,.08f,.13f,1);
            content=new GameObject("Safe arcade content",typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(panel.transform,false);content.sizeDelta=new Vector2(1080,1900);LateUpdate();
            Label(content,title,new Vector2(0,700),new Vector2(940,180),64);
            StartCoroutine(Reveal(panel.AddComponent<CanvasGroup>()));
        }
        private System.Collections.IEnumerator Reveal(CanvasGroup group)
        {
            group.blocksRaycasts=false;
            var movingContent=content;
            for(float t=0;t<.22f;t+=Time.unscaledDeltaTime){if(group==null)yield break;float eased=1-Mathf.Pow(1-t/.22f,3);group.alpha=eased;if(movingContent!=null)movingContent.anchoredPosition=Vector2.down*24*(1-eased);yield return null;}
            if(group!=null){group.alpha=1;group.blocksRaycasts=true;if(movingContent!=null)movingContent.anchoredPosition=Vector2.zero;}
        }
        private UnityEngine.UI.Button Button(string title,float y,Color color,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(content,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(900,180);rect.anchoredPosition=new Vector2(0,y);
            go.GetComponent<Image>().color=color;go.GetComponent<Button>().onClick.AddListener(action);go.AddComponent<StoneButtonFeedback>();
            CasualSurface.Style(go.GetComponent<Image>());
            Label(go.transform,title,Vector2.zero,new Vector2(860,160),36).raycastTarget=false;
            if(title.StartsWith("CLASSIC")||title.StartsWith("60 SECOND")||title.StartsWith("WOBBLE BALANCE")||title.StartsWith("TOWER TRIALS"))
            {
                int index=title.StartsWith("CLASSIC")?0:title.StartsWith("60 SECOND")?1:title.StartsWith("WOBBLE")?2:3;
                var preview=new GameObject("Animated mode preview",typeof(RectTransform));preview.transform.SetParent(go.transform,false);
                var p=preview.GetComponent<RectTransform>();p.sizeDelta=new Vector2(170,170);p.anchoredPosition=new Vector2(-350,0);
                preview.AddComponent<ArcadePreview>().Build(index,color);
                var label=go.GetComponentInChildren<TextMeshProUGUI>();label.rectTransform.sizeDelta=new Vector2(650,160);label.rectTransform.anchoredPosition=new Vector2(95,0);
            }
            return go.GetComponent<UnityEngine.UI.Button>();
        }
        public void Open()
        {
            Surface(L("OYUN MODU","GAME MODE"));
            Label(content,L("TEK DOKUNUŞ · DÖRT FARKLI MEYDAN OKUMA","ONE TAP · FOUR DIFFERENT CHALLENGES"),new Vector2(0,590),new Vector2(940,70),27);
            Button("CLASSIC\n<size=25>"+L("İstifle · Seriyi koru","Stack · Keep the combo")+"</size>\n<size=28>"+L("REKOR ","BEST ")+saves.Data.bestScore+"</size>",400,new Color(.5f,.43f,.3f),()=>game.SelectMode(ArcadeMode.Classic));
            Button("60 SECOND RUSH\n<size=25>"+L("60 saniye · 3 hata hakkı","60 seconds · 3 misses")+"</size>\n<size=28>"+L("REKOR ","BEST ")+saves.Data.rushBest+"</size>",150,new Color(.85f,.28f,.12f),()=>game.SelectMode(ArcadeMode.Rush));
            Button("WOBBLE BALANCE\n<size=25>"+L("Dengeyi bul · Kuleyi kurtar","Find balance · Save the tower")+"</size>\n<size=28>"+L("REKOR ","BEST ")+saves.Data.balanceBest+"</size>",-100,new Color(.08f,.48f,.48f),()=>game.SelectMode(ArcadeMode.Balance));
            int cleared=0;foreach(int stars in saves.Data.trialStars)if(stars>0)cleared++;
            Button("TOWER TRIALS\n<size=25>"+L("10 sınav · Her seviyede 3 yıldız","10 trials · 3 stars per level")+"</size>\n<size=28>"+cleared+" / 10</size>",-350,new Color(.38f,.24f,.6f),Trials);
            Button(L("GERİ","BACK"),-620,new Color(.2f,.3f,.4f),game.ReturnToMenu);
        }
        private int trialPage;
        private void Trials()
        {
            Surface(L("KULE SINAVLARI","TOWER TRIALS"));
            for(int i=0;i<4 && trialPage+i<10;i++)
            {
                int level=trialPage+i;var d=TrialDefinition.All[level];bool unlocked=level==0||saves.Data.trialStars[level-1]>0;
                Button((level+1)+" · "+d.blocks+L(" BLOK · "," BLOCKS · ")+d.chain+" PERFECT · "+d.seconds+"s\n"+(unlocked?saves.Data.trialStars[level]+L(" / 3 YILDIZ"," / 3 STARS"):L("KİLİTLİ","LOCKED")),400-i*230,new Color(.38f,.24f,.6f),()=>{if(unlocked)game.SelectMode(ArcadeMode.Trials,level);}).interactable=unlocked;
            }
            Button(L("DİĞER SEVİYELER","MORE LEVELS"),-600,new Color(.2f,.3f,.4f),()=>{trialPage=(trialPage+4)%12;if(trialPage>=10)trialPage=0;Trials();});
            Button(L("MOD SEÇ","SELECT MODE"),-810,new Color(.2f,.3f,.4f),Open);
        }
        public void Hide(){if(panel!=null)Destroy(panel);hud.text="";hudSurface.SetActive(false);}
        public void Refresh()
        {
            if(game.Mode==ArcadeMode.Classic){hud.text="";hudSurface.SetActive(false);return;}
            var r=game.Arcade;
            hudSurface.SetActive(panel==null && game.State==GameState.Playing);
            balanceTrack.gameObject.SetActive(game.Mode==ArcadeMode.Balance);
            hud.rectTransform.anchoredPosition=game.Mode==ArcadeMode.Balance?new Vector2(0,18):Vector2.zero;
            hud.text=game.Mode==ArcadeMode.Balance?L("DENGE ","BALANCE ")+r.Balance.x.ToString("+0.00;-0.00;0")+" / "+r.Balance.y.ToString("+0.00;-0.00;0"):
                Mathf.CeilToInt(r.Remaining)+"s  "+(r.Turbo?"TURBO  ":"")+(game.Mode==ArcadeMode.Rush?r.Misses+" / 3":r.Blocks+" / "+TrialDefinition.All[r.Trial].blocks);
            if(game.Mode==ArcadeMode.Trials)
            {
                var d=TrialDefinition.All[r.Trial];
                hud.text+="\n"+L("SERİ ","CHAIN ")+r.BestChain+"/"+d.chain+L(" · MİN. GENİŞLİK "," · MIN. WIDTH ")+d.width.ToString("0.0");
            }
            if(game.Mode==ArcadeMode.Balance)
            {
                float amount=Mathf.Clamp01(r.Balance.magnitude);
                balanceFill.rectTransform.anchorMax=new Vector2(amount,1);
                balanceFill.color=amount>=.8f?new Color(1,.4f,.25f):new Color(.2f,.85f,.73f);
                if(amount>=.8f)hud.text+="  "+L("TEHLİKE","DANGER");
            }
            hud.color=(r.Remaining<=10 && game.Mode!=ArcadeMode.Balance || r.Balance.magnitude>=.8f)?Color.Lerp(Color.white,Color.red,Mathf.PingPong(Time.unscaledTime*4,1)):Color.white;
        }
        public void Results()
        {
            Surface(L("SONUÇ","RESULT")+"\n"+game.Score);
            if(game.Mode==ArcadeMode.Trials)
            for(int i=0;i<3;i++)
            {
                var star=new GameObject("Trial star",typeof(RectTransform),typeof(MenuIcon));star.transform.SetParent(content,false);
                var rect=star.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(85,85);rect.anchoredPosition=new Vector2((i-1)*115,480);
                var icon=star.GetComponent<MenuIcon>();icon.shape=MenuIcon.Shape.Star;icon.color=i<game.Arcade.Stars?new Color(1,.73f,.2f):new Color(.25f,.3f,.35f);icon.raycastTarget=false;
            }
            Button(L("TEKRAR DENE","RETRY"),250,new Color(.85f,.28f,.12f),()=>game.SelectMode(game.Mode,game.Arcade.Trial));
            if(game.Mode==ArcadeMode.Trials && game.Arcade.Won && game.Arcade.Trial<9)Button(L("SONRAKİ SEVİYE","NEXT LEVEL"),0,new Color(.08f,.48f,.48f),()=>game.SelectMode(ArcadeMode.Trials,game.Arcade.Trial+1));
            Button(L("MOD SEÇ","SELECT MODE"),-250,new Color(.2f,.3f,.4f),game.ArcadeHome);
        }
    }
}
