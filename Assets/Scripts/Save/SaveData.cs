using System;
using System.Collections.Generic;

namespace Wobble.Save
{
    [Serializable]
    public class SaveData
    {
        public int schemaVersion = 2;
        public int bestScore;
        public int gems;
        public int lives = 5;
        public string selectedTheme = "Carrara";
        public bool hapticsEnabled = true;
        public bool audioEnabled = true;
        public bool musicEnabled = true;
        public int graphicsQuality = 2;
        public string selectedLocale = "tr";
        public int dailyRewardIndex;
        public long lastDailyClaimDay = -1;
        public bool removeAdsOwned;
        public List<string> ownedSkins = new() { "Carrara" };
    }
}
