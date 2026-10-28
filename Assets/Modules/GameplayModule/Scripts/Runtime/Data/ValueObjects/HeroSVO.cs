using System;
using System.Collections.Generic;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The party's progress as it is saved.</summary>
    [Serializable]
    public class HeroSVO
    {
        public int Level = 1;
        public long Exp;
        public long Gold;
        public int Kills;

        public int Floor = 1;
        public int FloorKills;

        /// <summary>Level-up choices earned and not yet answered.</summary>
        public int PendingUpgrades;

        public List<PartyRole> Recruited = new();
        public List<UpgradeSVO> Upgrades = new();

        /// <summary>When the progress was last saved, in UTC ticks - what the time away is measured from.</summary>
        public long LastSavedTicks;

        // Everything below survives a Reawakening.

        /// <summary>Levels bought in the shop, by item id.</summary>
        public List<UpgradeSVO> ShopItems = new();

        public List<ShadowSVO> Shadows = new();

        public int Essence;
        public int Reawakenings;
        public int BestFloor = 1;
    }
}
