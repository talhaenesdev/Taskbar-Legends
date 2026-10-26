using System.Collections.Generic;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    /// <summary>
    /// The party: its shared level, experience, gold and rank, and each member's HP - with the
    /// rules that keep them valid: HP inside its bounds, experience carried over a level, the rank
    /// that follows from the level, every stat that follows from both.
    /// </summary>
    internal interface IHeroModel
    {
        CD_Gameplay Config { get; }

        int Level { get; }
        RankCVO Rank { get; }
        IReadOnlyList<PartyMemberVO> Members { get; }

        /// <summary>True when every member is down.</summary>
        bool IsDefeated { get; }

        int MaxHpOf(PartyMemberVO member);
        int AttackOf(PartyMemberVO member);
        int RegenOf(PartyMemberVO member);

        /// <summary>The first member still standing, front to back; null when all are down.</summary>
        PartyMemberVO FrontMember();

        void Damage(PartyMemberVO member, int amount);
        void Heal(PartyMemberVO member, int amount);
        void RestoreAll();

        /// <summary>Brings every fallen member back with that share of their max HP.</summary>
        void ReviveFallen(float share);

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
