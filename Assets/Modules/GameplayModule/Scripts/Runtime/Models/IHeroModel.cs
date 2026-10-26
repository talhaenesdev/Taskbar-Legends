using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    /// <summary>
    /// The hero: level, experience, gold and HP, and the rules that keep them valid - HP inside
    /// its bounds, experience carried over a level, the rank that follows from the level.
    /// </summary>
    internal interface IHeroModel
    {
        CD_Gameplay Config { get; }

        int Level { get; }
        int Hp { get; }
        int MaxHp { get; }
        int Attack { get; }
        int RegenPerTurn { get; }
        RankCVO Rank { get; }
        bool IsDead { get; }

        void Damage(int amount);
        void Heal(int amount);
        void RestoreFullHp();

        void GrantRewards(int exp, int gold);
        void CountKill();

        /// <summary>Spends enough experience for one level when there is enough. True when a level was gained.</summary>
        bool TryLevelUp();

        /// <summary>Moves the rank to the highest one the level has reached. True when it changed.</summary>
        bool RefreshRank();

        HeroStateVO GetState();
        void Save();
    }
}
