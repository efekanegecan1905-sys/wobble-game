using UnityEngine;
using UnityEngine.UI;
namespace Wobble.UI
{
    // A tiny code-native isometric material study; locked themes remain previews only.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StonePreview : MaskableGraphic
    {
        public StonePreview(){useLegacyMeshGeneration=false;}
        public Color stone=Color.white;
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
            for(var i=0;i<3;i++)
            {
                var y=-.22f+i*.20f;
                Face(P(-.46f,y+.10f),P(0,y-.08f),P(.46f,y+.10f),P(0,y+.29f),stone);
                Face(P(-.46f,y+.10f),P(-.46f,y-.04f),P(0,y-.22f),P(0,y-.08f),stone*.58f);
                Face(P(0,y-.08f),P(0,y-.22f),P(.46f,y-.04f),P(.46f,y+.10f),stone*.78f);
            }
        }
    }
}
