using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>
    /// What the hero looks like to anyone outside the gameplay module - a snapshot taken by the
    /// Model, so a screen draws it without knowing how any of it is derived.
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
        public int Hp;
        public int MaxHp;
        public int Attack;
        public float CritChance;
    }
}
