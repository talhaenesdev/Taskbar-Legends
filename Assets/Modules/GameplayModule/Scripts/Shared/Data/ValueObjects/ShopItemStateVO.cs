using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>One shop item as a screen shows it: what it does, how far it has been bought, and the next level's price.</summary>
    [Serializable]
    public class ShopItemStateVO
    {
        public UpgradeKind Kind;
        public string Title;
        public string Description;
        public int Level;

        /// <summary>Zero for no limit.</summary>
        public int MaxLevel;

        public long Cost;
        public bool IsMaxed;
        public bool CanAfford;
    }
}
