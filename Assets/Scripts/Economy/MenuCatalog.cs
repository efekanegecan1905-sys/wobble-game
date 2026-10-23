using System;
using UnityEngine;

namespace Wobble.Economy
{
    [CreateAssetMenu(menuName = "WOBBLE/Menu Catalog")]
    public sealed class MenuCatalog : ScriptableObject
    {
        public ProductDefinition[] products;
        public ThemeDefinition[] themes;
        public RewardDefinition[] dailyRewards;
    }

    public enum ProductSection { Lives, Gems, Special }
    [Serializable] public sealed class ProductDefinition
    {
        public string id;
        public string nameKey;
        public ProductSection section;
        public string placeholderPrice;
        public int gems;
        public int lives;
        public string futureSkinId;
        public bool removesAds;
    }

    [Serializable] public sealed class ThemeDefinition
    {
        public string id;
        public string nameKey;
        public Material previewMaterial;
        public Color previewTint;
        public int gemPrice;
        public bool available;
    }

    [Serializable] public struct RewardDefinition { public int gems; public int lives; }
}
