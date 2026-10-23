using UnityEngine;

namespace Wobble.Gameplay
{
    public readonly struct CutResult
    {
        public readonly bool Success;
        public readonly bool Perfect;
        public readonly Vector3 KeptPosition;
        public readonly Vector3 KeptSize;
        public readonly Vector3 CutPosition;
        public readonly Vector3 CutSize;

        public CutResult(bool success, bool perfect, Vector3 keptPosition, Vector3 keptSize, Vector3 cutPosition, Vector3 cutSize)
        {
            Success = success; Perfect = perfect; KeptPosition = keptPosition; KeptSize = keptSize; CutPosition = cutPosition; CutSize = cutSize;
        }
    }

    public static class BlockCutter
    {
        public static CutResult Calculate(BlockView moving, BlockView previous, Vector3 axis, float perfectTolerance)
        {
            var a = axis.x != 0 ? moving.transform.position.x : moving.transform.position.z;
            var b = axis.x != 0 ? previous.transform.position.x : previous.transform.position.z;
            var movingLength = axis.x != 0 ? moving.Size.x : moving.Size.z;
            var previousLength = axis.x != 0 ? previous.Size.x : previous.Size.z;
            var delta = a - b;

            if (Mathf.Abs(delta) <= perfectTolerance)
                return new CutResult(true, true, previous.transform.position + Vector3.up * moving.Size.y, moving.Size, default, default);

            var overlap = (movingLength + previousLength) * .5f - Mathf.Abs(delta);
            if (overlap <= 0f) return new CutResult(false, false, default, default, moving.transform.position, moving.Size);

            var keptSize = moving.Size;
            var cutSize = moving.Size;
            if (axis.x != 0) { keptSize.x = overlap; cutSize.x = movingLength - overlap; }
            else { keptSize.z = overlap; cutSize.z = movingLength - overlap; }

            var sign = Mathf.Sign(delta);
            var previousEdge = b + sign * previousLength * .5f;
            var movingOuterEdge = a + sign * movingLength * .5f;
            var keptCoordinate = (previousEdge + (a - sign * movingLength * .5f)) * .5f;
            var cutCoordinate = (previousEdge + movingOuterEdge) * .5f;
            if (sign < 0f)
            {
                keptCoordinate = ((a + movingLength * .5f) + (b - previousLength * .5f)) * .5f;
                cutCoordinate = ((b - previousLength * .5f) + (a - movingLength * .5f)) * .5f;
            }

            var keptPosition = moving.transform.position;
            var cutPosition = moving.transform.position;
            if (axis.x != 0) { keptPosition.x = keptCoordinate; cutPosition.x = cutCoordinate; }
            else { keptPosition.z = keptCoordinate; cutPosition.z = cutCoordinate; }
            return new CutResult(true, false, keptPosition, keptSize, cutPosition, cutSize);
        }
    }
}
