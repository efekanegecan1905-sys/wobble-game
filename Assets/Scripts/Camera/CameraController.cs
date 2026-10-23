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
        public void PerfectImpulse() => impulseTime = 0f;

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
            var requiredDistance = 14f;
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
            var impulse = impulseTime < .18f ? Mathf.Sin(impulseTime / .18f * Mathf.PI * 2f) * .022f * (1f - impulseTime / .18f) : 0f;
            transform.SetPositionAndRotation(pivot - forward * followDistance + rotation * Vector3.up * impulse, rotation);
            cam.backgroundColor = RenderSettings.fogColor;
        }
    }
}
