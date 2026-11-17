using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Wobble.Themes;
namespace Wobble.UI
{
    public sealed class ArcadePreview:MonoBehaviour
    {
        private GameObject stage;private RenderTexture target;private Material material;private Transform[] blocks;
        private int mode;
        public void Build(int index,Color color)
        {
            mode=index;stage=new GameObject("Mode preview stage");stage.transform.position=new Vector3(10000+index*30,0,0);
            target=new RenderTexture(256,256,24);target.Create();
            var image=gameObject.AddComponent<RawImage>();image.texture=target;image.raycastTarget=false;
            var cameraObject=new GameObject("Preview camera",typeof(Camera));cameraObject.transform.SetParent(stage.transform,false);
            cameraObject.transform.localPosition=new Vector3(5,5,6);cameraObject.transform.LookAt(stage.transform.position+Vector3.up);
            var camera=cameraObject.GetComponent<Camera>();camera.targetTexture=target;camera.orthographic=true;camera.orthographicSize=2.7f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.08f,.13f,1);camera.cullingMask=1<<30;
            // Small menu previews do not need world shadows, HDR or extra URP copies.
            camera.nearClipPlane=.1f;camera.farClipPlane=20;camera.allowHDR=false;camera.allowMSAA=false;
            camera.backgroundColor=new Color(.035f,.08f,.13f,0);
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;data.renderShadows=false;
            data.requiresColorTexture=false;data.requiresDepthTexture=false;
            material=new Material(Resources.Load<Shader>("Shaders/Carrara"));material.color=color;
            blocks=new Transform[5];
            for(int i=0;i<5;i++)
            {
                var block=new GameObject("Preview stone",typeof(MeshFilter),typeof(MeshRenderer));block.layer=30;block.transform.SetParent(stage.transform,false);
                block.GetComponent<MeshFilter>().sharedMesh=ThemeWorldKit.BoxMesh;block.GetComponent<MeshRenderer>().sharedMaterial=material;
                block.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                block.GetComponent<MeshRenderer>().receiveShadows=false;
                block.transform.localScale=new Vector3(2,.35f,2);block.transform.localPosition=Vector3.up*i*.4f;blocks[i]=block.transform;
            }
        }
        private void Update()
        {
            if(blocks==null)return;float t=Time.unscaledTime;
            blocks[4].localPosition=new Vector3(Mathf.Sin(t*(mode==1?5:1.5f)),1.6f,0);
            for(int i=0;i<4;i++)blocks[i].localRotation=Quaternion.Euler(0,mode==3?Mathf.Sin(t+i)*8:0,mode==2?Mathf.Sin(t*2)*i*2:0);
        }
        private void OnDestroy(){if(target!=null){target.Release();Destroy(target);}Destroy(material);Destroy(stage);}
    }
}
