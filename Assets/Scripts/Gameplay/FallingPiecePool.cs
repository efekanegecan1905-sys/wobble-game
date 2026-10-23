using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wobble.Audio;

namespace Wobble.Gameplay
{
    public sealed class FallingPiecePool
    {
        private readonly Queue<BlockView> available = new();
        private readonly Transform parent;
        private readonly Material side;
        private readonly Material top;
        private readonly MonoBehaviour coroutineHost;
        private readonly AudioManager audio;

        public FallingPiecePool(Transform parent, Material sideMaterial, Material topMaterial, MonoBehaviour host, AudioManager audio = null, int preload = 12)
        {
            this.parent = parent; side = sideMaterial; top = topMaterial; coroutineHost = host;
            this.audio=audio;
            for (var i = 0; i < preload; i++) available.Enqueue(Create());
        }

        private BlockView Create()
        {
            var go = new GameObject("Falling marble piece");
            go.transform.SetParent(parent);
            var view = go.AddComponent<BlockView>();
            view.Initialize(Vector3.one, side, top);
            var body = go.AddComponent<Rigidbody>();
            body.mass = 2.8f;
            body.linearDamping = .08f;
            body.angularDamping = .12f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            go.AddComponent<FallingStoneContact>().Initialize(audio);
            go.SetActive(false);
            return view;
        }

        public void Release(Vector3 position, Vector3 size, Vector3 cutSide, Vector3 stoneOrigin)
        {
            var piece = available.Count > 0 ? available.Dequeue() : Create();
            piece.transform.SetPositionAndRotation(position, Quaternion.identity);
            piece.Resize(size);
            piece.RefreshStoneVariation();
            piece.SetStoneOrigin(stoneOrigin);
            piece.gameObject.SetActive(true);
            var body = piece.GetComponent<Rigidbody>();
            body.mass = Mathf.Clamp(size.x * size.y * size.z * 2.7f, .2f, 5f);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearVelocity = cutSide * .12f + Vector3.down * .12f;
            body.angularVelocity = Vector3.Cross(Vector3.up, cutSide) * 1.4f;
            body.WakeUp();
            coroutineHost.StartCoroutine(ReturnLater(piece));
        }

        private IEnumerator ReturnLater(BlockView piece)
        {
            var elapsed = 0f;
            while (piece != null && elapsed < 5f)
            {
                yield return new WaitForSeconds(.15f);
                elapsed += .15f;
                var camera = UnityEngine.Camera.main;
                if (camera != null && elapsed > .5f)
                {
                    var point = camera.WorldToViewportPoint(piece.transform.position);
                    if (point.y < -.2f || point.z < 0 || piece.transform.position.y < camera.transform.position.y - 28f) break;
                }
            }
            if (piece == null) yield break;
            piece.gameObject.SetActive(false);
            available.Enqueue(piece);
        }
    }
}
