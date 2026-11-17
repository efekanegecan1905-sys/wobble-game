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
        private bool Turkish=>saves.Data.selectedLocale=="tr";
        private string L(string tr,string en)=>Turkish?tr:en;
        public void Initialize(GameManager g,SaveManager s,UIManager ui)
        {
            game=g;saves=s;font=Resources.Load<TMP_FontAsset>("Menu/WobbleDisplay")??Resources.Load<TMP_FontAsset>("Menu/WobbleBody");
            var root=new GameObject("Arcade interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);
            hud=Label(root.transform,"",new Vector2(0,530),new Vector2(900,180),40);hud.raycastTarget=false;
        }
        private TextMeshProUGUI Label(Transform parent,string value,Vector2 pos,Vector2 size,int points)
        {
            var go=new GameObject("Arcade label",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=size;rect.anchoredPosition=pos;
            var text=go.GetComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=points;text.alignment=TextAlignmentOptions.Center;text.color=Color.white;return text;
        }
        private void Surface(string title)
        {
            if(panel!=null)Destroy(panel);hud.text="";
            panel=new GameObject("Arcade selection",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvas.transform,false);
            var rect=panel.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel.GetComponent<Image>().color=new Color(.035f,.08f,.13f,.98f);
            Label(panel.transform,title,new Vector2(0,700),new Vector2(940,180),64);
            StartCoroutine(Reveal(panel.AddComponent<CanvasGroup>()));
        }
        private System.Collections.IEnumerator Reveal(CanvasGroup group)
        {
            group.blocksRaycasts=false;
            for(float t=0;t<.22f;t+=Time.unscaledDeltaTime){if(group==null)yield break;group.alpha=t/.22f;yield return null;}
            if(group!=null){group.alpha=1;group.blocksRaycasts=true;}
        }
        private void Button(string title,float y,Color color,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(panel.transform,false);
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
        }
        public void Open()
        {
            Surface(L("OYUN MODU","GAME MODE"));
            Button("CLASSIC\n"+L("REKOR ","BEST ")+saves.Data.bestScore,400,new Color(.5f,.43f,.3f),()=>game.SelectMode(ArcadeMode.Classic));
            Button("60 SECOND RUSH\n"+L("REKOR ","BEST ")+saves.Data.rushBest,150,new Color(.85f,.28f,.12f),()=>game.SelectMode(ArcadeMode.Rush));
            Button("WOBBLE BALANCE\n"+L("REKOR ","BEST ")+saves.Data.balanceBest,-100,new Color(.08f,.48f,.48f),()=>game.SelectMode(ArcadeMode.Balance));
            int cleared=0;foreach(int stars in saves.Data.trialStars)if(stars>0)cleared++;
            Button("TOWER TRIALS\n"+cleared+" / 10",-350,new Color(.38f,.24f,.6f),Trials);
            Button(L("GERİ","BACK"),-620,new Color(.2f,.3f,.4f),game.ReturnToMenu);
        }
        private int trialPage;
        private void Trials()
        {
            Surface(L("KULE SINAVLARI","TOWER TRIALS"));
            for(int i=0;i<4 && trialPage+i<10;i++)
            {
                int level=trialPage+i;var d=TrialDefinition.All[level];bool unlocked=level==0||saves.Data.trialStars[level-1]>0;
                Button((level+1)+" · "+d.blocks+L(" BLOK · "," BLOCKS · ")+d.chain+" PERFECT · "+d.seconds+"s\n"+(unlocked?saves.Data.trialStars[level]+L(" / 3 YILDIZ"," / 3 STARS"):L("KİLİTLİ","LOCKED")),400-i*230,new Color(.38f,.24f,.6f),()=>{if(unlocked)game.SelectMode(ArcadeMode.Trials,level);});
            }
            Button(L("DİĞER SEVİYELER","MORE LEVELS"),-600,new Color(.2f,.3f,.4f),()=>{trialPage=(trialPage+4)%12;if(trialPage>=10)trialPage=0;Trials();});
            Button(L("MOD SEÇ","SELECT MODE"),-810,new Color(.2f,.3f,.4f),Open);
        }
        public void Hide(){if(panel!=null)Destroy(panel);hud.text="";}
        public void Refresh()
        {
            if(game.Mode==ArcadeMode.Classic){hud.text="";return;}
            var r=game.Arcade;
            hud.text=game.Mode==ArcadeMode.Balance?L("DENGE ","BALANCE ")+r.Balance.x.ToString("+0.00;-0.00;0")+" / "+r.Balance.y.ToString("+0.00;-0.00;0"):
                Mathf.CeilToInt(r.Remaining)+"s  "+(r.Turbo?"TURBO  ":"")+(game.Mode==ArcadeMode.Rush?r.Misses+" / 3":r.Blocks+" / "+TrialDefinition.All[r.Trial].blocks);
            if(game.Mode==ArcadeMode.Trials)
            {
                var d=TrialDefinition.All[r.Trial];
                hud.text+="\n"+L("SERİ ","CHAIN ")+r.BestChain+"/"+d.chain+L(" · GENİŞLİK ≥ "," · WIDTH ≥ ")+d.width.ToString("0.0");
            }
            if(game.Mode==ArcadeMode.Balance)
            {
                int bars=Mathf.Clamp(Mathf.RoundToInt(r.Balance.magnitude*10),0,10);
                hud.text+="\n"+new string('■',bars)+new string('□',10-bars)+(bars>=8?L("  TEHLİKE","  DANGER"):"");
            }
            hud.color=(r.Remaining<=10 && game.Mode!=ArcadeMode.Balance || r.Balance.magnitude>=.8f)?Color.Lerp(Color.white,Color.red,Mathf.PingPong(Time.unscaledTime*4,1)):Color.white;
        }
        public void Results()
        {
            Surface(L("SONUÇ","RESULT")+"\n"+game.Score);
            if(game.Mode==ArcadeMode.Trials)
            for(int i=0;i<3;i++)
            {
                var star=new GameObject("Trial star",typeof(RectTransform),typeof(MenuIcon));star.transform.SetParent(panel.transform,false);
                var rect=star.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(85,85);rect.anchoredPosition=new Vector2((i-1)*115,480);
                var icon=star.GetComponent<MenuIcon>();icon.shape=MenuIcon.Shape.Star;icon.color=i<game.Arcade.Stars?new Color(1,.73f,.2f):new Color(.25f,.3f,.35f);icon.raycastTarget=false;
            }
            Button(L("TEKRAR DENE","RETRY"),250,new Color(.85f,.28f,.12f),()=>game.SelectMode(game.Mode,game.Arcade.Trial));
            if(game.Mode==ArcadeMode.Trials && game.Arcade.Won && game.Arcade.Trial<9)Button(L("SONRAKİ SEVİYE","NEXT LEVEL"),0,new Color(.08f,.48f,.48f),()=>game.SelectMode(ArcadeMode.Trials,game.Arcade.Trial+1));
            Button(L("MOD SEÇ","SELECT MODE"),-250,new Color(.2f,.3f,.4f),game.ArcadeHome);
        }
    }
}
