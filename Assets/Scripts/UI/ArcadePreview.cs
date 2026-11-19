using UnityEngine;
using UnityEngine.Rendering.Universal;
using Wobble.Themes;
using Wobble.Gameplay;

namespace Wobble.UI
{
    // A single reusable miniature stage; switching mode allocates no cameras, meshes or textures.
    public sealed class ArcadePreview:MonoBehaviour
    {
        private GameObject stage;
        private RenderTexture target;
        private Material stone,accent,baseMaterial;
        private Transform tower,traveller,finish;
        private Transform[] slabs;
        private Camera previewCamera;
        private ArcadeMode mode;
        private float elapsed;
        public float Cycle=>Mathf.Repeat(elapsed,5f)/5f;
        public float Stability=>Mathf.Sin(Cycle*Mathf.PI*2)*.75f;
        public int PreviewSeconds=>60-Mathf.FloorToInt(Cycle*30);
        public bool Victory=>mode==ArcadeMode.Trials&&Cycle>.72f;
        public void Build(int index,Color color)
        {
            stage=new GameObject("Arcade showcase stage");stage.transform.position=new Vector3(10000,0,0);
            target=new RenderTexture(768,768,24){name="Single arcade showcase"};target.Create();
            var image=gameObject.AddComponent<UnityEngine.UI.RawImage>();image.texture=target;image.raycastTarget=true;
            var cameraObject=new GameObject("Showcase camera",typeof(Camera));cameraObject.transform.SetParent(stage.transform,false);
            previewCamera=cameraObject.GetComponent<Camera>();previewCamera.targetTexture=target;previewCamera.orthographic=true;previewCamera.orthographicSize=4.4f;
            previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=new Color(0,0,0,0);previewCamera.cullingMask=1<<29;
            previewCamera.nearClipPlane=.1f;previewCamera.farClipPlane=30;previewCamera.allowHDR=false;previewCamera.allowMSAA=false;
            cameraObject.transform.localPosition=new Vector3(7,6.5f,9);cameraObject.transform.LookAt(stage.transform.position+Vector3.up*1.4f);
            var data=previewCamera.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;data.renderShadows=false;data.requiresColorTexture=false;data.requiresDepthTexture=false;
            stone=new Material(Resources.Load<Shader>("Shaders/Carrara")){color=ArcadeStyle.Cream};
            var texture=Resources.Load<Material>("Menu/CarraraPreview");if(texture!=null)stone.CopyPropertiesFromMaterial(texture);
            accent=new Material(stone);baseMaterial=new Material(stone){color=Color.Lerp(ArcadeStyle.Mist,ArcadeStyle.Aqua,.15f)};
            Shape("Foundation",ThemeWorldKit.CylinderMesh,new Vector3(0,-.5f,0),new Vector3(5.6f,.28f,5.6f),baseMaterial,stage.transform);
            Shape("Raised podium",ThemeWorldKit.CylinderMesh,new Vector3(0,-.26f,0),new Vector3(4.8f,.22f,4.8f),stone,stage.transform);
            Shape("Mode ring",ThemeWorldKit.TrimMesh,new Vector3(0,-.08f,0),Vector3.one*2.45f,accent,stage.transform);
            tower=new GameObject("Balanced visual tower").transform;tower.SetParent(stage.transform,false);
            slabs=new Transform[7];
            for(int i=0;i<slabs.Length;i++)slabs[i]=Shape("Marble course "+i,ThemeWorldKit.BoxMesh,new Vector3(0,i*.38f,0),new Vector3(2.8f,.34f,2.8f),i==6?accent:stone,tower);
            traveller=slabs[6];
            finish=new GameObject("Trial finish gate").transform;finish.SetParent(stage.transform,false);
            Shape("Finish post",ThemeWorldKit.BoxMesh,new Vector3(-2.1f,1.7f,-1.5f),new Vector3(.09f,3.8f,.09f),accent,finish);
            Shape("Finish banner",ThemeWorldKit.BoxMesh,new Vector3(-1.45f,3.35f,-1.5f),new Vector3(1.2f,.45f,.1f),accent,finish);
            SetMode((ArcadeMode)index);
        }
        private Transform Shape(string name,Mesh mesh,Vector3 position,Vector3 scale,Material material,Transform parent)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.layer=29;go.transform.SetParent(parent,false);
            go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return go.transform;
        }
        public void SetMode(ArcadeMode selected)
        {
            mode=selected;elapsed=0;if(stage==null)return;
            accent.color=ArcadeStyle.Accent(mode);finish.gameObject.SetActive(mode==ArcadeMode.Trials);
            tower.localRotation=Quaternion.identity;tower.localPosition=Vector3.zero;
            foreach(var slab in slabs){slab.gameObject.SetActive(true);slab.localRotation=Quaternion.identity;}
            Animate();
        }
        private void Update(){elapsed+=Time.unscaledDeltaTime;Animate();}
        private void Animate()
        {
            if(stage==null)return;float phase=Cycle;
            for(int i=0;i<slabs.Length;i++)
            {
                slabs[i].localPosition=new Vector3(0,i*.38f,0);
                if(mode==ArcadeMode.Classic||mode==ArcadeMode.Trials)
                {
                    float reveal=i<3?1:Mathf.Clamp01(phase*9-(i-3)*1.3f);
                    slabs[i].gameObject.SetActive(reveal>.02f);
                    slabs[i].localScale=new Vector3(2.8f,.34f*Mathf.Max(.01f,reveal),2.8f);
                    slabs[i].localPosition+=Vector3.up*(1-reveal)*.6f;
                }
                else slabs[i].localScale=new Vector3(2.8f,.34f,2.8f);
            }
            if(mode==ArcadeMode.Rush)traveller.localPosition+=Vector3.right*Mathf.Sin(elapsed*5.5f)*1.7f;
            if(mode==ArcadeMode.Balance)
            {
                tower.localRotation=Quaternion.Euler(Stability*4,0,-Stability*12);
                traveller.localPosition+=Vector3.right*Mathf.Sin(elapsed*1.2566f+1)*.6f;
            }
            if(mode==ArcadeMode.Trials&&Victory)tower.localPosition=Vector3.up*Mathf.Sin((phase-.72f)/.28f*Mathf.PI)*.12f;
        }
        private void OnEnable(){if(stage!=null)stage.SetActive(true);}
        private void OnDisable(){if(stage!=null)stage.SetActive(false);}
        private void OnDestroy(){if(target!=null){target.Release();Destroy(target);}Destroy(stone);Destroy(accent);Destroy(baseMaterial);Destroy(stage);}
    }
}
