using UnityEngine;
using UnityEngine.UI;
using Wobble.Themes;
namespace Wobble.UI
{
    // One cached 9-sliced shape; no per-frame texture work or imported UI framework.
    public static class CasualSurface
    {
        private static Sprite rounded;
        public static void Style(Image image,bool shadow=true)
        {
            if(rounded==null)
            {
                var texture=new Texture2D(64,64,TextureFormat.RGBA32,false){name="Shared rounded cream surface",filterMode=FilterMode.Bilinear};
                var pixels=new Color[4096];
                for(var y=0;y<64;y++)for(var x=0;x<64;x++)
                {
                    var q=new Vector2(Mathf.Max(Mathf.Abs(x-31.5f)-12,0),Mathf.Max(Mathf.Abs(y-31.5f)-12,0));
                    pixels[y*64+x]=new Color(Mathf.Lerp(.87f,1,y/63f),Mathf.Lerp(.87f,1,y/63f),Mathf.Lerp(.87f,1,y/63f),Mathf.Clamp01(20-q.magnitude));
                }
                texture.SetPixels(pixels);texture.Apply(false,true);
                rounded=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(25,25,25,25));
            }
            image.sprite=rounded;image.type=Image.Type.Sliced;
            if(shadow&&image.GetComponent<Outline>()==null){var rim=image.gameObject.AddComponent<Outline>();rim.effectColor=WobblePalette.Alpha(WobblePalette.Surface,.65f);rim.effectDistance=new Vector2(2,-2);}
            if(shadow&&image.GetComponent<Shadow>()==null)
            {var s=image.gameObject.AddComponent<Shadow>();s.effectColor=WobblePalette.Alpha(WobblePalette.TextPrimary,.23f);s.effectDistance=new Vector2(0,-7);}
        }
    }
}
