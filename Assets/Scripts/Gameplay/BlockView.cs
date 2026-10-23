using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Wobble.Gameplay
{
    public sealed class BlockView : MonoBehaviour
    {
        public Vector3 Size { get; private set; }
        public Vector3 StoneOrigin { get; private set; }
        private BoxCollider boxCollider;
        private Transform visual;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock properties;
        private Coroutine punchRoutine;

        private void LateUpdate()
        {
            if (meshRenderer == null) return;
            // Only nearby slabs contribute to the active tower's shadow budget.
            meshRenderer.shadowCastingMode = Mathf.Abs(transform.position.y - Wobble.CameraSystem.CameraController.FocusHeight) < 3.6f
                ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void Initialize(Vector3 size, Material sideMaterial, Material topMaterial)
        {
            Size = size;
            visual = new GameObject("Stone surface").transform;
            visual.SetParent(transform, false);
            var meshFilter = visual.gameObject.AddComponent<MeshFilter>();
            meshRenderer = visual.gameObject.AddComponent<MeshRenderer>();
            properties = new MaterialPropertyBlock();
            boxCollider = gameObject.AddComponent<BoxCollider>();
            meshFilter.sharedMesh = BlockMesh.Create();
            meshRenderer.sharedMaterials = new[] { sideMaterial, topMaterial };
            Resize(size);
            RefreshStoneVariation();
        }

        public void Resize(Vector3 size)
        {
            Size = new Vector3(Mathf.Max(.001f, size.x), Mathf.Max(.001f, size.y), Mathf.Max(.001f, size.z));
            transform.localScale = Size;
            if (boxCollider != null) boxCollider.size = Vector3.one;
            if (properties != null) ApplyStoneCoordinates();
        }

        public void Punch(float strength = 1f)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchAnimation(strength));
        }

        public void RefreshStoneVariation()
        {
            properties.Clear();
            var height = transform.position.y;
            StoneOrigin = new Vector3(height * 2.37f, height * .73f, height * 1.19f);
            properties.SetFloat("_Sweep", -1f);
            ApplyStoneCoordinates();
        }

        public void SetStoneOrigin(Vector3 origin)
        {
            StoneOrigin = origin;
            ApplyStoneCoordinates();
        }

        private void ApplyStoneCoordinates()
        {
            properties.SetVector("_StoneSize", Size);
            properties.SetVector("_StoneOrigin", StoneOrigin);
            meshRenderer.SetPropertyBlock(properties);
        }

        private IEnumerator PunchAnimation(float strength)
        {
            const float duration = .22f;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var t = elapsed / duration;
                var pulse = Mathf.Sin(t * Mathf.PI) * (1f - t);
                // Animate only the surface; overlap dimensions and collider stay exact.
                visual.localScale = new Vector3(1f + pulse * .014f * strength, 1f - pulse * .035f * strength, 1f + pulse * .014f * strength);
                properties.SetFloat("_Flash", (1f - t) * .07f * strength);
                properties.SetFloat("_Sweep", t);
                meshRenderer.SetPropertyBlock(properties);
                yield return null;
            }
            visual.localScale = Vector3.one;
            properties.SetFloat("_Flash", 0);
            properties.SetFloat("_Sweep", -1);
            ApplyStoneCoordinates();
            punchRoutine = null;
        }

        private static class BlockMesh
        {
            private static Mesh cached;
            public static Mesh Create()
            {
                if (cached != null) return cached;
                var vertices = new List<Vector3>();
                var uvs = new List<Vector2>();
                var sides = new List<int>();
                var caps = new List<int>();
                // Shave only the rendered faces by 2%; collider and stack spacing
                // retain their approved dimensions. The fine seam catches contact AO.
                var ringHeights = new[] { -.49f, -.445f, .445f, .49f };
                var ringWidths = new[] { .492f, .5f, .5f, .492f };
                var corners = new[] { new Vector2(-1,-1), new Vector2(1,-1), new Vector2(1,1), new Vector2(-1,1) };
                Vector3 Ring(int ring, int corner) => new Vector3(corners[corner].x * ringWidths[ring], ringHeights[ring], corners[corner].y * ringWidths[ring]);
                void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, List<int> indices)
                {
                    var index = vertices.Count;
                    vertices.AddRange(new[] { a, b, c, d });
                    uvs.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                    indices.AddRange(new[] { index, index + 2, index + 1, index, index + 3, index + 2 });
                }
                for (var ring = 0; ring < 3; ring++)
                for (var side = 0; side < 4; side++)
                {
                    var next = (side + 1) % 4;
                    Quad(Ring(ring, side), Ring(ring, next), Ring(ring + 1, next), Ring(ring + 1, side), sides);
                }
                Quad(Ring(3,0), Ring(3,1), Ring(3,2), Ring(3,3), caps);
                Quad(Ring(0,3), Ring(0,2), Ring(0,1), Ring(0,0), caps);
                cached = new Mesh { name = "WOBBLE Block" };
                cached.SetVertices(vertices);
                cached.subMeshCount = 2;
                cached.SetTriangles(sides, 0);
                cached.SetTriangles(caps, 1);
                cached.SetUVs(0, uvs);
                cached.RecalculateNormals();
                cached.RecalculateBounds();
                return cached;
            }
        }
    }
}
