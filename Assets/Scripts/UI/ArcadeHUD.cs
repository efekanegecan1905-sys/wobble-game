using TMPro;
using UnityEngine;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Save;
namespace Wobble.UI
{
    public sealed class ArcadeHUD:MonoBehaviour
    {
        private Canvas canvas;private GameManager game;private SaveManager saves;private RectTransform root;
        private GameObject rush,balance,trials;
        private TextMeshProUGUI timer,rushScore,rushPhase,rushHint,balanceScore,balanceState,balanceHint,trialTitle,trialBlocks,trialChain,trialTimer,trialWidth;
        private UnityEngine.UI.Image timeFill,urgency;
        private readonly UnityEngine.UI.Image[] misses=new UnityEngine.UI.Image[3];
        private readonly MenuIcon[] milestones=new MenuIcon[3];
        private ArcadeGauge gauge;private float needle,needleVelocity,scorePunch;private int previousScore;
        private bool TR=>saves.Data.selectedLocale=="tr";private string L(string tr,string en)=>TR?tr:en;
        public void Initialize(Canvas c,GameManager g,SaveManager s)
        {
            canvas=c;game=g;saves=s;root=ArcadeStyle.Rect("Dedicated arcade HUD",canvas.transform,Vector2.zero,new Vector2(1080,1900));
            rush=ArcadeStyle.Rect("Rush HUD",root,Vector2.zero,new Vector2(1080,1900)).gameObject;
            balance=ArcadeStyle.Rect("Balance HUD",root,Vector2.zero,new Vector2(1080,1900)).gameObject;
            trials=ArcadeStyle.Rect("Trials HUD",root,Vector2.zero,new Vector2(1080,1900)).gameObject;
            BuildRush();BuildBalance();BuildTrials();
            foreach(var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))GameplayTypography.Apply(text);
            Hide();
        }
        private void OnEnable()=>Canvas.willRenderCanvases+=Fit;
        private void OnDisable()=>Canvas.willRenderCanvases-=Fit;
        private void Fit(){if(root!=null)ArcadeStyle.Fit(root,canvas);}
        private TextMeshProUGUI Text(Transform parent,string name,string value,float x,float y,float w,float h,float size,bool display=true)=>ArcadeStyle.Text(name,parent,value,new Vector2(x,y),new Vector2(w,h),size,display);
        private void BuildRush()
        {
            var p=rush.transform;
            ArcadeStyle.Surface("Rush header",p,new Vector2(0,700),new Vector2(980,400),ArcadeStyle.Cream);
            urgency=ArcadeStyle.Surface("Urgency accent",p,new Vector2(-455,700),new Vector2(10,320),ArcadeStyle.Coral);
            Text(p,"Rush name","60 SECOND RUSH",0,845,850,65,36);
            timer=Text(p,"Rush countdown","60",-240,695,420,200,140);
            Text(p,"Seconds unit","s",-65,655,60,90,40,false);
            rushScore=Text(p,"Rush points","0",235,730,380,100,68);
            rushPhase=Text(p,"Rush phase","",235,625,420,95,28);
            for(int i=0;i<3;i++)misses[i]=ArcadeStyle.Surface("Free miss "+i,p,new Vector2(160+i*75,535),new Vector2(48,12),ArcadeStyle.Coral);
            var track=ArcadeStyle.Surface("Rush time track",p,new Vector2(0,472),new Vector2(920,12),ArcadeStyle.Cream);
            timeFill=ArcadeStyle.Surface("Rush time remaining",track.transform,Vector2.zero,Vector2.zero,ArcadeStyle.Coral);
            timeFill.rectTransform.anchorMin=Vector2.zero;timeFill.rectTransform.anchorMax=Vector2.one;timeFill.rectTransform.offsetMin=timeFill.rectTransform.offsetMax=Vector2.zero;
            ArcadeStyle.Surface("Rush hint contrast",p,new Vector2(0,405),new Vector2(980,85),ArcadeStyle.Cream);
            rushHint=Text(p,"Rush urgency hint","",0,405,940,75,27);
        }
        private void BuildBalance()
        {
            var p=balance.transform;
            ArcadeStyle.Surface("Balance instrument",p,new Vector2(0,690),new Vector2(980,410),ArcadeStyle.Cream);
            Text(p,"Balance title","WOBBLE BALANCE",-100,845,700,60,37);
            balanceScore=Text(p,"Balance points","",345,778,200,70,42);
            var r=ArcadeStyle.Rect("Stability dial",p,new Vector2(-60,650),new Vector2(690,230));gauge=r.gameObject.AddComponent<ArcadeGauge>();gauge.raycastTarget=false;
            balanceState=Text(p,"Stability state","",0,520,880,65,35);
            ArcadeStyle.Surface("Balance hint contrast",p,new Vector2(0,405),new Vector2(980,95),ArcadeStyle.Cream);
            balanceHint=Text(p,"Counterbalance guidance","",0,405,940,90,27,false);
        }
        private void BuildTrials()
        {
            var p=trials.transform;
            ArcadeStyle.Surface("Trials objective sheet",p,new Vector2(0,690),new Vector2(980,420),ArcadeStyle.Cream);
            trialTitle=Text(p,"Trial level title","",-100,845,700,60,37);
            trialTimer=Text(p,"Trial time limit","",370,840,170,70,34);
            trialBlocks=Text(p,"Block objective","",-240,705,410,170,62);
            trialChain=Text(p,"Perfect objective","",240,705,410,170,52);
            trialWidth=Text(p,"Width objective","",0,555,880,60,27,false);
            for(int i=0;i<3;i++){var r=ArcadeStyle.Rect("Trial milestone "+i,p,new Vector2((i-1)*85,440),new Vector2(54,54));milestones[i]=r.gameObject.AddComponent<MenuIcon>();milestones[i].shape=MenuIcon.Shape.Star;milestones[i].raycastTarget=false;}
        }
        public void Hide(){if(root!=null)root.gameObject.SetActive(false);needle=needleVelocity=scorePunch=0;previousScore=0;}
        public void Refresh()
        {
            bool visible=game.State==GameState.Playing&&game.Mode!=ArcadeMode.Classic;root.gameObject.SetActive(visible);if(!visible)return;
            rush.SetActive(game.Mode==ArcadeMode.Rush);balance.SetActive(game.Mode==ArcadeMode.Balance);trials.SetActive(game.Mode==ArcadeMode.Trials);
            var run=game.Arcade;
            if(previousScore!=game.Score){previousScore=game.Score;scorePunch=.18f;}
            scorePunch=Mathf.Max(0,scorePunch-Time.unscaledDeltaTime);
            rushScore.rectTransform.localScale=Vector3.one*(1+.09f*Mathf.Sin(scorePunch/.18f*Mathf.PI));
            if(game.Mode==ArcadeMode.Rush)
            {
                int seconds=Mathf.CeilToInt(run.Remaining);timer.text=seconds.ToString("00");rushScore.text=game.Score.ToString();
                rushPhase.text=run.Turbo?"TURBO\n<size=22>"+L("TAM GAZ","FULL SPEED")+"</size>":L("PUAN\n<size=22>SERİNİ BÜYÜT</size>","POINTS\n<size=22>BUILD YOUR COMBO</size>");
                timeFill.rectTransform.anchorMax=new Vector2(run.Remaining/run.Tuning.rushDuration,1);
                for(int i=0;i<3;i++)misses[i].color=i<run.Misses?Color.Lerp(ArcadeStyle.Cream,ArcadeStyle.Ink,.2f):ArcadeStyle.Coral;
                bool final=seconds<=15;float beat=final?Mathf.Pow(1-Mathf.Repeat(run.Remaining,1),4):0;
                timer.color=final?Color.Lerp(ArcadeStyle.Ink,ArcadeStyle.Coral,.7f+beat*.3f):ArcadeStyle.Ink;timer.rectTransform.localScale=Vector3.one*(1+beat*.045f);
                urgency.color=run.Turbo?ArcadeStyle.Coral:ArcadeStyle.Aqua;
                rushHint.text=final?L("SON ","FINAL ")+seconds+L(" SANİYE · ŞİMDİ!"," SECONDS · GO!"):L("KALAN HATA HAKKI  ","MISSES LEFT  ")+(3-run.Misses);
            }
            else if(game.Mode==ArcadeMode.Balance)
            {
                float signed=run.Balance.x+run.Balance.y*.65f;float target=Mathf.Sign(signed)*Mathf.Clamp01(run.Balance.magnitude);
                if(Time.deltaTime>0)needle=Mathf.SmoothDamp(needle,target,ref needleVelocity,.18f,Mathf.Infinity,Time.deltaTime);
                gauge.Value=needle;
                balanceScore.text=game.Score.ToString();bool danger=run.Balance.magnitude>=.8f;
                balanceState.text=danger?L("DENGE KRİTİK","CRITICAL BALANCE"):L("DENGE ","STABILITY ")+Mathf.RoundToInt((1-Mathf.Clamp01(run.Balance.magnitude))*100)+"%";
                balanceState.color=danger?ArcadeStyle.Coral:ArcadeStyle.Ink;
                balanceHint.text=run.Balance.magnitude<.12f?L("Merkezdesin. Hassas yerleştirmeye devam et.","Centered. Keep placing with precision."):
                    L("Ağırlığın ters yönüne yerleştir.","Place against the weight to recover.");
            }
            else
            {
                var d=TrialDefinition.All[run.Trial];trialTitle.text=L("SINAV ","TRIAL ")+(run.Trial+1).ToString("00")+" / 10";trialTimer.text=Mathf.CeilToInt(run.Remaining)+"s";
                trialBlocks.text=run.Blocks+" / "+d.blocks+"\n<size=25>"+L("BLOK HEDEFİ","BLOCK OBJECTIVE")+"</size>";
                trialChain.text=(d.chain>0?run.BestChain+" / "+d.chain:L("SERBEST","FREE"))+"\n<size=25>"+L("PERFECT SERİSİ","PERFECT CHAIN")+"</size>";
                trialWidth.text=L("GENİŞLİK  ","WIDTH  ")+game.TowerWidth.ToString("0.0")+L("   /   MİN. ","   /   MIN. ")+d.width.ToString("0.0")+"   ·   "+L("KULENİ KORU","KEEP YOUR TOWER");
                milestones[0].color=run.Blocks>=d.blocks?ArcadeStyle.Gold:Color.Lerp(ArcadeStyle.Cream,ArcadeStyle.Ink,.3f);
                milestones[1].color=run.BestChain>=d.chain?ArcadeStyle.Gold:Color.Lerp(ArcadeStyle.Cream,ArcadeStyle.Ink,.3f);
                milestones[2].color=run.Remaining>=d.seconds*.25f?ArcadeStyle.Gold:Color.Lerp(ArcadeStyle.Cream,ArcadeStyle.Ink,.3f);
            }
        }
    }
}
