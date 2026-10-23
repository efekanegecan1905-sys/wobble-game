#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Wobble.Gameplay;

namespace Wobble.Editor.Tests
{
    public class BlockCutterTests
    {
        private Material material;
        private GameObject previousObject;
        private GameObject movingObject;

        [SetUp]
        public void SetUp()
        {
            material = new Material(Shader.Find("Standard"));
            previousObject = new GameObject("Previous");
            movingObject = new GameObject("Moving");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(previousObject);
            Object.DestroyImmediate(movingObject);
            Object.DestroyImmediate(material);
        }

        [Test]
        public void OffsetPlacement_ResizesKeptAndCutPieces()
        {
            var previous = previousObject.AddComponent<BlockView>();
            previous.Initialize(new Vector3(4f, .42f, 4f), material, material);
            var moving = movingObject.AddComponent<BlockView>();
            moving.Initialize(new Vector3(4f, .42f, 4f), material, material);
            moving.transform.position = new Vector3(.7f, .42f, 0);

            var result = BlockCutter.Calculate(moving, previous, Vector3.right, .11f);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Perfect, Is.False);
            Assert.That(result.KeptSize.x, Is.EqualTo(3.3f).Within(.001f));
            Assert.That(result.CutSize.x, Is.EqualTo(.7f).Within(.001f));
        }

        [Test]
        public void PlacementInsideTolerance_SnapsPerfectlyWithoutLoss()
        {
            var previous = previousObject.AddComponent<BlockView>();
            previous.Initialize(new Vector3(3f, .42f, 3f), material, material);
            var moving = movingObject.AddComponent<BlockView>();
            moving.Initialize(new Vector3(3f, .42f, 3f), material, material);
            moving.transform.position = new Vector3(.1f, .42f, 0);

            var result = BlockCutter.Calculate(moving, previous, Vector3.right, .11f);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Perfect, Is.True);
            Assert.That(result.KeptSize.x, Is.EqualTo(3f));
            Assert.That(result.KeptPosition.x, Is.EqualTo(0f));
        }

        [Test]
        public void NoOverlap_IsAMiss()
        {
            var previous = previousObject.AddComponent<BlockView>();
            previous.Initialize(new Vector3(3f, .42f, 3f), material, material);
            var moving = movingObject.AddComponent<BlockView>();
            moving.Initialize(new Vector3(3f, .42f, 3f), material, material);
            moving.transform.position = new Vector3(3.01f, .42f, 0);

            var result = BlockCutter.Calculate(moving, previous, Vector3.right, .11f);
            Assert.That(result.Success, Is.False);
        }
    }
}
#endif
