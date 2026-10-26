using System;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The hero's progress as it is saved.</summary>
    [Serializable]
    public class HeroSVO
    {
        public int Level = 1;
        public long Exp;
        public long Gold;
        public int Kills;
    }
}
