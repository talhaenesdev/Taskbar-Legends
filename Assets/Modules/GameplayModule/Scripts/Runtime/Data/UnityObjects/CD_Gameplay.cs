using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    /// <summary>
    /// Everything the idle battle is balanced by: the party's growth and its heroes, the level-up
    /// choices, the floors and their creatures, and the ranks.
    /// </summary>
    [CreateAssetMenu(fileName = "CD_Gameplay", menuName = "FlowIoC/GameplayModule/Data/CD_Gameplay")]
    public class CD_Gameplay : ScriptableObject
    {
        /// <summary>Seconds between two turns, before Speed upgrades. One party member acts per turn, in order.</summary>
        [Header("Turns")] public float TurnSeconds = 0.45f;

        /// <summary>The shortest a turn may get, however many Speed upgrades are stacked.</summary>
        public float MinTurnSeconds = 0.2f;

        /// <summary>
        /// Seconds a new creature spends walking in before anybody strikes - turned into turns at
        /// spawn, so a party made faster by Speed upgrades still waits for the creature to arrive.
        /// </summary>
        public float EnemyArrivalSeconds = 1.9f;

        /// <summary>The creature strikes once every this many turns.</summary>
        public int EnemyAttackEveryTurns = 2;

        /// <summary>The share of max HP a fallen member comes back with when the creature that felled it dies.</summary>
        [Range(0f, 1f)] public float ReviveShareAfterKill = 0.35f;

        /// <summary>A healer heals instead of striking while somebody is below this share of their HP.</summary>
        [Range(0f, 1f)] public float HealBelowShare = 0.7f;

        [Header("Party")]
        public HeroCVO Hero = new();

        /// <summary>Front to back. Members who are not recruited yet keep their place for when they join.</summary>
        public List<PartyMemberCVO> Party = new();

        [Header("Level-up choices")]
        [Min(1)] public int OfferOptionCount = 3;
        public float AutoPickSeconds = 12f;
        public List<UpgradeCVO> Upgrades = new();

        /// <summary>Kills on a floor before its boss; the boss is the kill after them.</summary>
        [Header("Floors")] [Min(1)] public int FloorKillsToBoss = 10;

        /// <summary>How many levels a creature gains with every floor.</summary>
        public int EnemyLevelsPerFloor = 2;

        /// <summary>How much stronger a creature gets per level, as a share of its level 1 values.</summary>
        public float EnemyGrowthPerLevel = 0.22f;

        [Range(0f, 1f)] public float EliteChance = 0.12f;
        public float EliteHpMultiplier = 2f;
        public float EliteAttackMultiplier = 1.4f;
        public float EliteRewardMultiplier = 2.5f;

        public float BossHpMultiplier = 5f;
        public float BossAttackMultiplier = 1.6f;
        public float BossRewardMultiplier = 6f;
        public float BossScaleMultiplier = 1.35f;

        public List<EnemyCVO> Enemies = new();

        /// <summary>Ordered from the lowest MinLevel up.</summary>
        [Header("Ranks")] public List<RankCVO> Ranks = new();
    }
}
