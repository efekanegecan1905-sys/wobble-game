using System;
using Wobble.Save;

namespace Wobble.Economy
{
    public interface IRewardClock { DateTimeOffset UtcNow { get; } }
    public sealed class LocalRewardClock : IRewardClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }

    public sealed class DailyRewardService
    {
        private readonly SaveManager save;
        private readonly MenuCatalog catalog;
        private readonly IRewardClock clock;
        public DailyRewardService(SaveManager save, MenuCatalog catalog, IRewardClock clock = null)
        { this.save = save; this.catalog = catalog; this.clock = clock ?? new LocalRewardClock(); }
        private long Today => clock.UtcNow.ToUnixTimeSeconds() / 86400;
        public int NextIndex => save.Data.dailyRewardIndex;
        public bool CanClaim => Today > save.Data.lastDailyClaimDay && catalog.dailyRewards?.Length == 7;
        public bool Claim()
        {
            if (!CanClaim) return false;
            var reward = catalog.dailyRewards[NextIndex];
            // Reward and claim stamp are persisted together, before another tap is accepted.
            save.Data.gems = (int)Math.Min(int.MaxValue, (long)save.Data.gems + Math.Max(0, reward.gems));
            save.Data.lives = (int)Math.Min(int.MaxValue, (long)save.Data.lives + Math.Max(0, reward.lives));
            save.Data.lastDailyClaimDay = Today;
            save.Data.dailyRewardIndex = (NextIndex + 1) % 7;
            save.Save();
            return true;
        }
    }
}
