using UnityEngine;
namespace Wobble.UI
{
    // Vector geometry, shared UI material, no texture or glyph dependency.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ArcadeGauge:UnityEngine.UI.MaskableGraphic
    {
        public ArcadeGauge(){useLegacyMeshGeneration=false;}
        private float amount;
        public float Value {get=>amount;set{if(Mathf.Abs(amount-value)<.001f)return;amount=value;SetVerticesDirty();}}
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();float radius=Mathf.Min(rectTransform.rect.width*.43f,rectTransform.rect.height*.85f);Vector2 center=new Vector2(0,-rectTransform.rect.height*.35f);
            for(int i=0;i<48;i++)
            {
                float a=Mathf.Lerp(170,10,i/48f)*Mathf.Deg2Rad,b=Mathf.Lerp(170,10,(i+1)/48f)*Mathf.Deg2Rad;
                Color c=i<5||i>42?ArcadeStyle.Coral:Color.Lerp(ArcadeStyle.Violet,ArcadeStyle.Mist,.3f);
                Quad(vh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,
                    center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(radius-15),center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(radius-15),c);
            }
            for(int i=0;i<=10;i++)
            {
                float a=Mathf.Lerp(170,10,i/10f)*Mathf.Deg2Rad;var axis=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                Line(vh,center+axis*(radius-24),center+axis*(radius-(i==5?52:37)),3,ArcadeStyle.Ink);
            }
            float angle=(90-Mathf.Clamp(amount,-1,1)*80)*Mathf.Deg2Rad;
            Line(vh,center,center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(radius-35),7,ArcadeStyle.Ink);
        }
        private void Line(UnityEngine.UI.VertexHelper vh,Vector2 a,Vector2 b,float width,Color c)
        {var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Quad(vh,a-n,b-n,b+n,a+n,c);}
        private void Quad(UnityEngine.UI.VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color)
        {int i=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddVert(d,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
    }
}
