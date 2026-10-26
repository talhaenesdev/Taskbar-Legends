using System;
using Modules.GameplayModule.Shared.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>One kind of creature, at level 1. It scales with the level it is spawned at.</summary>
    [Serializable]
    public class EnemyCVO
    {
        public string Name = "Goblin";
        public EnemyLook Look = EnemyLook.Goblin;
        public float Scale = 1f;
        public int MinHeroLevel = 1;
        public int BaseHp = 40;
        public int BaseAttack = 5;
        public int BaseExp = 12;
        public int BaseGold = 4;
        public Color Tint = Color.green;
    }
}
