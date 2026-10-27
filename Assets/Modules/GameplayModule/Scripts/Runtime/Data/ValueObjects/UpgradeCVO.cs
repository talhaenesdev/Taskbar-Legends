using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>One improvement a level-up may offer. Each pick adds one stack of Amount.</summary>
    [Serializable]
    public class UpgradeCVO
    {
        /// <summary>The key a pick is saved under; never change it once players have picked it.</summary>
        public string Id = "attack_all";

        public UpgradeKind Kind = UpgradeKind.Attack;

        /// <summary>The member it improves - only for Attack and Health, and only when AllRoles is off.</summary>
        public PartyRole Role;
        public bool AllRoles = true;

        /// <summary>Per stack: a share (0.1 is +10%) for every kind but Speed, where it is the share of turn time taken off.</summary>
        public float Amount = 0.1f;

        /// <summary>Zero for no limit.</summary>
        public int MaxStacks;

        /// <summary>How often it comes up against the others.</summary>
        public float Weight = 1f;

        public string Title = "Sharpened Blades";
        public string Description = "All heroes +10% attack";
    }
}
