using System.Collections.Generic;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    /// <summary>
    /// The party: its shared level, experience, gold, rank, floor and upgrades, and each recruited
    /// member's HP - with the rules that keep them valid: HP inside its bounds, experience carried
    /// over a level, the rank that follows from the level, every stat that follows from the level
    /// and the upgrades stacked on it.
    /// </summary>
    internal interface IHeroModel
    {
        CD_Gameplay Config { get; }

        int Level { get; }
        RankCVO Rank { get; }

        /// <summary>The recruited members, front to back.</summary>
        IReadOnlyList<PartyMemberVO> Members { get; }

        /// <summary>Every upgrade the config lists, with its stacks, by id.</summary>
        IReadOnlyCollection<UpgradeVO> Upgrades { get; }

        /// <summary>True when every member is down.</summary>
        bool IsDefeated { get; }

        float TurnSeconds { get; }
        float CritMultiplier { get; }

        int MaxHpOf(PartyMemberVO member);
        int AttackOf(PartyMemberVO member);
        int RegenOf(PartyMemberVO member);
        float CritChanceOf(PartyMemberVO member);

        /// <summary>The first member still standing, front to back; null when all are down.</summary>
        PartyMemberVO FrontMember();

        /// <summary>The standing member with the smallest share of their HP left; null when all are down.</summary>
        PartyMemberVO MostHurtMember();

        float HpShareOf(PartyMemberVO member);

        void Damage(PartyMemberVO member, int amount);
        void Heal(PartyMemberVO member, int amount);
        void RestoreAll();

        /// <summary>Brings every fallen member back with that share of their max HP.</summary>
        void ReviveFallen(float share);

        /// <summary>Gives the reward with the Gold and Exp upgrades applied; the reward actually given is handed back.</summary>
        (int exp, int gold) GrantRewards(int exp, int gold);
        void CountKill();

        /// <summary>Spends enough experience for one level when there is enough. True when a level was gained.</summary>
        bool TryLevelUp();

        /// <summary>Moves the rank to the highest one the level has reached. True when it changed.</summary>
        bool RefreshRank();

        int Floor { get; }
        int FloorKills { get; }

        /// <summary>True when the next creature on this floor is its boss.</summary>
        bool IsBossDue { get; }

        void CountFloorKill();
        void AdvanceFloor();
        void ResetFloorProgress();

        /// <summary>Level-up choices earned and not yet answered - the open one included.</summary>
        int PendingUpgrades { get; }
        void AddPendingUpgrade();
        void TakePendingUpgrade();

        /// <summary>The heroes the level has unlocked who have not joined yet, in formation order.</summary>
        IEnumerable<PartyMemberCVO> RecruitableMembers();

        /// <summary>True while the upgrade may take another stack.</summary>
        bool CanStack(UpgradeVO upgrade);

        void Recruit(PartyMemberCVO member);
        void Stack(UpgradeCVO upgrade);

        /// <summary>Seconds between the last save and this start; zero once the time away has been rewarded.</summary>
        double SecondsAway { get; }
        void ClearSecondsAway();
        void CountKills(int count);

        /// <summary>The shop item at that index of the config's list; null when there is none.</summary>
        ShopItemVO FindShopItem(int index);
        bool IsMaxed(ShopItemVO item);
        long CostOf(ShopItemVO item);

        /// <summary>Pays for the next level of the item. False when it is maxed or the gold is short.</summary>
        bool TryBuy(ShopItemVO item);

        int ShadowCount { get; }
        bool HasShadowRoom { get; }
        float ShadowAttackBonus { get; }
        void AddShadow(string name, Shared.Enums.EnemyLook look);

        int Essence { get; }
        float EssenceBonus { get; }

        /// <summary>True once the party has reached the floor a Reawakening asks for.</summary>
        bool CanReawaken { get; }
        int EssenceOnReawaken { get; }

        /// <summary>Starts the climb over for Essence; the shop, the shadows and the Essence are kept. The Essence gained is handed back.</summary>
        int Reawaken();

        HeroStateVO GetState();
        void Save();
    }
}
