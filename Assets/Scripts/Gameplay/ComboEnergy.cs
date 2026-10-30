using UnityEngine;

namespace Wobble.Gameplay
{
    // Presentation-only combo state. It never changes scoring, speed, input or
    // collision; it simply gives the audio/visual layer a smooth 0..2 energy value
    // so FLOW and FEVER can ramp in and relax instead of hard-cutting.
    public sealed class ComboEnergy
    {
        public int TargetTier { get; private set; }
        public int Tier { get; private set; }
        public float Level { get; private set; }
        public float Intensity => Mathf.Clamp01(Level * .5f);

        // A clean attack and a slightly longer release: the drop back to normal is
        // felt as a release, not as a switch being turned off. Both durations are
        // full-range (0..2), so the whole escalation lands in ~.16 s and relaxes in
        // ~.42 s regardless of which tier was reached.
        private const float Attack = .16f;
        private const float Release = .42f;
        private const float TierRange = 2f;

        public void Reset()
        {
            TargetTier = Tier = 0;
            Level = 0f;
        }

        // Returns the tier reached by this placement (0 when there is no escalation).
        public int Push(bool perfect, int combo)
        {
            var target = !perfect ? 0 : combo >= 8 ? 2 : combo >= 5 ? 1 : 0;
            var escalated = target > TargetTier ? target : 0;
            TargetTier = target;
            Tier = target;
            return escalated;
        }

        public void Tick(float dt)
        {
            var speed = TargetTier > Level ? TierRange / Attack : TierRange / Release;
            Level = Mathf.MoveTowards(Level, TargetTier, Mathf.Max(0f, dt) * speed);
            Tier = TargetTier;
        }
    }
}
