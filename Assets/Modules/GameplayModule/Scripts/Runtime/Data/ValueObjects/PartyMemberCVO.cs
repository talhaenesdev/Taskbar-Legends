using System;
using Modules.GameplayModule.Shared.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>One hero the party can have: its share of the party's HP and attack, and what sets it apart.</summary>
    [Serializable]
    public class PartyMemberCVO
    {
        public PartyRole Role;
        public string Name = "Warrior";
        [TextArea] public string Description = "A sturdy fighter who holds the front.";

        /// <summary>In the party from the first run. Everyone else joins through a level-up choice.</summary>
        public bool StartsInParty;

        /// <summary>The party level from which this hero may be offered.</summary>
        public int UnlockLevel = 1;

        public float HpShare = 1f;
        public float AttackShare = 1f;

        /// <summary>Added to the party's critical chance on this hero's blows.</summary>
        [Range(0f, 1f)] public float CritBonus;

        /// <summary>Blows per turn.</summary>
        [Min(1)] public int Hits = 1;

        /// <summary>
        /// Above zero, the hero heals instead of striking whenever somebody is hurt: this share of
        /// its attack goes to the member with the least HP left.
        /// </summary>
        public float HealPower;
    }
}
