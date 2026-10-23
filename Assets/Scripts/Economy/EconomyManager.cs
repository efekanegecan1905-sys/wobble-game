using Wobble.Save;

namespace Wobble.Economy
{
    public sealed class EconomyManager
    {
        private readonly SaveManager save;
        public bool DemoMode { get; }
        public int Gems => DemoMode ? int.MaxValue : save.Data.gems;
        public int Lives => save.Data.lives;
        public bool RemoveAdsOwned => save.Data.removeAdsOwned;

        public EconomyManager(SaveManager saveManager, bool demoMode = false) { save = saveManager; DemoMode = demoMode; }

        public void AddGems(int amount)
        {
            GrantReward(amount, 0);
        }

        public bool TrySpendGems(int amount)
        {
            if (DemoMode) return amount >= 0;
            if (amount < 0 || save.Data.gems < amount) return false;
            save.Data.gems -= amount;
            save.Save();
            return true;
        }

        public void GrantReward(int gems, int lives)
        {
            save.Data.gems = (int)System.Math.Min(int.MaxValue, (long)save.Data.gems + System.Math.Max(0, gems));
            save.Data.lives = (int)System.Math.Min(int.MaxValue, (long)save.Data.lives + System.Math.Max(0, lives));
            save.Save();
        }

        public bool CanContinueWithLife() => Lives > 0;
        public bool ConsumeLifeForContinue()
        {
            if (!CanContinueWithLife()) return false;
            save.Data.lives--;
            save.Save();
            return true;
        }
        // Future integration points: no state change until gameplay continuation exists.
        public bool ContinueWithLife() => false;
        public bool ContinueWithRewardedAd() => false;
        public void PurchaseRemoveAds() { }
        public void PurchaseStarterPack() { }
        public void PurchaseSkinPack(string packId) { }
        public void PurchaseSeasonPass(string seasonId) { }
    }
}
