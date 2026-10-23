using UnityEngine;
using UnityEngine.Rendering;

namespace Wobble.Themes
{
    // Presentation only: no colliders, scoring, input, movement or scene loads.
    public sealed class CarraraWorld : MonoBehaviour
    {
        private sealed class Monument
        {
            public Transform transform;
            public Renderer renderer;
            public Vector3 origin;
            public int layer;
            public float visibility;
            public MaterialPropertyBlock properties=new();
        }
        private readonly Monument[] monuments=new Monument[24];
        private readonly Material[] materials=new Material[2];
        private readonly Mesh[] meshes=new Mesh[3];
        private Camera gameplayCamera;
        private Light key;
        private ParticleSystem dust;
        private Material dustMaterial;
        private Transform content;
        private float height,progress,targetProgress;
        private bool playing;
        private ThemeManager themes;
        public void SetThemes(ThemeManager value) => themes = value;
        public int Stage => Mathf.Clamp(Mathf.FloorToInt(targetProgress)+1,1,4);
        public int GeometryCount => monuments.Length;
        public int AmbientParticleLimit => dust==null?0:dust.main.maxParticles;

        public void Build(Camera gameplayCamera,Light keyLight)
        {
            this.gameplayCamera=gameplayCamera;key=keyLight;
            content=new GameObject("Carrara architecture — presentation only").transform;content.SetParent(transform,false);
            for(var i=0;i<2;i++)materials[i]=new Material(Resources.Load<Shader>("Shaders/ArchitecturalStone")){name=i==0?"Weathered midground stone":"Fog-bound distant stone",enableInstancing=true};
            materials[0].color=WobblePalette.StoneLight;
            materials[1].color=WobblePalette.StoneMid;
            foreach(var material in materials)material.mainTexture=Resources.Load<Texture2D>("Rendering/CarraraNatural");
            meshes[0]=Resources.Load<Mesh>("Presentation/QuarryGarden");meshes[1]=Resources.Load<Mesh>("Presentation/ColumnGarden");meshes[2]=Resources.Load<Mesh>("Presentation/ArchGarden");
            for(var i=0;i<monuments.Length;i++)
            {
                var distant=i>=16;var a=i*Mathf.PI*2/(distant?8:16)+(distant?.3f:0);
                var radius=distant?66:31+(i%3)*5;
                var origin=new Vector3(Mathf.Cos(a)*radius,-14-(i%3)*2,Mathf.Sin(a)*radius);
                var go=new GameObject(distant?"Distant quarry ridge":i%3==1?"Broken monumental column":i%3==2?"Abstract stone portal":"Quarry terrace");
                go.transform.SetParent(content,false);go.transform.localPosition=origin;
                go.transform.localRotation=Quaternion.Euler(0,i*37,0);
                go.transform.localScale=distant?new Vector3(9,14+i%4*3,7):new Vector3(3+i%3,10+i%4*3,3+i%2);
                go.AddComponent<MeshFilter>().sharedMesh=meshes[distant?0:i%3];
                var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=materials[distant?1:0];
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                monuments[i]=new Monument{transform=go.transform,renderer=renderer,origin=origin,layer=distant?1:0};
            }
            BuildDust();Show(false);
        }
        private void BuildDust()
        {
            var go=new GameObject("Suspended marble motes");go.transform.SetParent(content,false);
            dust=go.AddComponent<ParticleSystem>();dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=dust.main;main.playOnAwake=false;main.startLifetime=9;main.startSpeed=.025f;
            main.startSize=new ParticleSystem.MinMaxCurve(.008f,.019f);main.startColor=new Color(.68f,.66f,.6f,.1f);
            main.maxParticles=QualitySettings.GetQualityLevel()==0?12:36;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var shape=dust.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(18,12,18);
            var emission=dust.emission;emission.rateOverTime=1;
            dustMaterial=new Material(Resources.Load<Shader>("Shaders/StoneDust"));
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=dustMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
        public void Show(bool show)
        {
            playing=show;content.gameObject.SetActive(show);
            if(show){height=0;progress=targetProgress=0;dust.Play();}
            else {dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);key.intensity=1.15f;key.transform.rotation=Quaternion.Euler(46,-32,0);}
        }
        public void Follow(float topHeight)=>height=topHeight;
        public void SetLevel(int level)=>targetProgress=Mathf.Clamp(level-1,0,3);
        private void LateUpdate()
        {
            if(!playing)return;
            if (themes != null)
            {
                var palette = themes.Environment;
                materials[0].color = Color.Lerp(palette.distantStructureTint, palette.fogNear, .25f);
                materials[1].color = palette.distantStructureTint;
                var main = dust.main;
                main.startColor = WobblePalette.Alpha(palette.optionalParticleTint, .13f);
            }
            var sky=RenderSettings.skybox;
            if(sky!=null)
                foreach(var material in materials)
                {
                    material.SetColor("_SkyTop",sky.GetColor("_Top"));
                    material.SetColor("_SkyHorizon",sky.GetColor("_Horizon"));
                    material.SetColor("_SkyBottom",sky.GetColor("_Bottom"));
                    material.SetColor("_Accent",sky.GetColor("_Accent"));
                    material.SetFloat("_AccentStrength",sky.GetFloat("_AccentStrength"));
                    material.SetFloat("_FogNear",RenderSettings.fogStartDistance);
                    material.SetFloat("_FogFar",RenderSettings.fogEndDistance);
                }
            progress=Mathf.MoveTowards(progress,targetProgress,Time.deltaTime/2.4f);
            var stage=progress/3;
            // World-anchored layers lag far behind tower growth instead of riding with it.
            for(var i=0;i<monuments.Length;i++)
            {
                var m=monuments[i];
                var target=m.origin+Vector3.up*(height*(m.layer==0?.16f:.055f)-stage*5);
                m.transform.localPosition=Vector3.Lerp(m.transform.localPosition,target,1-Mathf.Exp(-Time.deltaTime*.7f));
                // Keep every visible object behind the active tower plane at every orbit angle.
                var forward=gameplayCamera.transform.forward;
                var bounds=m.renderer.bounds;
                var radius=Vector3.Dot(bounds.extents,new Vector3(Mathf.Abs(forward.x),Mathf.Abs(forward.y),Mathf.Abs(forward.z)));
                var behind=Vector3.Dot(bounds.center-new Vector3(0,height,0),forward)-radius;
                var visibility=Mathf.SmoothStep(0,1,(behind-9)/14);
                var character=m.layer==1?Mathf.Lerp(.65f,1,stage):i%3==0?Mathf.Lerp(1,.35f,stage)
                    :Mathf.Lerp(.7f,1,Mathf.Min(1,progress))*Mathf.Lerp(1,.4f,Mathf.Clamp01((progress-1)/2));
                visibility*=character;
                m.visibility=visibility;
                m.properties.SetFloat("_Visibility",m.visibility);
                m.renderer.SetPropertyBlock(m.properties);m.renderer.enabled=m.visibility>.005f;
            }
            dust.transform.position=new Vector3(0,height+1,0);
            var emission=dust.emission;emission.rateOverTime=QualitySettings.GetQualityLevel()==0?.5f:Mathf.Lerp(.7f,2.4f,stage);
            key.intensity=Mathf.Lerp(1.15f,1.22f,stage);
            key.transform.rotation=Quaternion.Euler(Mathf.Lerp(46,51,stage),Mathf.Lerp(-32,-22,stage),0);
        }
        private void OnDestroy()
        {foreach(var material in materials)Destroy(material);Destroy(dustMaterial);}
    }
}
