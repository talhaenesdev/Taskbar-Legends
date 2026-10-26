using System;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>How the hero grows: the level 1 values and what each level adds.</summary>
    [Serializable]
    public class HeroCVO
    {
        public int BaseMaxHp = 120;
        public int MaxHpPerLevel = 18;
        public int BaseAttack = 12;
        public int AttackPerLevel = 3;

        [Range(0f, 1f)] public float CritChance = 0.12f;
        public float CritMultiplier = 2f;

        /// <summary>The share of max HP regained every turn.</summary>
        [Range(0f, 0.2f)] public float RegenPerTurn = 0.02f;

        public int BaseExpToLevel = 50;
        public float ExpGrowth = 1.18f;
    }
}
