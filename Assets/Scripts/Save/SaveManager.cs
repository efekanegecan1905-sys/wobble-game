using UnityEngine;
using System;
using System.Collections.Generic;

namespace Wobble.Save
{
    public sealed class SaveManager
    {
        private const string Key = "wobble.save.v1";
        private readonly ISaveStore store;
        public SaveData Data { get; private set; }
        public event Action Changed;

        public SaveManager(ISaveStore store = null) { this.store = store ?? new PlayerPrefsSaveStore(); Load(); }

        public void Load()
        {
            var json = store.Read(Key);
            Data = new SaveData();
            if (!string.IsNullOrWhiteSpace(json))
            {
                try { JsonUtility.FromJsonOverwrite(json, Data); }
                catch (ArgumentException) { store.Write(Key + ".recovery", json); Data = new SaveData(); }
            }
            Data.schemaVersion = 2;
            Data.gems = Mathf.Max(0, Data.gems);
            Data.lives = Mathf.Max(0, Data.lives);
            Data.bestScore = Mathf.Max(0, Data.bestScore);
            Data.graphicsQuality = Mathf.Clamp(Data.graphicsQuality, 0, 2);
            if (Data.selectedLocale != "tr" && Data.selectedLocale != "en") Data.selectedLocale = "tr";
            Data.ownedSkins ??= new List<string>();
            if (!Data.ownedSkins.Contains("Carrara")) Data.ownedSkins.Add("Carrara");
            if (!Data.ownedSkins.Contains(Data.selectedTheme)) Data.selectedTheme = "Carrara";
            Data.dailyRewardIndex = Mathf.Clamp(Data.dailyRewardIndex, 0, 6);
        }

        public void Save()
        {
            store.Write(Key, JsonUtility.ToJson(Data));
            Changed?.Invoke();
        }

        public bool SubmitScore(int score)
        {
            if (score <= Data.bestScore) return false;
            Data.bestScore = score;
            Save();
            return true;
        }
    }

    public interface ISaveStore
    {
        string Read(string key);
        void Write(string key, string value);
    }

    // Demo changes remain in memory, including score, settings and unlocks.
    public sealed class DemoSaveStore : ISaveStore
    {
        private readonly Dictionary<string,string> values = new();
        public string Read(string key) => values.TryGetValue(key, out var value) ? value : PlayerPrefs.GetString(key, string.Empty);
        public void Write(string key, string value) => values[key] = value;
    }

    internal sealed class PlayerPrefsSaveStore : ISaveStore
    {
        public string Read(string key) => PlayerPrefs.GetString(key, string.Empty);
        public void Write(string key, string value) { PlayerPrefs.SetString(key, value); PlayerPrefs.Save(); }
    }
}
