using System;
using UnityEngine;

namespace Wobble.Gameplay
{
    public sealed class LevelManager
    {
        public int Level { get; private set; } = 1;
        public int PlacedBlocks { get; private set; }
        public const float InitialSpeed = 4f;
        public const float LevelSpeedGain = .35f;
        public const float MaximumSpeed = 6.6f;
        public float Speed { get; private set; } = InitialSpeed;
        public event Action<int> LevelUp;

        public void ResetRun()
        {
            Level = 1; PlacedBlocks = 0; Speed = InitialSpeed;
        }

        public void RegisterPlacement()
        {
            PlacedBlocks++;
            if (PlacedBlocks % 10 != 0) return;
            Level++;
            Speed = Mathf.Min(MaximumSpeed, InitialSpeed + (Level - 1) * LevelSpeedGain);
            LevelUp?.Invoke(Level);
        }
    }
}
