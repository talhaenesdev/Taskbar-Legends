using System.Collections.Generic;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>
    /// The party as one record: its growth config, its save, its rank, the members it has
    /// recruited in formation order, and every upgrade the config lists with its stacks.
    /// </summary>
    internal class HeroVO
    {
        public HeroCVO Config;
        public HeroSVO Save;
        public RankCVO Rank;
        public List<PartyMemberVO> Members = new();
        public Dictionary<string, UpgradeVO> Upgrades = new();
    }
}
