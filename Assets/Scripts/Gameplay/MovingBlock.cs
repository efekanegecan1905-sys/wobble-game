using UnityEngine;

namespace Wobble.Gameplay
{
    public enum MoveDirection { PositiveX, NegativeZ, NegativeX, PositiveZ }

    public sealed class MovingBlock : MonoBehaviour
    {
        public MoveDirection Direction { get; private set; }
        public bool IsMoving { get; private set; }
        public Vector3 Axis { get; private set; }
        public int TravelSign { get; private set; }
        public float MinBoundary { get; private set; }
        public float MaxBoundary { get; private set; }
        private float speed;

        public void Begin(MoveDirection direction, Vector3 towerCenter, float travel, float moveSpeed)
        {
            Direction = direction;
            speed = Mathf.Max(0f, moveSpeed);
            Axis = direction is MoveDirection.PositiveX or MoveDirection.NegativeX ? Vector3.right : Vector3.forward;
            var positive = direction is MoveDirection.PositiveX or MoveDirection.PositiveZ;
            travel = Mathf.Max(.01f, travel);
            MinBoundary = GetCoordinate(towerCenter) - travel;
            MaxBoundary = GetCoordinate(towerCenter) + travel;
            TravelSign = positive ? 1 : -1;
            SetCoordinate(positive ? MinBoundary : MaxBoundary);
            IsMoving = true;
        }

        private void Update() => Advance(Time.deltaTime);

        internal void Advance(float deltaTime)
        {
            if (!IsMoving) return;
            var coordinate = GetCoordinate(transform.position);
            var remaining = speed * Mathf.Max(0f, deltaTime);
            // Consume the full frame's travel, reflecting any overshoot at the edge.
            // No time reset, lost travel, replacement object or opposite-edge snap.
            while (remaining > 0f)
            {
                var boundary = TravelSign > 0 ? MaxBoundary : MinBoundary;
                var distance = Mathf.Abs(boundary - coordinate);
                if (remaining < distance)
                {
                    coordinate += TravelSign * remaining;
                    break;
                }
                coordinate = boundary;
                remaining -= distance;
                TravelSign = -TravelSign;
            }
            SetCoordinate(coordinate);
        }

        public void Stop() => IsMoving = false;
        private float GetCoordinate(Vector3 point) => Axis.x != 0 ? point.x : point.z;
        private void SetCoordinate(float value)
        {
            var p = transform.position;
            if (Axis.x != 0) p.x = value; else p.z = value;
            transform.position = p;
        }
    }
}
