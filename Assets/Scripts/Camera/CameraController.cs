using UnityEngine;
using Wobble.Gameplay;

namespace Wobble.CameraSystem
{
    public sealed class CameraController : MonoBehaviour
    {
        public const float OrbitDuration = .95f;
        public static float FocusHeight { get; private set; }
        private const float Elevation = 27f;
        private Vector3 target;
        private Vector3 pivot;
        private Vector3 followVelocity;
        private float angle = 42f;
        private float orbitStart = 42f;
        private float desiredAngle = 42f;
        private float orbitTime = OrbitDuration;
        private float impulseTime = 1f;
        private float failTime = 1f;
        private float comboTime = 1f;
        private float comboKick;
        private float placementTime = 1f;
        private float placementKick;
        private float heroTime = 1f;
        private float heroKick;
        private UnityEngine.Camera cam;
        private float followDistance, distanceVelocity;
        public float FollowDistance => followDistance;
        private static readonly Vector3[] CorridorAxes = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        public void Initialize()
        {
            cam = GetComponent<UnityEngine.Camera>();
            cam.fieldOfView = 40f;
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 160f;
            cam.clearFlags = CameraClearFlags.Skybox;
            FollowTop(Vector3.zero);
            pivot = target;
            LateUpdate();
        }

        // Follow the actual top in all axes, including drift caused by cuts.
        // Anticipate the next placement and leave the last slabs below center.
        public void FollowTop(Vector3 top)
        {
            FocusHeight = top.y;
            target = top + Vector3.up * .35f;
        }
        public void FollowHeight(float y) => FollowTop(new Vector3(0, y, 0));

        // Placement, FLOW and FEVER stay under 4.5 cm of travel so the moving slab
        // never becomes harder to read. Fail is deliberately the strongest beat.
        public void PlacementImpulse(float strength = .55f)
        {
            placementTime = 0f;
            placementKick = Mathf.Clamp01(strength);
        }

        public void PerfectImpulse() => PlacementImpulse(1f);

        public void FlowPulse(float strength = 1f)
        {
            heroTime = 0f;
            heroKick = Mathf.Clamp01(strength);
        }

        public void ComboImpulse(int combo)
        {
            comboTime = 0f;
            comboKick = Mathf.Clamp01(combo / 8f);
        }

        public void FailImpulse() => failTime = 0f;

        public void RotateForLevel(int level)
        {
            orbitStart = angle;
            desiredAngle = 42f + (level - 1) * 24f;
            orbitTime = 0f;
        }

        private void LateUpdate()
        {
            if (cam == null) return;
            pivot = Vector3.SmoothDamp(pivot, target, ref followVelocity, .16f, Mathf.Infinity, Time.deltaTime);
            orbitTime = Mathf.Min(OrbitDuration, orbitTime + Time.deltaTime);
            angle = Mathf.LerpAngle(orbitStart, desiredAngle, Mathf.SmoothStep(0, 1, orbitTime / OrbitDuration));
            var rotation = Quaternion.Euler(Elevation, -90f - angle, 0);
            var forward = rotation * Vector3.forward;
            // Fit the actual projected corridor instead of its oversized bounding sphere.
            // Fixed full-size slabs keep framing stable after cuts; tower height never adds retreat.
            var tanVertical = Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
            var horizontalTangent = tanVertical * Mathf.Max(.3f, cam.aspect) * .95f;
            var requiredDistance = 13.2f;
            var inverse = Quaternion.Inverse(rotation);
            foreach (var axis in CorridorAxes)
            for (var x = -1; x <= 1; x += 2)
            for (var z = -1; z <= 1; z += 2)
            {
                var point = inverse * (axis * StackManager.TravelBoundary + new Vector3(x * 1.65f, StackManager.BlockHeight - .35f, z * 1.65f));
                requiredDistance = Mathf.Max(requiredDistance, Mathf.Abs(point.x) / horizontalTangent - point.z);
                requiredDistance = Mathf.Max(requiredDistance, Mathf.Abs(point.y) / (tanVertical * .54f) - point.z);
            }
            // Smooth inward movement, with a visibility floor during an orbit or aspect change.
            followDistance = followDistance == 0 ? requiredDistance : Mathf.Max(requiredDistance,
                Mathf.SmoothDamp(followDistance, requiredDistance, ref distanceVelocity, .35f));
            impulseTime += Time.deltaTime;
            failTime += Time.deltaTime;
            comboTime += Time.deltaTime;
            placementTime += Time.deltaTime;
            heroTime += Time.deltaTime;
            var impulse = impulseTime < .18f ? Mathf.Sin(impulseTime / .18f * Mathf.PI * 2f) * .022f * (1f - impulseTime / .18f) : 0f;
            var combo = comboTime < .26f ? Mathf.Sin(comboTime / .26f * Mathf.PI) * .026f * comboKick : 0f;
            // A short percussive tick on every placement; the falling body never moves.
            var placement = placementTime < .20f
                ? Mathf.Sin(placementTime / .20f * Mathf.PI * 2f) * .016f * placementKick * (1f - placementTime / .20f)
                : 0f;
            // FLOW/FEVER hero pulse: one clean, fast push rather than a sustained shake.
            var hero = heroTime < .38f
                ? Mathf.Sin(heroTime / .38f * Mathf.PI) * .040f * heroKick * (1f - heroTime / .38f)
                : 0f;
            var heroRoll = heroTime < .38f
                ? Mathf.Sin(heroTime / .38f * Mathf.PI) * .13f * heroKick * (1f - heroTime / .38f)
                : 0f;
            // A short, heavy settle when the tower is lost; never runs during framing checks.
            var impact = failTime < .46f ? Mathf.Sin(failTime / .46f * Mathf.PI * 1.5f) * .095f * (1f - failTime / .46f) : 0f;
            var shake = rotation * new Vector3(impact * .55f, impulse + placement + combo + hero + impact, 0f);
            var finalRotation = rotation * Quaternion.Euler(0f, 0f, heroRoll);
            transform.SetPositionAndRotation(pivot - forward * followDistance + shake, finalRotation);
            cam.backgroundColor = RenderSettings.fogColor;
        }
    }
}
