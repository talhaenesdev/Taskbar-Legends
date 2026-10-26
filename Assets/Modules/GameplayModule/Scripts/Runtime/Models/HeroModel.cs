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
    /// PlayerPrefs, and puts both on one record. Everything derived from the level - max HP,
    /// attack, the experience a level costs, the rank - is derived here and nowhere else.
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
        public int Hp => _hero.Hp;
        public int MaxHp => _hero.Config.BaseMaxHp + _hero.Config.MaxHpPerLevel * (Level - 1);
        public int Attack => _hero.Config.BaseAttack + _hero.Config.AttackPerLevel * (Level - 1);
        public int RegenPerTurn => Mathf.Max(1, Mathf.RoundToInt(MaxHp * _hero.Config.RegenPerTurn));
        public RankCVO Rank => _hero.Rank;
        public bool IsDead => _hero.Hp <= 0;

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
                FlowLogger.LogError("GameplaySystemRoot's adapter files no CD_Gameplay, so the hero cannot be built.", _root);
                return;
            }

            _hero = new HeroVO { Config = Config.Hero, Save = LoadSave() };
            _hero.Rank = FindRank(Level);
            _hero.Hp = MaxHp;
        }

        public void Deconstruct()
        {
            _hero = null;
            Config = null;
        }

        public void Damage(int amount) => _hero.Hp = Mathf.Max(0, _hero.Hp - Mathf.Max(0, amount));

        public void Heal(int amount) => _hero.Hp = Mathf.Min(MaxHp, _hero.Hp + Mathf.Max(0, amount));

        public void RestoreFullHp() => _hero.Hp = MaxHp;

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

            // The HP a level adds is granted with it, so a level gained mid-fight is felt at once.
            int maxHpBefore = MaxHp;
            _hero.Save.Exp -= cost;
            _hero.Save.Level++;
            _hero.Hp += MaxHp - maxHpBefore;
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

        public HeroStateVO GetState() => new()
        {
            Level = Level,
            Rank = _hero.Rank?.Rank ?? default,
            RankTitle = _hero.Rank?.Title ?? string.Empty,
            Exp = _hero.Save.Exp,
            ExpToNextLevel = ExpToNextLevel,
            Gold = _hero.Save.Gold,
            Kills = _hero.Save.Kills,
            Hp = _hero.Hp,
            MaxHp = MaxHp,
            Attack = Attack,
            CritChance = _hero.Config.CritChance,
        };

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
