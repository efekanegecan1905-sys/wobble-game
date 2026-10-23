using UnityEngine;
using Wobble.Gameplay;
using Wobble.Themes;
using Wobble.CameraSystem;
namespace Wobble.UI
{
    // One lightweight stage shared by Home, identity reveal and every menu.
    public sealed class HomePresentation : MonoBehaviour
    {
        private Transform sculpture,logo;
        private readonly Transform[] letters=new Transform[6];
        private readonly Transform[] introFragments=new Transform[8];
        private readonly Vector3[] monumentScales=new Vector3[7];
        private readonly GameObject[] monuments=new GameObject[7];
        private ParticleSystem signatureDust;
        private Material dustMaterial;
        private MaterialPropertyBlock highlight;
        private readonly Renderer[] letterSurfaces=new Renderer[6];
        private Camera homeCamera,gameplayCamera;
        private GameObject stage;
        private Material backdrop,foliage,flowers;
        private float section,sectionTarget,entry,clock;
        public Camera Camera=>homeCamera;
        public float IntroProgress {get;private set;}=1;
        public void Build(ThemeManager themes,Camera gameplayCamera)
        {
            highlight=new MaterialPropertyBlock();
            this.gameplayCamera=gameplayCamera;
            stage=new GameObject("Carrara gallery");stage.transform.SetParent(transform,false);stage.transform.position=new Vector3(1000,0,1000);
            sculpture=new GameObject("Cantilever Carrara sculpture").transform;sculpture.SetParent(stage.transform,false);
            for(var i=0;i<8;i++)
            {
                var slab=new GameObject("Gallery slab "+i);slab.transform.SetParent(sculpture,false);
                slab.transform.localPosition=new Vector3(Mathf.Sin(i*.9f)*.36f,i*.50f-1.25f,0);
                slab.transform.localRotation=Quaternion.Euler(0,(i-3)*4,0);
                slab.AddComponent<BlockView>().Initialize(new Vector3(i%3==1?1.65f:2.5f,.48f,i%3==1?1.65f:2.5f),themes.BlockMaterial,themes.TopMaterial);
                slab.GetComponent<Collider>().enabled=false;
            }
            backdrop=new Material(Resources.Load<Shader>("Shaders/ArchitecturalStone")){color=WobblePalette.StoneLight};
            backdrop.mainTexture=Resources.Load<Texture2D>("Rendering/CarraraNatural");
            backdrop.SetFloat("_Visibility",1);backdrop.SetFloat("_FogNear",20);backdrop.SetFloat("_FogFar",65);
            for(var i=0;i<7;i++)
            {
                var go=new GameObject("Gallery monolith "+i);monuments[i]=go;go.transform.SetParent(stage.transform,false);
                go.transform.localPosition=new Vector3((i-3)*5,-3,-14-i%2*6);
                go.transform.localScale=new Vector3(2+i%2,4+i%3*3,2);
                monumentScales[i]=go.transform.localScale;
                go.transform.localRotation=Quaternion.Euler(0,i*19,0);
                go.AddComponent<MeshFilter>().sharedMesh=Resources.Load<Mesh>("Presentation/Monolith");
                var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=backdrop;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            logo=new GameObject("WOBBLE stone identity").transform;logo.SetParent(stage.transform,false);
            for(var i=0;i<6;i++)
            {
                var go=new GameObject("WOBBLE letter "+i);go.transform.SetParent(logo,false);letters[i]=go.transform;
                go.AddComponent<MeshFilter>().sharedMesh=Resources.Load<Mesh>("Presentation/Letter"+i);
                letterSurfaces[i]=go.AddComponent<MeshRenderer>();letterSurfaces[i].sharedMaterial=themes.TopMaterial;
            }
            for(var i=0;i<introFragments.Length;i++)
            {
                var go=new GameObject("Intro marble fragment "+i);go.transform.SetParent(logo,false);introFragments[i]=go.transform;
                go.AddComponent<MeshFilter>().sharedMesh=Resources.Load<Mesh>("Presentation/Monolith");
                var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=themes.TopMaterial;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var dustObject=new GameObject("Identity stone dust");dustObject.transform.SetParent(stage.transform,false);
            signatureDust=dustObject.AddComponent<ParticleSystem>();signatureDust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=signatureDust.main;main.playOnAwake=false;main.loop=false;main.startLifetime=1.1f;main.startSpeed=.12f;main.startSize=.018f;main.startColor=new Color(.94f,.86f,.72f,.20f);main.maxParticles=32;
            main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=signatureDust.emission;emission.enabled=false;
            var shape=signatureDust.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(4,.3f,.2f);
            dustMaterial=new Material(Resources.Load<Shader>("Shaders/StoneDust"));dustObject.GetComponent<ParticleSystemRenderer>().sharedMaterial=dustMaterial;
            var cameraObject=new GameObject("Home Camera");cameraObject.transform.SetParent(stage.transform,false);
            homeCamera=cameraObject.AddComponent<Camera>();homeCamera.cullingMask=(1<<30)|(1<<5)|1;homeCamera.clearFlags=CameraClearFlags.Skybox;
            homeCamera.fieldOfView=40;homeCamera.nearClipPlane=.1f;homeCamera.farClipPlane=100;
            cameraObject.AddComponent<FidelityController>();
            foreach(var child in stage.GetComponentsInChildren<Transform>())child.gameObject.layer=30;
            BuildGarden(themes);
            foreach(var child in stage.GetComponentsInChildren<Transform>())child.gameObject.layer=30;
            SetIntro(1);
        }
        public void SetSection(MenuScreen screen)=>sectionTarget=screen==MenuScreen.Shop?1:screen==MenuScreen.Themes?-1:screen==MenuScreen.Daily?.5f:screen==MenuScreen.Settings?.2f:0;
        public void SetEntry(float progress)=>entry=progress;
        public void SignatureDust(){signatureDust.transform.position=logo.position;signatureDust.Emit(12);}
        public void AssemblyDust(int index){signatureDust.transform.position=index<0?logo.position:letters[index].position;signatureDust.Emit(index<0?6:2);}
        public void SetIntro(float progress)
        {
            IntroProgress=Mathf.Clamp01(progress);logo.gameObject.SetActive(progress<1);
            var seconds=IntroProgress*5.8f;
            var worldReveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,4,seconds));
            for(var i=0;i<monuments.Length;i++){monuments[i].SetActive(worldReveal>0);monuments[i].transform.localScale=monumentScales[i]*worldReveal;}
            sculpture.localScale=Vector3.one*(.94f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.65f,5.3f,seconds)));
            if(progress>=1)signatureDust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public void Show(bool show){stage.SetActive(show);gameplayCamera.enabled=!show;if(show)entry=0;}
        private void LateUpdate()
        {
            if(stage==null||!stage.activeSelf)return;
            clock+=Time.unscaledDeltaTime;
            var sky=RenderSettings.skybox;if(sky!=null){backdrop.SetColor("_SkyTop",sky.GetColor("_Top"));backdrop.SetColor("_SkyHorizon",sky.GetColor("_Horizon"));backdrop.SetColor("_SkyBottom",sky.GetColor("_Bottom"));}
            section=Mathf.Lerp(section,sectionTarget,1-Mathf.Exp(-Time.unscaledDeltaTime*5));
            sculpture.localRotation=Quaternion.Euler(0,Mathf.Sin(clock*.08f)*7+section*12,0);
            var rotation=Quaternion.Euler(24+Mathf.Sin(clock*.12f)*.6f,-132+section*4,0);
            var introSeconds=IntroProgress*5.8f;
            var hero=Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,4,introSeconds));
            var exit=Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.8f,5.8f,introSeconds));
            if(IntroProgress<1)rotation*=Quaternion.Euler(-2*(1-exit),Mathf.Lerp(-5,3,hero)*(1-exit),0);
            var distance=Mathf.Max(12.6f,8.4f/Mathf.Max(.4f,homeCamera.aspect));
            var focus=stage.transform.position+new Vector3(section*.5f,.05f,0);
            homeCamera.transform.SetPositionAndRotation(focus-rotation*Vector3.forward*(distance-entry*2),rotation);
            if(IntroProgress<1)
            {
                logo.position=focus;logo.rotation=rotation;
                logo.position+=rotation*new Vector3(0,exit*7,-exit*3);
                logo.localScale=Vector3.one*Mathf.Lerp(.74f,.84f,hero);
                for(var i=0;i<6;i++)
                {
                    var gather=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.6f,1.8f,introSeconds));
                    var assemble=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.8f+i*.15f,2.15f+i*.15f,introSeconds));
                    var spread=(1-assemble)*Mathf.Lerp(5,1.5f,gather);
                    letters[i].localPosition=new Vector3((i-2.5f)*1.02f,(i%2==0?1:-1)*spread,-(1-assemble)*Mathf.Lerp(7,1.8f+i*.2f,gather));
                    letters[i].localRotation=Quaternion.Euler((1-assemble)*35,0,(1-assemble)*(i%2==0?14:-14));
                    var sweep=Mathf.Exp(-Mathf.Pow((introSeconds-3.12f)*8-i*1.05f,2));
                    var warmth=Color.Lerp(new Color(.70f,.74f,.77f),new Color(.88f,.83f,.74f),hero);
                    highlight.SetColor("_Color",Color.Lerp(warmth,new Color(1,.95f,.83f),sweep));letterSurfaces[i].SetPropertyBlock(highlight);
                }
                var fragmentsIn=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.6f,1.8f,introSeconds));
                var fragmentsOut=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.4f,3.1f,introSeconds));
                for(var i=0;i<introFragments.Length;i++)
                {
                    var angle=i*Mathf.PI*.25f;var radius=Mathf.Lerp(8,3.5f,fragmentsIn);
                    introFragments[i].localPosition=new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius*.65f,Mathf.Lerp(-8-i*.3f,1+i%3,fragmentsIn));
                    introFragments[i].localRotation=Quaternion.Euler(i*23,introSeconds*12+i*31,i*17);
                    introFragments[i].localScale=new Vector3(.22f,.12f,.18f)*fragmentsIn*fragmentsOut;
                }
            }
        }
        private void BuildGarden(ThemeManager themes)
        {
            // Shared marble meshes, no colliders or extra lights; terrace and arch frame the tower.
            GameObject Stone(string name,Vector3 position,Vector3 scale,PrimitiveType shape=PrimitiveType.Cube)
            {
                var go=GameObject.CreatePrimitive(shape);go.name=name;go.transform.SetParent(stage.transform,false);
                go.transform.localPosition=position;go.transform.localScale=scale;Destroy(go.GetComponent<Collider>());
                var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=themes.BlockMaterial;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go;
            }
            Stone("Circular Carrara plinth",new Vector3(0,-1.8f,0),new Vector3(4.6f,.35f,4.6f),PrimitiveType.Cylinder);
            Stone("Lower terrace",new Vector3(0,-2.45f,0),new Vector3(6,.3f,6),PrimitiveType.Cylinder);
            for(var i=0;i<18;i++)
            {
                var side=i%2==0?-1:1;var rank=i/2;
                Stone("Quarry terrace",new Vector3(side*(4.4f+rank%3*2),-3.7f+rank%3*.6f,-4-rank*1.5f),
                    new Vector3(2.8f,2+rank%3,2.6f));
            }
            foliage=new Material(themes.BlockMaterial){color=new Color(.28f,.48f,.28f)};
            flowers=new Material(themes.BlockMaterial){color=WobblePalette.Primary};
            for(var i=0;i<14;i++)
            {
                var side=i%2==0?-1:1;var rank=i/2;var tall=i%3==0;var h=tall?2.7f:.35f;
                var terraceTop=-3.7f+rank%3*.6f+(2+rank%3)*.5f;
                var plant=Stone("Cypress and coral garden",new Vector3(side*(4.4f+rank%3*2),terraceTop+h*.5f,-4-rank*1.5f),new Vector3(tall?.45f:.8f,h,tall?.45f:.8f),PrimitiveType.Sphere);
                var renderer=plant.GetComponent<Renderer>();renderer.sharedMaterial=i%3==0?foliage:flowers;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var arch=new GameObject("Warm marble arch").transform;arch.SetParent(stage.transform,false);
            arch.localPosition=new Vector3(0,-2,-8);arch.localRotation=Quaternion.Euler(0,38,0);arch.localScale=Vector3.one*.8f;
            for(var i=0;i<15;i++)
            {
                var angle=i/14f*Mathf.PI;
                var block=Stone("Arch voussoir",Vector3.zero,new Vector3(.82f,.9f,1.1f));
                block.transform.SetParent(arch,false);block.transform.localPosition=new Vector3(Mathf.Cos(angle)*3,3+Mathf.Sin(angle)*3,0);
                block.transform.localRotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg-90);
            }
            for(var i=0;i<4;i++)
            {
                var column=Stone("Arch support",Vector3.zero,new Vector3(.95f,1.5f,1.1f));
                column.transform.SetParent(arch,false);column.transform.localPosition=new Vector3(i%2==0?-3:3,i/2*1.5f,0);
            }
        }
        private void OnDestroy(){if(gameplayCamera!=null)gameplayCamera.enabled=true;Destroy(backdrop);Destroy(dustMaterial);Destroy(foliage);Destroy(flowers);}
    }
}
