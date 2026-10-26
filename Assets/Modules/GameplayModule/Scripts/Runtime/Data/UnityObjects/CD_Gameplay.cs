using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    /// <summary>Everything the idle battle is balanced by: the party's growth, the creatures, and the ranks.</summary>
    [CreateAssetMenu(fileName = "CD_Gameplay", menuName = "FlowIoC/GameplayModule/Data/CD_Gameplay")]
    public class CD_Gameplay : ScriptableObject
    {
        /// <summary>Seconds between two turns. One party member acts per turn, in order.</summary>
        public float TurnSeconds = 0.45f;

        /// <summary>Turns a new creature spends walking in before anybody strikes.</summary>
        public int EnemyArrivalTurns = 3;

        /// <summary>The creature strikes once every this many turns.</summary>
        public int EnemyAttackEveryTurns = 2;

        /// <summary>The share of max HP a fallen member comes back with when the creature that felled it dies.</summary>
        [Range(0f, 1f)] public float ReviveShareAfterKill = 0.35f;

        /// <summary>How much stronger a creature gets per level, as a share of its level 1 values.</summary>
        public float EnemyGrowthPerLevel = 0.22f;

        public HeroCVO Hero = new();

        /// <summary>Front to back: the first member is the one the creature strikes while it stands.</summary>
        public List<PartyMemberCVO> Party = new();

        public List<EnemyCVO> Enemies = new();

        /// <summary>Ordered from the lowest MinLevel up.</summary>
        public List<RankCVO> Ranks = new();
    }
}
