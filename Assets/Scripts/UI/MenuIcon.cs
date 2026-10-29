using UnityEngine;
using UnityEngine.UI;

namespace Wobble.UI
{
    // Resolution-independent bold icons and faceted rewards; no emoji/font dependency.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuIcon : MaskableGraphic
    {
        public MenuIcon() { useLegacyMeshGeneration = false; }
        public enum Shape { Gem, Heart, Layers, Shop, Calendar, Settings, Play, Trophy }
        public Shape shape;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            Vector2 P(float x, float y) => rect.center + new Vector2(x * rect.width * .42f, y * rect.height * .42f);
            void Line(Vector2 a, Vector2 b)
            {
                var n = new Vector2(-(b-a).y, (b-a).x).normalized * Mathf.Max(2,rect.width*.045f);
                var index = vh.currentVertCount;
                vh.AddVert(a-n,color,Vector2.zero); vh.AddVert(a+n,color,Vector2.zero);
                vh.AddVert(b+n,color,Vector2.zero); vh.AddVert(b-n,color,Vector2.zero);
                vh.AddTriangle(index,index+1,index+2); vh.AddTriangle(index,index+2,index+3);
            }
            void Path(params Vector2[] points) { for(var i=1;i<points.Length;i++) Line(points[i-1],points[i]); }
            void Facet(Vector2 a,Vector2 b,Vector2 c,float light)
            {
                var tint=Color.Lerp(color,Color.white,Mathf.Max(0,light));if(light<0)tint=color*(1+light);tint.a=color.a;
                var k=vh.currentVertCount;vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddTriangle(k,k+1,k+2);
            }
            if(shape==Shape.Gem)
            {
                Facet(P(-1,.35f),P(-.55f,.9f),P(0,.35f),.30f);Facet(P(-.55f,.9f),P(.55f,.9f),P(0,.35f),.55f);
                Facet(P(.55f,.9f),P(1,.35f),P(0,.35f),.05f);Facet(P(-1,.35f),P(0,.35f),P(0,-1),.12f);
                Facet(P(0,.35f),P(1,.35f),P(0,-1),-.2f);return;
            }
            if(shape==Shape.Heart)
            {
                var points=new[]{P(0,-.9f),P(-.95f,.1f),P(-.95f,.6f),P(-.5f,.9f),P(0,.5f),P(.5f,.9f),P(.95f,.6f),P(.95f,.1f)};
                for(var i=0;i<points.Length;i++)Facet(P(0,0),points[i],points[(i+1)%points.Length],i<4?.18f:0);
                return;
            }
            if(shape==Shape.Play)
            {
                Facet(P(-.55f,.86f),P(-.55f,-.86f),P(.82f,0),.06f);
                Facet(P(-.55f,.86f),P(.82f,0),P(-.1f,.28f),.22f);
                return;
            }
            if(shape==Shape.Trophy)
            {
                Facet(P(-.58f,.92f),P(.58f,.92f),P(.38f,.16f),.30f);
                Facet(P(-.58f,.92f),P(-.38f,.16f),P(.38f,.16f),.05f);
                Facet(P(-.38f,.16f),P(.38f,.16f),P(0,-.10f),.18f);
                Path(P(-.20f,-.10f),P(-.20f,-.58f),P(-.46f,-.90f),P(.46f,-.90f),P(.20f,-.58f),P(.20f,-.10f));
                Path(P(-.58f,.92f),P(-.92f,.74f),P(-.92f,.36f),P(-.54f,.18f));
                Path(P(.58f,.92f),P(.92f,.74f),P(.92f,.36f),P(.54f,.18f));
                return;
            }
            if (shape == Shape.Gem) { Path(P(-1,.35f),P(-.55f,.9f),P(.55f,.9f),P(1,.35f),P(0,-1),P(-1,.35f),P(1,.35f)); Path(P(-.55f,.9f),P(0,-1),P(.55f,.9f)); }
            else if (shape == Shape.Heart) Path(P(0,-.9f),P(-.95f,.1f),P(-.95f,.6f),P(-.5f,.9f),P(0,.5f),P(.5f,.9f),P(.95f,.6f),P(.95f,.1f),P(0,-.9f));
            else if (shape == Shape.Layers) { Path(P(-1,.4f),P(0,.95f),P(1,.4f),P(0,-.15f),P(-1,.4f)); Path(P(-1,-.15f),P(0,-.7f),P(1,-.15f)); Path(P(-1,-.55f),P(0,-1),P(1,-.55f)); }
            else if (shape == Shape.Shop) { Path(P(-.85f,.2f),P(-.85f,-.9f),P(.85f,-.9f),P(.85f,.2f)); Path(P(-1,.2f),P(-.75f,.9f),P(.75f,.9f),P(1,.2f),P(-1,.2f)); }
            else if (shape == Shape.Calendar) { Path(P(-.9f,.7f),P(.9f,.7f),P(.9f,-.9f),P(-.9f,-.9f),P(-.9f,.7f)); Line(P(-.9f,.2f),P(.9f,.2f)); Line(P(-.45f,1),P(-.45f,.45f)); Line(P(.45f,1),P(.45f,.45f)); Path(P(-.4f,-.3f),P(-.05f,-.6f),P(.5f,-.15f)); }
            else { for(var i=0;i<3;i++) { var y=.65f-i*.65f; Line(P(-1,y),P(1,y)); var x=i==1?.4f:-.35f; Line(P(x,y-.22f),P(x,y+.22f)); } }
        }
    }
}
