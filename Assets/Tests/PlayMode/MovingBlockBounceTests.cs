using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wobble.Gameplay;

namespace Wobble.Tests
{
    public class MovingBlockBounceTests
    {
        private GameObject block;

        [TearDown]
        public void Cleanup() { if (block != null) Object.DestroyImmediate(block); }

        [TestCase(MoveDirection.PositiveX)]
        [TestCase(MoveDirection.NegativeX)]
        [TestCase(MoveDirection.PositiveZ)]
        [TestCase(MoveDirection.NegativeZ)]
        public void ExactBoundary_ReversesInPlace_AndPreservesObject(MoveDirection direction)
        {
            block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.position = new Vector3(3, 7, -2);
            var moving = block.AddComponent<MovingBlock>();
            moving.Begin(direction, block.transform.position, 1f, 2f);
            var initial = block.transform.position;
            var sign = moving.TravelSign;
            var id = block.GetInstanceID();
            var renderer = block.GetComponent<Renderer>();

            for (var bounce = 0; bounce < 8; bounce++)
            {
                var before = block.transform.position;
                moving.Advance(1f); // exactly two world units, to the opposite boundary
                Assert.That(Vector3.Distance(before, block.transform.position), Is.EqualTo(2f).Within(.00001f));
                Assert.That(moving.TravelSign, Is.EqualTo(-sign));
                Assert.That(Coordinate(moving), Is.EqualTo(sign > 0 ? moving.MaxBoundary : moving.MinBoundary).Within(.00001f));
                Assert.That(block.activeInHierarchy && renderer.enabled, Is.True);
                Assert.That(block.GetInstanceID(), Is.EqualTo(id));
                Assert.That(block.transform.position.y, Is.EqualTo(initial.y));
                Assert.That(moving.Direction, Is.EqualTo(direction), "Incoming direction must not be replaced by travel direction.");
                sign = -sign;
            }
            moving.Stop();
            var stopped = block.transform.position;
            moving.Advance(.3f);
            Assert.That(block.transform.position, Is.EqualTo(stopped), "Placement must freeze the bouncing block.");
        }

        [TestCase(MoveDirection.PositiveX)]
        [TestCase(MoveDirection.NegativeX)]
        [TestCase(MoveDirection.PositiveZ)]
        [TestCase(MoveDirection.NegativeZ)]
        public void Overshoot_IsReflectedWithoutLosingSpeedOrWrapping(MoveDirection direction)
        {
            block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var moving = block.AddComponent<MovingBlock>();
            moving.Begin(direction, Vector3.zero, 1f, 2f);
            var sign = moving.TravelSign;
            moving.Advance(.975f);
            var nearEdge = Coordinate(moving);
            moving.Advance(.05f); // .05 to edge, .05 back: same position, reversed
            Assert.That(Coordinate(moving), Is.EqualTo(nearEdge).Within(.00001f));
            Assert.That(moving.TravelSign, Is.EqualTo(-sign));
            moving.Advance(.05f);
            Assert.That(Coordinate(moving), Is.EqualTo(nearEdge - sign * .1f).Within(.00001f));
            moving.Advance(4f); // Multiple complete bounces in a long frame.
            Assert.That(Coordinate(moving), Is.EqualTo(nearEdge - sign * .1f).Within(.00002f));
        }

        [UnityTest]
        public IEnumerator LiveUpdates_AllFourDirections_BounceRepeatedlyAndRemainVisible()
        {
            foreach (var direction in new[] { MoveDirection.PositiveX, MoveDirection.NegativeZ, MoveDirection.NegativeX, MoveDirection.PositiveZ })
            {
                block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.position = new Vector3(0, 2, 0);
                block.transform.localScale = Vector3.one * .2f;
                var moving = block.AddComponent<MovingBlock>();
                moving.Begin(direction, block.transform.position, .45f, 2f);
                var renderer = block.GetComponent<Renderer>();
                var id = block.GetInstanceID();
                var previous = block.transform.position;
                var sign = moving.TravelSign;
                var reversals = 0;
                var elapsed = 0f;
                while (reversals < 6 && elapsed < 6f)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                    var step = Vector3.Distance(previous, block.transform.position);
                    Assert.That(step, Is.LessThanOrEqualTo(2f * Time.deltaTime + .0001f), "No frame may jump to the opposite edge.");
                    Assert.That(Coordinate(moving), Is.InRange(moving.MinBoundary - .0001f, moving.MaxBoundary + .0001f));
                    Assert.That(block.activeInHierarchy && renderer.enabled, Is.True);
                    Assert.That(block.GetInstanceID(), Is.EqualTo(id));
                    if (moving.TravelSign != sign)
                    {
                        reversals++;
                        sign = moving.TravelSign;
                        Assert.That(Mathf.Min(Mathf.Abs(Coordinate(moving) - moving.MinBoundary), Mathf.Abs(Coordinate(moving) - moving.MaxBoundary)), Is.LessThanOrEqualTo(2f * Time.deltaTime + .0001f));
                    }
                    previous = block.transform.position;
                }
                Assert.That(reversals, Is.EqualTo(6));
                moving.Stop();
                previous = block.transform.position;
                yield return null;
                Assert.That(block.transform.position, Is.EqualTo(previous));
                Object.Destroy(block);
                yield return null;
            }
        }

        private static float Coordinate(MovingBlock moving) => Vector3.Dot(moving.transform.position, moving.Axis);
    }
}
