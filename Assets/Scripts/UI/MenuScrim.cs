using UnityEngine;
using UnityEngine.UI;

namespace Wobble.UI
{
    // A cheap two-stop vertical scrim painted straight into the canvas. Used behind
    // the home controls so cream type stays readable over a bright 3D world without
    // adding a second full-screen texture or a post effect. The solid part hugs the
    // bottom, then the fade starts at `hold`.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuScrim : MaskableGraphic
    {
        public Color bottomColor = new(0.04f, 0.06f, 0.09f, .58f);
        public Color topColor = new(0.04f, 0.06f, 0.09f, 0f);
        [Range(0f, 1f)] public float hold = .42f;

        public MenuScrim() { useLegacyMeshGeneration = false; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            var split = Mathf.Clamp01(hold);
            var ySplit = Mathf.Lerp(rect.yMin, rect.yMax, split);
            var solid = bottomColor;
            // Solid band.
            Band(vh, rect.xMin, rect.yMin, rect.xMax, ySplit, solid, solid);
            // Fade band.
            Band(vh, rect.xMin, ySplit, rect.xMax, rect.yMax, solid, topColor);
        }

        private static void Band(VertexHelper vh, float xMin, float yMin, float xMax, float yMax, Color low, Color high)
        {
            var index = vh.currentVertCount;
            vh.AddVert(new Vector3(xMin, yMin), low, Vector2.zero);
            vh.AddVert(new Vector3(xMin, yMax), high, Vector2.up);
            vh.AddVert(new Vector3(xMax, yMax), high, Vector2.one);
            vh.AddVert(new Vector3(xMax, yMin), low, Vector2.right);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
