using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The level a rank is reached at, and what the System calls a hunter of it.</summary>
    [Serializable]
    public class RankCVO
    {
        public HeroRank Rank;
        public int MinLevel = 1;
        public string Title = "E-Rank Hunter";
    }
}
