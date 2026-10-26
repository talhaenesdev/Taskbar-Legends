namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The hero as one record: what the config says of it, what the save holds, its rank and its HP in play.</summary>
    internal class HeroVO
    {
        public HeroCVO Config;
        public HeroSVO Save;
        public RankCVO Rank;
        public int Hp;
    }
}
