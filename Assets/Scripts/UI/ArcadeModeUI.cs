using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wobble.Core;
using Wobble.Gameplay;
using Wobble.Save;

namespace Wobble.UI
{
    public sealed class ArcadeModeUI:MonoBehaviour,IBeginDragHandler,IEndDragHandler
    {
        private GameManager game;private SaveManager saves;private Canvas canvas;
        private RectTransform content,showcase,levels,result,hero,details;
        private GameObject backdrop;private CanvasGroup group,heroGroup;
        private TextMeshProUGUI heading,title,description,record,eyebrow,previewCaption,playLabel,resultTitle,resultScore,resultReward;
        private ArcadePreview preview;private ArcadeHUD modeHud;
        private ArcadeGauge previewGauge;
        private UnityEngine.UI.Image accentRule;private UnityEngine.UI.Button next;
        private readonly UnityEngine.UI.Image[] dots=new UnityEngine.UI.Image[4];
        private readonly MenuIcon[] previewStars=new MenuIcon[3],resultStars=new MenuIcon[3];
        private readonly UnityEngine.UI.Button[] levelButtons=new UnityEngine.UI.Button[4];
        private Coroutine transition;private Vector2 dragStart;private int selected,trialPage;
        private bool victoryPlayed;private float resultTime;
        public ArcadeMode SelectedMode=>(ArcadeMode)selected;
        private bool Turkish=>saves.Data.selectedLocale=="tr";
        private string L(string tr,string en)=>Turkish?tr:en;
        private static readonly string[] Titles={"CLASSIC","60 SECOND\nRUSH","WOBBLE\nBALANCE","TOWER TRIALS"};
        public void Initialize(GameManager g,SaveManager s,UIManager ui)
        {
            game=g;saves=s;
            var root=new GameObject("Arcade interface",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));root.transform.SetParent(transform,false);
            canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);
            var bg=ArcadeStyle.Surface("Bright arcade sky",root.transform,Vector2.zero,Vector2.zero,ArcadeStyle.Mist,false);backdrop=bg.gameObject;
            bg.raycastTarget=true;bg.rectTransform.anchorMin=Vector2.zero;bg.rectTransform.anchorMax=Vector2.one;bg.rectTransform.offsetMin=bg.rectTransform.offsetMax=Vector2.zero;
            content=ArcadeStyle.Rect("Safe arcade content",backdrop.transform,Vector2.zero,new Vector2(1080,1900));group=backdrop.AddComponent<CanvasGroup>();
            heading=ArcadeStyle.Text("Section heading",content,"",new Vector2(0,820),new Vector2(880,80),42);
            accentRule=ArcadeStyle.Surface("Mode accent rule",content,new Vector2(0,755),new Vector2(64,7),ArcadeStyle.Aqua);
            BuildShowcase();BuildLevels();BuildResults();
            modeHud=gameObject.AddComponent<ArcadeHUD>();modeHud.Initialize(canvas,game,saves);
            Hide();
        }
        private void OnEnable()=>Canvas.willRenderCanvases+=Fit;
        private void OnDisable(){Canvas.willRenderCanvases-=Fit;CancelTransition();}
        private void Fit(){if(content!=null)ArcadeStyle.Fit(content,canvas);}
        private MenuIcon Star(Transform parent,Vector2 position,int size)
        {
            var r=ArcadeStyle.Rect("Achievement star",parent,position,new Vector2(size,size));var star=r.gameObject.AddComponent<MenuIcon>();star.shape=MenuIcon.Shape.Star;star.raycastTarget=false;star.color=ArcadeStyle.Gold;return star;
        }
        private void BuildShowcase()
        {
            showcase=ArcadeStyle.Rect("Swipeable mode showcase",content,Vector2.zero,new Vector2(1080,1900));
            eyebrow=ArcadeStyle.Text("Mode index",showcase,"",new Vector2(0,655),new Vector2(840,60),26,false);
            hero=ArcadeStyle.Rect("Hero preview",showcase,new Vector2(0,210),new Vector2(880,800));heroGroup=hero.gameObject.AddComponent<CanvasGroup>();
            // A quiet architectural halo frames the stage without another render texture.
            var halo=ArcadeStyle.Surface("Cloud platform",hero,new Vector2(0,-135),new Vector2(860,410),ArcadeStyle.Cream);
            var stage=ArcadeStyle.Rect("Selected 3D mode",hero,new Vector2(0,45),new Vector2(850,850));preview=stage.gameObject.AddComponent<ArcadePreview>();preview.Build(0,ArcadeStyle.Aqua);
            previewCaption=ArcadeStyle.Text("Animated preview cue",hero,"",new Vector2(0,-310),new Vector2(780,74),32);
            var dial=ArcadeStyle.Rect("Preview stability dial",hero,new Vector2(0,-230),new Vector2(300,100));previewGauge=dial.gameObject.AddComponent<ArcadeGauge>();previewGauge.raycastTarget=false;
            for(int i=0;i<3;i++)previewStars[i]=Star(hero,new Vector2((i-1)*75,-310),48);
            details=ArcadeStyle.Rect("Mode copy",showcase,Vector2.zero,new Vector2(1000,600));
            title=ArcadeStyle.Text("Selected mode title",details,"",new Vector2(0,-275),new Vector2(940,190),76);title.lineSpacing=-12;
            description=ArcadeStyle.Text("Mode description",details,"",new Vector2(0,-410),new Vector2(890,85),30,false);
            record=ArcadeStyle.Text("Personal record",details,"",new Vector2(0,-500),new Vector2(850,56),28);
            for(int i=0;i<4;i++)
            {
                int index=i;var dot=ArcadeStyle.Button("Mode page "+i,showcase,"",new Vector2((i-1.5f)*60,-575),new Vector2(40,34),ArcadeStyle.Cream,()=>SelectPreview(index));dots[i]=dot.GetComponent<UnityEngine.UI.Image>();
            }
            ArcadeStyle.Button("Previous mode",showcase,"‹",new Vector2(-445,120),new Vector2(95,120),ArcadeStyle.Cream,()=>Navigate(-1)).GetComponentInChildren<TextMeshProUGUI>().fontSize=64;
            ArcadeStyle.Button("Next mode",showcase,"›",new Vector2(445,120),new Vector2(95,120),ArcadeStyle.Cream,()=>Navigate(1)).GetComponentInChildren<TextMeshProUGUI>().fontSize=64;
            var play=ArcadeStyle.Button("Play selected mode",showcase,"",new Vector2(0,-700),new Vector2(850,130),ArcadeStyle.Coral,()=>Launch(PlaySelected));playLabel=play.GetComponentInChildren<TextMeshProUGUI>();playLabel.fontSize=44;
            var back=ArcadeStyle.Button("Back home",showcase,"",new Vector2(0,-845),new Vector2(500,80),ArcadeStyle.Mist,()=>game.ReturnToMenu());
            back.GetComponentInChildren<TextMeshProUGUI>().fontSize=28;
        }
        private void BuildLevels()
        {
            levels=ArcadeStyle.Rect("Trial level selection",content,Vector2.zero,new Vector2(1080,1900));
            for(int i=0;i<4;i++)
            {
                int slot=i;levelButtons[i]=ArcadeStyle.Button("Trial level slot "+i,levels,"",new Vector2(0,460-i*255),new Vector2(900,205),ArcadeStyle.Cream,()=>Launch(()=>game.SelectMode(ArcadeMode.Trials,trialPage+slot)));
                levelButtons[i].GetComponentInChildren<TextMeshProUGUI>().fontSize=33;
            }
            ArcadeStyle.Button("More trial levels",levels,"",new Vector2(0,-650),new Vector2(850,115),ArcadeStyle.Gold,()=>{trialPage=(trialPage+4)%12;if(trialPage>=10)trialPage=0;ShowTrials();});
            ArcadeStyle.Button("Back to showcase",levels,"",new Vector2(0,-820),new Vector2(600,90),ArcadeStyle.Mist,Open);
        }
        private void BuildResults()
        {
            result=ArcadeStyle.Rect("Arcade result",content,Vector2.zero,new Vector2(1080,1900));
            resultTitle=ArcadeStyle.Text("Result headline",result,"",new Vector2(0,560),new Vector2(940,170),66);
            resultScore=ArcadeStyle.Text("Result score",result,"",new Vector2(0,300),new Vector2(900,200),126);
            for(int i=0;i<3;i++)resultStars[i]=Star(result,new Vector2((i-1)*150,60),112);
            resultReward=ArcadeStyle.Text("Result reward",result,"",new Vector2(0,-100),new Vector2(900,110),31,false);
            ArcadeStyle.Button("Retry arcade",result,"",new Vector2(0,-330),new Vector2(850,130),ArcadeStyle.Coral,()=>Launch(()=>game.SelectMode(game.Mode,game.Arcade.Trial)));
            next=ArcadeStyle.Button("Next trial",result,"",new Vector2(0,-510),new Vector2(850,120),ArcadeStyle.Gold,()=>Launch(()=>game.SelectMode(ArcadeMode.Trials,game.Arcade.Trial+1)));
            ArcadeStyle.Button("Return to modes",result,"",new Vector2(0,-720),new Vector2(800,110),ArcadeStyle.Cream,game.ArcadeHome);
        }
        private void SetButtonText(Transform parent,string name,string text)
        {parent.Find(name).GetComponentInChildren<TextMeshProUGUI>().text=text;}
        private void Show(RectTransform page,string headingText)
        {
            CancelTransition();backdrop.SetActive(true);modeHud.Hide();showcase.gameObject.SetActive(page==showcase);levels.gameObject.SetActive(page==levels);result.gameObject.SetActive(page==result);
            heading.text=headingText;Fit();group.alpha=1;group.blocksRaycasts=true;
        }
        public void Open()
        {
            Show(showcase,L("OYUN MODLARI","GAME MODES"));
            SetButtonText(showcase,"Back home",L("ANA MENÜ","HOME"));UpdateSelection();transition=StartCoroutine(SwitchMotion(1));
        }
        public void Navigate(int direction)=>SelectPreview((selected+direction+4)%4);
        public void SelectPreview(int index)
        {
            index=Mathf.Clamp(index,0,3);int direction=index>=selected?1:-1;CancelTransition();selected=index;UpdateSelection();
            transition=StartCoroutine(SwitchMotion(direction));
        }
        private void UpdateSelection()
        {
            var mode=SelectedMode;preview.SetMode(mode);victoryPlayed=false;
            title.text=Titles[selected];title.fontSize=mode==ArcadeMode.Classic?90:70;
            description.text=mode==ArcadeMode.Classic?L("Her dokunuşta yüksel. Kusursuz serini koru.","Rise with every tap. Keep your perfect streak."):
                mode==ArcadeMode.Rush?L("60 saniye. Üç hata hakkı. Sonuna kadar hız.","60 seconds. Three misses. Full speed to the finish."):
                mode==ArcadeMode.Balance?L("Ağırlığı hisset. Ters yönde yerleştir, dengeyi bul.","Feel the weight. Counterbalance to recover."):
                L("10 kısa sınav. Her kulede üç yıldız hedefle.","10 short trials. Reach for three stars in every tower.");
            int best=mode==ArcadeMode.Classic?saves.Data.bestScore:mode==ArcadeMode.Rush?saves.Data.rushBest:saves.Data.balanceBest;
            int cleared=0;foreach(int stars in saves.Data.trialStars)if(stars>0)cleared++;
            record.text=mode==ArcadeMode.Trials?cleared+L(" / 10 SEVİYE TAMAMLANDI"," / 10 LEVELS CLEARED"):L("KİŞİSEL REKOR  ","PERSONAL BEST  ")+best;
            eyebrow.text="0"+(selected+1)+" / 04   ·   "+(mode==ArcadeMode.Classic?L("SAKİNLİK","FLOW"):mode==ArcadeMode.Rush?L("HIZ","SPEED"):mode==ArcadeMode.Balance?L("HASSASİYET","PRECISION"):L("BAŞARI","ACHIEVEMENT"));
            playLabel.text=mode==ArcadeMode.Trials?L("SEVİYE SEÇ","CHOOSE LEVEL"):L("OYNA","PLAY");accentRule.color=ArcadeStyle.Accent(mode);
            for(int i=0;i<4;i++){dots[i].color=i==selected?ArcadeStyle.Accent(mode):Color.Lerp(ArcadeStyle.Mist,ArcadeStyle.Ink,.16f);dots[i].rectTransform.sizeDelta=new Vector2(i==selected?40:18,18);}
            foreach(var star in previewStars)star.gameObject.SetActive(mode==ArcadeMode.Trials);
            previewCaption.gameObject.SetActive(mode!=ArcadeMode.Trials);
            previewGauge.gameObject.SetActive(mode==ArcadeMode.Balance);
        }
        private IEnumerator SwitchMotion(int direction)
        {
            for(float t=0;t<.34f;t+=Time.unscaledDeltaTime)
            {
                float ease=ArcadeStyle.Ease(t/.34f);hero.anchoredPosition=new Vector2(direction*110*(1-ease),210);details.anchoredPosition=new Vector2(direction*45*(1-ease),0);heroGroup.alpha=ease;
                title.color=Color.Lerp(new Color(ArcadeStyle.Ink.r,ArcadeStyle.Ink.g,ArcadeStyle.Ink.b,0),ArcadeStyle.Ink,ArcadeStyle.Ease(t/.22f));yield return null;
            }
            ResetMotion();transition=null;
        }
        private void ResetMotion(){if(hero!=null){hero.anchoredPosition=new Vector2(0,210);details.anchoredPosition=Vector2.zero;heroGroup.alpha=1;title.color=ArcadeStyle.Ink;}}
        private void CancelTransition(){if(transition!=null)StopCoroutine(transition);transition=null;ResetMotion();if(group!=null){group.alpha=1;group.blocksRaycasts=true;}}
        private void Launch(System.Action action)
        {
            if(!group.blocksRaycasts)return;
            CancelTransition();transition=StartCoroutine(LaunchMotion(action));
        }
        private IEnumerator LaunchMotion(System.Action action)
        {
            group.blocksRaycasts=false;
            for(float t=0;t<.2f;t+=Time.unscaledDeltaTime){group.alpha=1-ArcadeStyle.Ease(t/.2f);yield return null;}
            transition=null;group.alpha=1;group.blocksRaycasts=true;action();
        }
        public void PlaySelected(){CancelTransition();if(SelectedMode==ArcadeMode.Trials)ShowTrials();else game.SelectMode(SelectedMode);}
        public void ShowTrials()
        {
            Show(levels,L("KULE SINAVLARI","TOWER TRIALS"));accentRule.color=ArcadeStyle.Gold;
            for(int i=0;i<4;i++)
            {
                int level=trialPage+i;var b=levelButtons[i];b.gameObject.SetActive(level<10);if(level>=10)continue;
                var d=TrialDefinition.All[level];bool unlocked=level==0||saves.Data.trialStars[level-1]>0;b.interactable=unlocked;
                b.GetComponentInChildren<TextMeshProUGUI>().text=L("SEVİYE ","LEVEL ")+(level+1)+"  ·  "+d.blocks+L(" BLOK"," BLOCKS")+"\n<size=25>"+d.chain+" PERFECT  ·  "+d.seconds+L(" sn  ·  MİN. "," s  ·  MIN. ")+d.width.ToString("0.0")+"</size>\n<size=27>"+(unlocked?saves.Data.trialStars[level]+L(" / 3 YILDIZ"," / 3 STARS"):L("KİLİTLİ","LOCKED"))+"</size>";
            }
            SetButtonText(levels,"More trial levels",L("DİĞER SEVİYELER","MORE LEVELS"));SetButtonText(levels,"Back to showcase",L("MODLARA DÖN","BACK TO MODES"));
        }
        public void Hide(){CancelTransition();if(backdrop!=null)backdrop.SetActive(false);modeHud?.Hide();}
        public void Refresh()=>modeHud?.Refresh();
        public void Results()
        {
            Show(result,L("TUR TAMAMLANDI","RUN COMPLETE"));accentRule.color=ArcadeStyle.Accent(game.Mode);resultTime=0;
            bool trial=game.Mode==ArcadeMode.Trials;bool won=trial&&game.Arcade.Won;
            resultTitle.text=won?L("ZİRVE SENİN!","SUMMIT REACHED!"):trial?L("BİR DAHA DENE","TRY AGAIN"):game.Mode==ArcadeMode.Rush?L("NE HIZ AMA!","WHAT A RUSH!"):L("DENGE YOLCULUĞU","A BALANCING ACT");
            resultScore.text=game.Score.ToString();resultReward.text=won?(game.TrialFirstClear?L("İLK ZAFER  ·  +5 ELMAS","FIRST CLEAR  ·  +5 GEMS"):L("Yıldızların kaydedildi. Yeni zirveye hazır mısın?","Stars saved. Ready for your next summit?")):L("Her tur yeni bir zirve.","Every run is a new summit.");
            for(int i=0;i<3;i++){resultStars[i].gameObject.SetActive(trial);resultStars[i].color=i<game.Arcade.Stars?ArcadeStyle.Gold:Color.Lerp(ArcadeStyle.Mist,ArcadeStyle.Ink,.18f);}
            next.gameObject.SetActive(won&&game.Arcade.Trial<9);
            SetButtonText(result,"Retry arcade",L("TEKRAR OYNA","PLAY AGAIN"));SetButtonText(result,"Next trial",L("SONRAKİ SEVİYE","NEXT LEVEL"));SetButtonText(result,"Return to modes",L("MODLARA DÖN","BACK TO MODES"));
        }
        private void Update()
        {
            if(showcase!=null&&showcase.gameObject.activeInHierarchy)
            {
                previewGauge.Value=preview.Stability;
                previewCaption.text=SelectedMode==ArcadeMode.Rush?preview.PreviewSeconds+L(" SANİYE · HIZLAN"," SECONDS · GO FAST"):
                    SelectedMode==ArcadeMode.Balance?(Mathf.Abs(preview.Stability)>.5f?L("AĞIRLIĞI DENGELE","COUNTER THE WEIGHT"):L("DENGEYE DÖNÜŞ","STABILITY RECOVERED")):L("BİR BLOK DAHA YÜKSEĞE","ONE BLOCK CLOSER TO THE SKY");
                for(int i=0;i<3;i++)previewStars[i].color=preview.Cycle>.63f+i*.08f?ArcadeStyle.Gold:Color.Lerp(ArcadeStyle.Mist,ArcadeStyle.Ink,.16f);
                if(preview.Victory&&!victoryPlayed){victoryPlayed=true;game.ArcadePreviewBeat();}if(!preview.Victory)victoryPlayed=false;
            }
            if(result!=null&&result.gameObject.activeInHierarchy)
            {
                resultTime+=Time.unscaledDeltaTime;
                for(int i=0;i<3;i++){float t=Mathf.Clamp01((resultTime-i*.12f)/.35f);float back=1+2.2f*Mathf.Pow(t-1,3)+1.2f*Mathf.Pow(t-1,2);resultStars[i].transform.localScale=Vector3.one*Mathf.Max(.01f,back);}
                resultReward.alpha=ArcadeStyle.Ease((resultTime-.3f)/.25f);
            }
        }
        public void OnBeginDrag(PointerEventData e){dragStart=e.position;}
        public void OnEndDrag(PointerEventData e)
        {
            if(!showcase.gameObject.activeInHierarchy)return;
            Vector2 delta=e.position-dragStart;if(Mathf.Abs(delta.x)>Screen.width*.08f&&Mathf.Abs(delta.x)>Mathf.Abs(delta.y))Navigate(delta.x<0?1:-1);
        }
    }
}
