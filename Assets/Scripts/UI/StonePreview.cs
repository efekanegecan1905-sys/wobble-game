using UnityEngine;
using UnityEngine.UI;
namespace Wobble.UI
{
    // A tiny code-native isometric material study; locked themes remain previews only.
    // Four tiers, a soft accent glow and a contact shadow read as a real tower at card size.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StonePreview : MaskableGraphic
    {
        public StonePreview(){useLegacyMeshGeneration=false;}
        public Color stone=Color.white;
        public Color accent=new(1f,.78f,.45f,1f);
        private Texture texture;
        public override Texture mainTexture=>texture!=null?texture:texture=Resources.Load<Texture2D>("Rendering/CarraraNatural");
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;
            Vector2 P(float x,float y)=>rect.center+new Vector2(x*rect.width,y*rect.height);
            void Face(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint)
            {
                tint.a=1;
                var n=vh.currentVertCount;vh.AddVert(a,tint,new Vector2(0,0));vh.AddVert(b,tint,new Vector2(0,1));vh.AddVert(c,tint,new Vector2(1,1));vh.AddVert(d,tint,new Vector2(1,0));vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
            void Disc(Vector2 centre,float radiusX,float radiusY,Color tint,int segments=22)
            {
                if(tint.a<=.001f)return;
                var n=vh.currentVertCount;vh.AddVert(centre,tint,new Vector2(.5f,.5f));
                for(var s=0;s<=segments;s++)
                {
                    var angle=s/(float)segments*Mathf.PI*2f;
                    vh.AddVert(centre+new Vector2(Mathf.Cos(angle)*radiusX,Mathf.Sin(angle)*radiusY),tint,new Vector2(.5f,.5f));
                }
                for(var s=0;s<segments;s++)vh.AddTriangle(n,n+1+s,n+2+s);
            }
            // Halo and contact shadow sit behind the stone and give the card depth.
            Disc(P(0,-.02f),rect.width*.40f,rect.width*.28f,new Color(accent.r,accent.g,accent.b,.13f));
            Disc(P(0,-.365f),rect.width*.29f,rect.width*.075f,new Color(.05f,.06f,.09f,.18f));
            var topColor=Color.Lerp(stone,accent,.46f);
            for(var i=0;i<4;i++)
            {
                var y=-.30f+i*.145f;
                var w=.47f-i*.022f;
                Face(P(-w,y+.10f),P(0,y-.08f),P(w,y+.10f),P(0,y+.29f),i==3?topColor:stone);
                Face(P(-w,y+.10f),P(-w,y-.04f),P(0,y-.22f),P(0,y-.08f),stone*.55f);
                Face(P(0,y-.08f),P(0,y-.22f),P(w,y-.04f),P(w,y+.10f),stone*.79f);
            }
        }
    }
}
