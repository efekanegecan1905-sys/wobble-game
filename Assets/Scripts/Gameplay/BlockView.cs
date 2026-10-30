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
        private Coroutine wobbleRoutine;
        private Quaternion leanRotation = Quaternion.identity;
        private float tensionAmount, tensionPhase;
        private bool tensionApplied;

        private void LateUpdate()
        {
            if (meshRenderer == null) return;
            // Only nearby slabs contribute to the active tower's shadow budget.
            meshRenderer.shadowCastingMode = Mathf.Abs(transform.position.y - Wobble.CameraSystem.CameraController.FocusHeight) < 3.6f
                ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            // WOBBLE is render-only: each placed slab sways a fraction of a degree
            // around its own centre. The collider, size and next placement stay exact.
            if (visual == null) return;
            if (tensionAmount > .0001f || tensionApplied)
            {
                var time = Time.time;
                var sway = Mathf.Sin(time * 7.1f + tensionPhase) * tensionAmount;
                var lean = Mathf.Cos(time * 5.7f + tensionPhase * 1.31f) * tensionAmount * .72f;
                visual.localRotation = leanRotation * Quaternion.Euler(lean, 0, sway);
                tensionApplied = tensionAmount > .0001f;
            }
            else visual.localRotation = leanRotation;
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

        public void Punch(float strength = 1f, bool sweep = false)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchAnimation(Mathf.Clamp01(strength), sweep));
        }

        // A short, damped lean for off-centre cuts. Rotates only the visual child,
        // so the slab's size, collider and stacking height never change.
        public void Lean(float strength = 1f)
        {
            if (visual == null) return;
            if (wobbleRoutine != null) StopCoroutine(wobbleRoutine);
            wobbleRoutine = StartCoroutine(LeanAnimation(strength));
        }

        // Presentation-only tension supplied by StackManager. Amplitude is in
        // degrees and is composed with the off-centre lean, never with the collider.
        public void SetTension(float amount, float phase)
        {
            tensionAmount = Mathf.Max(0f, amount);
            tensionPhase = phase;
        }

        private IEnumerator LeanAnimation(float strength)
        {
            const float duration = .34f;
            var axis = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            if (axis.sqrMagnitude < .01f) axis = Vector3.right;
            axis.Normalize();
            var amount = Mathf.Lerp(1.4f, 4.2f, Mathf.Clamp01(strength));
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var t = elapsed / duration;
                var angle = Mathf.Sin(t * Mathf.PI * 4f) * amount * (1f - t);
                leanRotation = Quaternion.AngleAxis(angle, axis);
                yield return null;
            }
            leanRotation = Quaternion.identity;
            wobbleRoutine = null;
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

        private IEnumerator PunchAnimation(float strength, bool sweep)
        {
            // 70-120 ms tactile punch on the render root only. The peak is 1.2-1.8%
            // wide and a hair flatter, which reads as weight without ever touching
            // the gameplay footprint or the stack spacing.
            const float duration = .105f;
            var peak = Mathf.Lerp(.012f, .018f, strength);
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var t = elapsed / duration;
                var pulse = Mathf.Sin(t * Mathf.PI);
                visual.localScale = new Vector3(1f + pulse * peak, 1f - pulse * peak * 1.18f, 1f + pulse * peak);
                properties.SetFloat("_Flash", (1f - t) * (sweep ? .62f : .12f));
                properties.SetFloat("_Sweep", sweep ? t : -1f);
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
                // The rendered slab is 12% taller than the .40 collider: it looks
                // like the approved .44-.46 stone while cuts and stacking keep the
                // original height. Adjacent slabs overlap by a few millimetres,
                // which hides hairline gaps and reads as compressed stone.
                var ringHeights = new[] { -.56f, -.50f, .50f, .56f };
                var ringWidths = new[] { .478f, .5f, .5f, .478f };
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
