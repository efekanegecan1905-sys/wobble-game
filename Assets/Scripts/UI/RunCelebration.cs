using TMPro;
using UnityEngine;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Save;
namespace Wobble.UI
{
    // One persistent presentation layer. Never owns timing, rewards or saved records.
    public sealed class RunCelebration:MonoBehaviour
    {
        private GameManager game;private SaveManager saves;private Canvas canvas;
        private RectTransform safe,result,cueRoot;private CanvasGroup resultGroup,cueGroup;
        private TextMeshProUGUI cue,caption,headline,score,comparison,reward;
        private UnityEngine.UI.Image cuePlate,rule;
        private UnityEngine.UI.Button next;
        private readonly MenuIcon[] stars=new MenuIcon[3];
        private readonly RectTransform[] sparks=new RectTransform[18];
        private readonly UnityEngine.UI.Image[] sparkImages=new UnityEngine.UI.Image[18];
        private float cueTime=10,resultTime,sparkTime=10;private int shownScore=-1;
        private RunRecord record;private bool activeResult;private float balanceValue;
        private ArcadeGauge resultGauge;
        public int ResultCount {get;private set;}
        private bool TR=>saves.Data.selectedLocale=="tr";
        private string L(string tr,string en)=>TR?tr:en;
        public void Initialize(GameManager owner,SaveManager store)
        {
            game=owner;saves=store;
            var root=new GameObject("Gameplay celebrations",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));root.transform.SetParent(transform,false);
            canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=60;
            var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);
            safe=ArcadeStyle.Rect("Safe gameplay celebrations",root.transform,Vector2.zero,new Vector2(1080,1900));
            result=ArcadeStyle.Rect("Run result",safe,Vector2.zero,new Vector2(1080,1900));resultGroup=result.gameObject.AddComponent<CanvasGroup>();
            var bg=ArcadeStyle.Surface("Result backdrop",result,Vector2.zero,new Vector2(4000,5000),ArcadeStyle.Mist,false);bg.raycastTarget=true;bg.transform.SetAsFirstSibling();
            caption=Text("Result mode",result,780,56,34,false);
            rule=ArcadeStyle.Surface("Record accent",result,new Vector2(0,715),new Vector2(120,8),ArcadeStyle.Gold);
            headline=Text("Record headline",result,565,180,72,true);
            score=Text("Final score",result,340,200,150,false);
            comparison=Text("Previous record comparison",result,180,90,32,false);
            for(int i=0;i<3;i++){var r=ArcadeStyle.Rect("Earned star "+i,result,new Vector2((i-1)*150,-5),new Vector2(106,106));stars[i]=r.gameObject.AddComponent<MenuIcon>();stars[i].shape=MenuIcon.Shape.Star;stars[i].raycastTarget=false;}
            var dial=ArcadeStyle.Rect("Settled balance",result,new Vector2(0,-15),new Vector2(400,140));resultGauge=dial.gameObject.AddComponent<ArcadeGauge>();resultGauge.raycastTarget=false;
            reward=Text("Earned reward",result,-160,100,32,false);
            Button("Retry run",result,-375,ArcadeStyle.Coral,()=>game.SelectMode(game.Mode,game.Arcade.Trial));
            next=Button("Next level",result,-550,ArcadeStyle.Gold,()=>game.SelectMode(ArcadeMode.Trials,game.Arcade.Trial+1));
            Button("Choose mode",result,-770,ArcadeStyle.Cream,game.ArcadeHome);
            cueRoot=ArcadeStyle.Rect("Nonblocking gameplay cue",safe,new Vector2(0,275),new Vector2(930,180));cueGroup=cueRoot.gameObject.AddComponent<CanvasGroup>();cueGroup.blocksRaycasts=false;
            cuePlate=ArcadeStyle.Surface("Cue contrast",cueRoot,Vector2.zero,new Vector2(930,155),ArcadeStyle.Ink);
            cue=Text("Countdown and achievement",cueRoot,0,230,76,true);cue.color=ArcadeStyle.Cream;
            var particles=ArcadeStyle.Rect("Reusable reward sparks",safe,new Vector2(0,400),new Vector2(1000,600));
            for(int i=0;i<sparks.Length;i++){var p=ArcadeStyle.Surface("Reward spark "+i,particles,Vector2.zero,new Vector2(8,20),ArcadeStyle.Gold,false);sparks[i]=p.rectTransform;sparkImages[i]=p;p.gameObject.SetActive(false);}
            Reset();
        }
        private TextMeshProUGUI Text(string name,Transform parent,float y,float h,float size,bool expressive)
        {var t=ArcadeStyle.Text(name,parent,"",new Vector2(0,y),new Vector2(940,h),size);GameplayTypography.Apply(t,expressive);return t;}
        private UnityEngine.UI.Button Button(string name,Transform parent,float y,Color color,UnityEngine.Events.UnityAction action)
        {var b=ArcadeStyle.Button(name,parent,"",new Vector2(0,y),new Vector2(850,120),color,action);GameplayTypography.Apply(b.GetComponentInChildren<TextMeshProUGUI>(),true);return b;}
        private void OnEnable()=>Canvas.willRenderCanvases+=Fit;
        private void OnDisable()=>Canvas.willRenderCanvases-=Fit;
        private void Fit(){if(canvas!=null)ArcadeStyle.Fit(safe,canvas);}
        public void Reset()
        {activeResult=false;if(result!=null)result.gameObject.SetActive(false);cueTime=sparkTime=10;if(cueGroup!=null)cueGroup.alpha=0;if(cueRoot!=null)cueRoot.anchoredPosition=new Vector2(0,275);foreach(var s in sparks)if(s!=null)s.gameObject.SetActive(false);}
        public void Countdown(int seconds)
        {
            string message=seconds==15?L("SON 15 SANİYE","15 SECONDS LEFT"):seconds==10?(game.Mode==ArcadeMode.Rush?L("SON ATAK!","FINAL RUSH!"):L("SON SANİYELER!","FINAL SECONDS!")):seconds==0?L("SÜRE DOLDU","TIME UP"):seconds.ToString();
            Announce(message,seconds<=3&&seconds>0?148:72,ArcadeStyle.Coral);
        }
        public void Placement(bool perfect,int chain)
        {
            if(!perfect||game.Mode==ArcadeMode.Classic)return;
            // Urgency owns this lane during the final seconds.
            if(game.Arcade.Remaining>0&&game.Arcade.Remaining<=10)return;
            Announce(chain>=8?"FEVER ×"+chain:chain>=5?"FLOW ×"+chain:"PERFECT",52,ArcadeStyle.Accent(game.Mode));
        }
        public void Recovered()=>Announce(L("DENGE GERİ GELDİ","STABILITY RESTORED"),58,ArcadeStyle.Violet);
        private void Announce(string text,float size,Color accent)
        {cue.text=text;cue.fontSize=size;cue.color=Color.Lerp(accent,Color.white,.6f);cueTime=0;cueGroup.alpha=1;cueRoot.gameObject.SetActive(true);}
        public void Show(RunRecord outcome,float balance=0)
        {
            if(activeResult)return;
            record=outcome;activeResult=true;ResultCount++;resultTime=0;shownScore=-1;balanceValue=balance;
            result.gameObject.SetActive(true);resultGroup.alpha=1;
            // Expiry feedback uses the empty next-level slot, never covering the score.
            if(game.Arcade.TimedOut)cueRoot.anchoredPosition=new Vector2(0,-560);
            else {cueTime=10;cueGroup.alpha=0;}
            caption.text=outcome.Mode==ArcadeMode.Rush?"60 SECOND RUSH":outcome.Mode==ArcadeMode.Balance?"WOBBLE BALANCE":L("KULE SINAVI ","TOWER TRIAL ")+(outcome.Trial+1);
            headline.text=outcome.Improved?L("YENİ REKOR!","NEW BEST!"):outcome.Mode==ArcadeMode.Trials&&outcome.Won?L("SEVİYE TAMAM!","LEVEL COMPLETE!"):outcome.NearRecord?L("ÇOK YAKLAŞTIN!","SO CLOSE!"):L("BİR TUR DAHA?","ONE MORE RUN?");
            headline.color=outcome.Improved?Color.Lerp(ArcadeStyle.Gold,ArcadeStyle.Ink,.55f):ArcadeStyle.Ink;
            rule.color=ArcadeStyle.Accent(outcome.Mode);
            comparison.text=outcome.Mode==ArcadeMode.Trials?L("ÖNCEKİ ","PREVIOUS ")+outcome.Previous+" / 3   ·   "+L("EN İYİ ","BEST ")+outcome.Best+" / 3":L("ÖNCEKİ REKOR ","PREVIOUS BEST ")+outcome.Previous+"   ·   "+L("EN İYİ ","BEST ")+outcome.Best;
            reward.text=outcome.FirstClear?L("İLK ZAFER · +5 ELMAS","FIRST CLEAR · +5 GEMS"):outcome.Improved?L("Bu rekor sana ait.","A new personal milestone."):L("Bir sonraki zirve seni bekliyor.","Your next summit is waiting.");
            foreach(var star in stars)star.gameObject.SetActive(outcome.Mode==ArcadeMode.Trials);
            resultGauge.gameObject.SetActive(outcome.Mode==ArcadeMode.Balance);
            next.gameObject.SetActive(outcome.Mode==ArcadeMode.Trials&&outcome.Won&&outcome.Trial<9);
            result.Find("Retry run").GetComponentInChildren<TextMeshProUGUI>().text=L("TEKRAR OYNA","PLAY AGAIN");
            result.Find("Next level").GetComponentInChildren<TextMeshProUGUI>().text=L("SONRAKİ SEVİYE","NEXT LEVEL");
            result.Find("Choose mode").GetComponentInChildren<TextMeshProUGUI>().text=L("MODLARA DÖN","BACK TO MODES");
            if(outcome.Improved)Spark();
        }
        public void ClassicRecord(RunRecord outcome)
        {if(outcome.Improved)Spark();}
        private void Spark(){sparkTime=0;for(int i=0;i<sparks.Length;i++)sparks[i].gameObject.SetActive(true);}
        private void Update()
        {
            if(canvas==null)return;
            cueTime+=Time.unscaledDeltaTime;
            if(cueGroup.alpha>0)
            {
                float t=Mathf.Clamp01(cueTime/.24f);float u=t-1;float pop=1+2.2f*u*u*u+1.2f*u*u;
                cueRoot.localScale=Vector3.one*Mathf.Lerp(.8f,1,pop);cueGroup.alpha=1-Mathf.Clamp01((cueTime-.45f)/.22f);
            }
            if(activeResult)
            {
                resultTime+=Time.unscaledDeltaTime;
                float t=Mathf.Clamp01(resultTime/.26f);headline.rectTransform.localScale=Vector3.one*(1+.10f*Mathf.Sin(t*Mathf.PI));
                int value=Mathf.RoundToInt(record.Score*ArcadeStyle.Ease(resultTime/.65f));if(value!=shownScore){shownScore=value;score.text=value.ToString();}
                for(int i=0;i<stars.Length;i++){float p=ArcadeStyle.Ease((resultTime-i*.17f)/.3f);stars[i].transform.localScale=Vector3.one*Mathf.Max(.01f,p);stars[i].color=i<record.Value?ArcadeStyle.Gold:Color.Lerp(ArcadeStyle.Mist,ArcadeStyle.Ink,.18f);}
                reward.alpha=ArcadeStyle.Ease((resultTime-.35f)/.25f);
                resultGauge.Value=Mathf.Lerp(balanceValue,0,ArcadeStyle.Ease(resultTime/.7f));
            }
            if(sparkTime<.85f)
            {
                sparkTime+=Time.unscaledDeltaTime;float t=sparkTime/.85f;
                for(int i=0;i<sparks.Length;i++){float a=i*2.39996f;var r=sparks[i];r.anchoredPosition=new Vector2(Mathf.Cos(a)*t*450,Mathf.Sin(a)*t*180-t*t*180);r.localRotation=Quaternion.Euler(0,0,i*37+t*130);sparkImages[i].color=new Color(ArcadeStyle.Gold.r,ArcadeStyle.Gold.g,ArcadeStyle.Gold.b,1-t);if(t>=1)r.gameObject.SetActive(false);}
            }
        }
    }
}
