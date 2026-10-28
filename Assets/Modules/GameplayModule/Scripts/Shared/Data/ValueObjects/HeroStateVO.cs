using System;
using System.Collections.Generic;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>
    /// What the party looks like to anyone outside the gameplay module - a snapshot taken by the
    /// Model, so a screen draws it without knowing how any of it is derived. Level, rank, EXP and
    /// gold belong to the party as a whole; HP belongs to each member.
    /// </summary>
    [Serializable]
    public class HeroStateVO
    {
        public int Level;
        public HeroRank Rank;
        public string RankTitle;
        public long Exp;
        public long ExpToNextLevel;
        public long Gold;
        public int Kills;
        public int Attack;
        public float CritChance;

        public int Floor;
        public int FloorKills;
        public int FloorKillsToBoss;
        public List<PartyMemberStateVO> Members = new();

        /// <summary>The shop in the config's order; a purchase names an item by its index here.</summary>
        public List<ShopItemStateVO> Shop = new();

        /// <summary>The body of every shadow soldier raised, oldest first.</summary>
        public List<EnemyLook> Shadows = new();

        /// <summary>The share of attack the shadow army adds - 0.12 is +12%.</summary>
        public float ShadowAttackBonus;

        public int Essence;
        public int Reawakenings;

        /// <summary>The share Essence adds to attack, HP, gold and EXP.</summary>
        public float EssenceBonus;

        public bool CanReawaken;
        public int ReawakenMinFloor;
        public int EssenceOnReawaken;
        public int BestFloor;
    }
}
