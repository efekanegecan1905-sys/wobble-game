using System;

namespace Wobble.Gameplay
{
    public sealed class ComboManager
    {
        public int ConsecutivePerfects { get; private set; }
        public int MaximumCombo { get; private set; }
        public int PerfectCount { get; private set; }
        public int DisplayMultiplier => ConsecutivePerfects + 1;
        public event Action<int> ComboChanged;

        public void ResetRun()
        {
            ConsecutivePerfects = 0; MaximumCombo = 0; PerfectCount = 0;
        }

        public int Register(bool perfect)
        {
            if (!perfect)
            {
                ConsecutivePerfects = 0;
                return 1;
            }
            PerfectCount++;
            ConsecutivePerfects++;
            MaximumCombo = Math.Max(MaximumCombo, DisplayMultiplier);
            ComboChanged?.Invoke(DisplayMultiplier);
            return 1 + Math.Min(ConsecutivePerfects, 5);
        }

        public void BreakChain() => ConsecutivePerfects = 0;
    }
}
