using System;
using UnityEngine;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>The creature the hero is fighting, as a screen draws it.</summary>
    [Serializable]
    public class EnemyStateVO
    {
        public string Name;
        public int Level;
        public int Hp;
        public int MaxHp;
        public Color Tint;
    }
}
