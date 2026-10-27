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
    }
}
