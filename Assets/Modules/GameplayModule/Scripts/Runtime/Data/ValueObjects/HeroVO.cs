using System.Collections.Generic;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The party as one record: its growth config, its save, its rank and its members in play.</summary>
    internal class HeroVO
    {
        public HeroCVO Config;
        public HeroSVO Save;
        public RankCVO Rank;
        public List<PartyMemberVO> Members = new();
    }
}
