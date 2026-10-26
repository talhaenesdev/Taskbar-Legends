using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    /// <summary>Everything the idle battle is balanced by: the hero's growth, the creatures, and the ranks.</summary>
    [CreateAssetMenu(fileName = "CD_Gameplay", menuName = "FlowIoC/GameplayModule/Data/CD_Gameplay")]
    public class CD_Gameplay : ScriptableObject
    {
        /// <summary>Seconds between two turns of combat.</summary>
        public float TurnSeconds = 0.8f;

        /// <summary>How much stronger a creature gets per level, as a share of its level 1 values.</summary>
        public float EnemyGrowthPerLevel = 0.22f;

        public HeroCVO Hero = new();
        public List<EnemyCVO> Enemies = new();

        /// <summary>Ordered from the lowest MinLevel up.</summary>
        public List<RankCVO> Ranks = new();
    }
}
