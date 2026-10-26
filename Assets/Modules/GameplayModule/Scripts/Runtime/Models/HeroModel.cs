using System.Collections.Generic;
using FlowIoC.BaseModule.Adapters;
using FlowIoC.BaseModule.Constructables;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.RootsContexts;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Models
{
    /// <summary>
    /// PostConstruct takes CD_Gameplay off the Root's adapter and the saved progress out of
    /// PlayerPrefs, and builds one record for the party with one entry per member the config
    /// lists. Everything derived from the level - max HP, attack, the experience a level costs, the
    /// rank - is derived here and nowhere else.
    /// </summary>
    internal class HeroModel : IHeroModel, IConstructable
    {
        [Inject(nameof(GameplaySystemContext))]
        private GameObject _root { get; set; }

        private HeroVO _hero;

        public bool IsPostConstructed { get; set; }
        public bool IsDeconstructed { get; set; }

        public CD_Gameplay Config { get; private set; }

        public int Level => _hero.Save.Level;
        public RankCVO Rank => _hero.Rank;
        public IReadOnlyList<PartyMemberVO> Members => _hero.Members;

        public bool IsDefeated => FrontMember() == null;

        private int PartyMaxHp => _hero.Config.BaseMaxHp + _hero.Config.MaxHpPerLevel * (Level - 1);
        private int PartyAttack => _hero.Config.BaseAttack + _hero.Config.AttackPerLevel * (Level - 1);

        private long ExpToNextLevel =>
            Mathf.RoundToInt(_hero.Config.BaseExpToLevel * Mathf.Pow(_hero.Config.ExpGrowth, Level - 1));

        public void PostConstruct()
        {
            if (_root == null) return;

            var adapter = _root.GetComponent<RootAdapter>();
            if (adapter == null)
            {
                FlowLogger.LogError("GameplaySystemRoot has no RootAdapter, so CD_Gameplay cannot be read.", _root);
                return;
            }

            Config = adapter.GetScriptable<CD_Gameplay>();
            if (Config == null)
            {
                FlowLogger.LogError("GameplaySystemRoot's adapter files no CD_Gameplay, so the party cannot be built.", _root);
                return;
            }

            if (Config.Party.Count == 0)
                FlowLogger.LogError("CD_Gameplay lists no party member, so nobody can fight.", Config);

            _hero = new HeroVO { Config = Config.Hero, Save = LoadSave() };
            _hero.Rank = FindRank(Level);

            foreach (PartyMemberCVO member in Config.Party)
                _hero.Members.Add(new PartyMemberVO { Config = member });

            RestoreAll();
        }

        public void Deconstruct()
        {
            _hero = null;
            Config = null;
        }

        public int MaxHpOf(PartyMemberVO member) => Mathf.Max(1, Mathf.RoundToInt(PartyMaxHp * member.Config.HpShare));
        public int AttackOf(PartyMemberVO member) => Mathf.Max(1, Mathf.RoundToInt(PartyAttack * member.Config.AttackShare));
        public int RegenOf(PartyMemberVO member) => Mathf.Max(1, Mathf.RoundToInt(MaxHpOf(member) * _hero.Config.RegenPerTurn));

        public PartyMemberVO FrontMember()
        {
            foreach (PartyMemberVO member in _hero.Members)
            {
                if (!member.IsDown)
                    return member;
            }

            return null;
        }

        public void Damage(PartyMemberVO member, int amount) => member.Hp = Mathf.Max(0, member.Hp - Mathf.Max(0, amount));

        public void Heal(PartyMemberVO member, int amount)
        {
            // A member who is down stays down until something revives it.
            if (member.IsDown) return;
            member.Hp = Mathf.Min(MaxHpOf(member), member.Hp + Mathf.Max(0, amount));
        }

        public void RestoreAll()
        {
            foreach (PartyMemberVO member in _hero.Members)
                member.Hp = MaxHpOf(member);
        }

        public void ReviveFallen(float share)
        {
            foreach (PartyMemberVO member in _hero.Members)
            {
                if (member.IsDown)
                    member.Hp = Mathf.Max(1, Mathf.RoundToInt(MaxHpOf(member) * share));
            }
        }

        public void GrantRewards(int exp, int gold)
        {
            _hero.Save.Exp += Mathf.Max(0, exp);
            _hero.Save.Gold += Mathf.Max(0, gold);
        }

        public void CountKill() => _hero.Save.Kills++;

        public bool TryLevelUp()
        {
            long cost = ExpToNextLevel;
            if (_hero.Save.Exp < cost)
                return false;

            // The HP a level adds is granted with it to everyone standing, so a level gained mid-fight is felt at once.
            var before = new int[_hero.Members.Count];
            for (int i = 0; i < before.Length; i++)
                before[i] = MaxHpOf(_hero.Members[i]);

            _hero.Save.Exp -= cost;
            _hero.Save.Level++;

            for (int i = 0; i < before.Length; i++)
            {
                PartyMemberVO member = _hero.Members[i];
                if (!member.IsDown)
                    member.Hp += MaxHpOf(member) - before[i];
            }

            return true;
        }

        public bool RefreshRank()
        {
            RankCVO rank = FindRank(Level);
            if (rank == _hero.Rank)
                return false;

            _hero.Rank = rank;
            return true;
        }

        public HeroStateVO GetState()
        {
            var state = new HeroStateVO
            {
                Level = Level,
                Rank = _hero.Rank?.Rank ?? default,
                RankTitle = _hero.Rank?.Title ?? string.Empty,
                Exp = _hero.Save.Exp,
                ExpToNextLevel = ExpToNextLevel,
                Gold = _hero.Save.Gold,
                Kills = _hero.Save.Kills,
                Attack = PartyAttack,
                CritChance = _hero.Config.CritChance,
            };

            foreach (PartyMemberVO member in _hero.Members)
            {
                state.Members.Add(new PartyMemberStateVO
                {
                    Role = member.Config.Role,
                    Name = member.Config.Name,
                    Hp = member.Hp,
                    MaxHp = MaxHpOf(member),
                });
            }

            return state;
        }

        public void Save()
        {
            PlayerPrefs.SetString(GameplayConstants.SAVE_KEY, JsonUtility.ToJson(_hero.Save));
            PlayerPrefs.Save();
        }

        private static HeroSVO LoadSave()
        {
            string json = PlayerPrefs.GetString(GameplayConstants.SAVE_KEY, string.Empty);
            HeroSVO save = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<HeroSVO>(json);
            save ??= new HeroSVO();

            // Repaired once on the way in, so no read has to.
            save.Level = Mathf.Max(1, save.Level);
            save.Exp = System.Math.Max(0, save.Exp);
            save.Gold = System.Math.Max(0, save.Gold);
            save.Kills = Mathf.Max(0, save.Kills);
            return save;
        }

        /// <summary>The highest rank whose MinLevel the level has reached; the first rank when none has been.</summary>
        private RankCVO FindRank(int level)
        {
            RankCVO found = null;
            foreach (RankCVO rank in Config.Ranks)
            {
                if (rank.MinLevel <= level && (found == null || rank.MinLevel >= found.MinLevel))
                    found = rank;
            }

            return found ?? (Config.Ranks.Count > 0 ? Config.Ranks[0] : null);
        }
    }
}
