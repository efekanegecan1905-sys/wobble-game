#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Wobble.Editor
{
    // Geometry baked once in the editor, never generated during startup.
    public static class ProductPresentationBuilder
    {
        [MenuItem("WOBBLE/Presentation/Bake assets")]
        public static void Bake()
        {
            Directory.CreateDirectory("Assets/Resources/Presentation");AssetDatabase.Refresh();
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);var source=cube.GetComponent<MeshFilter>().sharedMesh;
            var shapes=new[]{"W","O","B","B","L","E"};
            for(var letter=0;letter<6;letter++)
            {
                var bars=new List<CombineInstance>();
                void Bar(float x,float y,float w,float h,float angle=0)=>bars.Add(new CombineInstance{mesh=source,transform=Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,angle),new Vector3(w,h,.22f))});
                var ch=shapes[letter];
                if(ch=="W")
                {
                    var points=new[]{new Vector2(-.4f,.5f),new Vector2(-.2f,-.5f),new Vector2(0,.12f),new Vector2(.2f,-.5f),new Vector2(.4f,.5f)};
                    for(var j=0;j<4;j++){var a=points[j];var b=points[j+1];var mid=(a+b)*.5f;var delta=b-a;Bar(mid.x,mid.y,.14f,delta.magnitude,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90);}
                }
                else
                {
                    Bar(-.32f,0,.15f,1);
                    Bar(0,-.44f,.76f,.14f);
                    if(ch!="L")Bar(0,.44f,.76f,.14f);
                    if(ch=="O"||ch=="B")Bar(.32f,0,.15f,1);
                    if(ch=="B"||ch=="E")Bar(0,0,.69f,.14f);
                }
                var mesh=new Mesh{name="Stone letter "+ch};mesh.CombineMeshes(bars.ToArray());SaveMesh(mesh,"Letter"+letter);
            }
            Object.DestroyImmediate(cube);
            // Chamfered, tapering eight-sided forms with an intentionally fractured crown.
            SaveMesh(Pier(1,.78f,0),"Monolith");
            SaveMesh(Pier(1.4f,1.05f,.24f),"Quarry");
            SaveMesh(Pier(.78f,.52f,-.18f),"Column");
            UpdateStrings();AssetDatabase.SaveAssets();
        }
        private static Mesh Pier(float lower,float upper,float slope)
        {
            var outline=new[]{new Vector2(-.4f,-.5f),new Vector2(.4f,-.5f),new Vector2(.5f,-.4f),new Vector2(.5f,.4f),new Vector2(.4f,.5f),new Vector2(-.4f,.5f),new Vector2(-.5f,.4f),new Vector2(-.5f,-.4f)};
            var vertices=new List<Vector3>();var triangles=new List<int>();
            void Tri(Vector3 a,Vector3 b,Vector3 c){var n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);}
            for(var i=0;i<8;i++)
            {
                var a=outline[i];var b=outline[(i+1)%8];
                var p=new Vector3(a.x*lower,-.5f,a.y*lower);var q=new Vector3(b.x*lower,-.5f,b.y*lower);
                var r=new Vector3(a.x*upper,.5f+a.x*slope,a.y*upper);var s=new Vector3(b.x*upper,.5f+b.x*slope,b.y*upper);
                Tri(p,r,q);Tri(q,r,s);Tri(Vector3.up*.5f,s,r);Tri(Vector3.down*.5f,p,q);
            }
            var mesh=new Mesh{name="Baked chamfered architectural stone"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private static void SaveMesh(Mesh mesh,string name)
        {
            var path="Assets/Resources/Presentation/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);}
        }
        private static void UpdateStrings()
        {
            var collection=LocalizationEditorSettings.GetStringTableCollection("Wobble");
            var lines=File.ReadAllLines("Assets/LocalizationAuthoring/Wobble.tsv");
            foreach(var line in lines)
            {
                var cells=line.Split('\t');if(cells.Length!=3||cells[0]=="key")continue;
                for(var i=0;i<2;i++)
                {
                    var table=(StringTable)collection.GetTable(new LocaleIdentifier(i==0?"tr":"en"));
                    var entry=table.AddEntry(cells[0],cells[i+1].Replace("\\n","\n"));entry.IsSmart=cells[i+1].Contains("{");EditorUtility.SetDirty(table);
                }
            }
            EditorUtility.SetDirty(collection.SharedData);
        }
        public static void BuildReview()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Wobble.unity"},locationPathName="/private/tmp/wobble-product-player/WOBBLE.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Product review build failed.");
        }
    }
}
#endif
