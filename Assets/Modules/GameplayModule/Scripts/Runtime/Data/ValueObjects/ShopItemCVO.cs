using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>
    /// A permanent improvement bought with gold. Each level adds Amount to its kind for the whole
    /// party, and unlike a level-up stack it survives a Reawakening.
    /// </summary>
    [Serializable]
    public class ShopItemCVO
    {
        /// <summary>The key a purchase is saved under; never change it once players have bought it.</summary>
        public string Id = "shop_attack";

        public UpgradeKind Kind = UpgradeKind.Attack;

        /// <summary>Per level: a share (0.05 is +5%), read the same way as a level-up stack of the same kind.</summary>
        public float Amount = 0.05f;

        public long BaseCost = 60;

        /// <summary>Each level costs this many times the one before.</summary>
        public float CostGrowth = 1.3f;

        /// <summary>Zero for no limit.</summary>
        public int MaxLevel = 50;

        public string Title = "Sharpened Blades";
        public string Description = "+5% party attack";
    }
}
