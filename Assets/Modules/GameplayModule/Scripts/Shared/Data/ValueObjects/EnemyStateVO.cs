using System;
using Modules.GameplayModule.Shared.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>The creature the party is fighting, as a screen draws it.</summary>
    [Serializable]
    public class EnemyStateVO
    {
        /// <summary>Counts up with every creature spawned, so a reader can tell a new creature from the old one hit again.</summary>
        public int SpawnId;

        public string Name;
        public int Level;
        public int Hp;
        public int MaxHp;
        public EnemyLook Look;
        public float Scale = 1f;
        public Color Tint;
    }
}
