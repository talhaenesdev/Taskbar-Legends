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
using Modules.GameplayModule.Shared.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Models
{
    /// <summary>
    /// PostConstruct takes CD_Gameplay off the Root's adapter and the saved progress out of
    /// PlayerPrefs. The config decides which heroes and upgrades exist; the save attaches to them -
    /// which heroes have joined, how many stacks each upgrade has. Everything derived from the level
    /// and the stacks - max HP, attack, crit, turn time, the experience a level costs, the rank - is
    /// derived here and nowhere else.
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
        public IReadOnlyCollection<UpgradeVO> Upgrades => _hero.Upgrades.Values;

        public bool IsDefeated => FrontMember() == null;

        public float TurnSeconds =>
            Mathf.Max(Config.MinTurnSeconds, Config.TurnSeconds * (1f - Stacked(UpgradeKind.Speed, null)));

        public float CritMultiplier => _hero.Config.CritMultiplier + Stacked(UpgradeKind.CritDamage, null);

        public int Floor => _hero.Save.Floor;
        public int FloorKills => _hero.Save.FloorKills;
        public bool IsBossDue => _hero.Save.FloorKills >= Config.FloorKillsToBoss;
        public int PendingUpgrades => _hero.Save.PendingUpgrades;

        public double SecondsAway { get; private set; }

        public int ShadowCount => _hero.Save.Shadows.Count;
        public bool HasShadowRoom => ShadowCount < Config.MaxShadows;
        public float ShadowAttackBonus => ShadowCount * Config.ShadowAttackBonus;

        public int Essence => _hero.Save.Essence;
        public float EssenceBonus => Essence * Config.EssenceBonus;
        public bool CanReawaken => Floor >= Config.ReawakenMinFloor;
        public int EssenceOnReawaken => Mathf.Max(1, Mathf.RoundToInt(Floor * Config.EssencePerFloor));

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

            _hero = new HeroVO { Config = Config.Hero, Save = LoadSave() };
            _hero.Rank = FindRank(Level);

            long savedTicks = _hero.Save.LastSavedTicks;
            SecondsAway = savedTicks > 0 ? System.Math.Max(0d, (System.DateTime.UtcNow.Ticks - savedTicks) / (double) System.TimeSpan.TicksPerSecond) : 0d;

            FillUpgrades();
            FillShop();
            FillMembers();
            RestoreAll();
        }

        public void Deconstruct()
        {
            _hero = null;
            Config = null;
        }

        public int MaxHpOf(PartyMemberVO member) =>
            Mathf.Max(1, Mathf.RoundToInt(PartyMaxHp * member.Config.HpShare * (1f + Stacked(UpgradeKind.Health, member))));

        public int AttackOf(PartyMemberVO member) =>
            Mathf.Max(1, Mathf.RoundToInt(PartyAttack * member.Config.AttackShare * (1f + Stacked(UpgradeKind.Attack, member))));

        public int RegenOf(PartyMemberVO member) =>
            Mathf.Max(1, Mathf.RoundToInt(MaxHpOf(member) * (_hero.Config.RegenPerTurn + Stacked(UpgradeKind.Regen, null))));

        public float CritChanceOf(PartyMemberVO member) =>
            Mathf.Clamp01(_hero.Config.CritChance + member.Config.CritBonus + Stacked(UpgradeKind.CritChance, null));

        public PartyMemberVO FrontMember()
        {
            foreach (PartyMemberVO member in _hero.Members)
            {
                if (!member.IsDown)
                    return member;
            }

            return null;
        }

        public PartyMemberVO MostHurtMember()
        {
            PartyMemberVO found = null;
            foreach (PartyMemberVO member in _hero.Members)
            {
                if (!member.IsDown && (found == null || HpShareOf(member) < HpShareOf(found)))
                    found = member;
            }

            return found;
        }

        public float HpShareOf(PartyMemberVO member) => (float) member.Hp / MaxHpOf(member);

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

        public (int exp, int gold) GrantRewards(int exp, int gold)
        {
            exp = Mathf.Max(0, Mathf.RoundToInt(exp * (1f + Stacked(UpgradeKind.Exp, null))));
            gold = Mathf.Max(0, Mathf.RoundToInt(gold * (1f + Stacked(UpgradeKind.Gold, null))));

            _hero.Save.Exp += exp;
            _hero.Save.Gold += gold;
            return (exp, gold);
        }

        public void CountKill() => _hero.Save.Kills++;

        public void CountKills(int count) => _hero.Save.Kills += Mathf.Max(0, count);

        public void ClearSecondsAway() => SecondsAway = 0d;

        public ShopItemVO FindShopItem(int index) => index >= 0 && index < _hero.Shop.Count ? _hero.Shop[index] : null;

        public bool IsMaxed(ShopItemVO item) => item.Config.MaxLevel > 0 && item.Save.Count >= item.Config.MaxLevel;

        public long CostOf(ShopItemVO item) =>
            (long) System.Math.Round(item.Config.BaseCost * System.Math.Pow(item.Config.CostGrowth, item.Save.Count));

        public bool TryBuy(ShopItemVO item)
        {
            long cost = CostOf(item);
            if (IsMaxed(item) || _hero.Save.Gold < cost)
                return false;

            // A level bought is felt at once, like a level gained.
            int[] before = MaxHps();
            _hero.Save.Gold -= cost;
            item.Save.Count++;
            GrantMaxHpGain(before);
            return true;
        }

        public void AddShadow(string name, EnemyLook look)
        {
            if (!HasShadowRoom)
                return;

            int[] before = MaxHps();
            _hero.Save.Shadows.Add(new ShadowSVO { Name = name, Look = look });
            GrantMaxHpGain(before);
        }

        public int Reawaken()
        {
            int gained = EssenceOnReawaken;
            HeroSVO save = _hero.Save;

            save.Essence += gained;
            save.Reawakenings++;
            save.BestFloor = Mathf.Max(save.BestFloor, save.Floor);

            // The climb starts over; the shop, the shadows and the Essence carry on.
            save.Level = 1;
            save.Exp = 0;
            save.Gold = 0;
            save.Floor = 1;
            save.FloorKills = 0;
            save.PendingUpgrades = 0;
            save.Recruited.Clear();
            foreach (UpgradeVO upgrade in _hero.Upgrades.Values)
                upgrade.Save.Count = 0;

            _hero.Rank = FindRank(Level);
            _hero.Members.Clear();
            FillMembers();
            RestoreAll();
            return gained;
        }

        public bool TryLevelUp()
        {
            long cost = ExpToNextLevel;
            if (_hero.Save.Exp < cost)
                return false;

            // The HP a level adds is granted with it to everyone standing, so a level gained mid-fight is felt at once.
            int[] before = MaxHps();
            _hero.Save.Exp -= cost;
            _hero.Save.Level++;
            GrantMaxHpGain(before);
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

        public void CountFloorKill() => _hero.Save.FloorKills++;

        public void AdvanceFloor()
        {
            _hero.Save.Floor++;
            _hero.Save.FloorKills = 0;
            _hero.Save.BestFloor = Mathf.Max(_hero.Save.BestFloor, _hero.Save.Floor);
        }

        public void ResetFloorProgress() => _hero.Save.FloorKills = 0;

        public void AddPendingUpgrade() => _hero.Save.PendingUpgrades++;

        public void TakePendingUpgrade() => _hero.Save.PendingUpgrades = Mathf.Max(0, _hero.Save.PendingUpgrades - 1);

        public IEnumerable<PartyMemberCVO> RecruitableMembers()
        {
            foreach (PartyMemberCVO member in Config.Party)
            {
                if (member.UnlockLevel <= Level && !_hero.Save.Recruited.Contains(member.Role))
                    yield return member;
            }
        }

        public bool CanStack(UpgradeVO upgrade) => upgrade.Config.MaxStacks <= 0 || upgrade.Save.Count < upgrade.Config.MaxStacks;

        public void Recruit(PartyMemberCVO member)
        {
            if (_hero.Save.Recruited.Contains(member.Role))
                return;

            _hero.Save.Recruited.Add(member.Role);
            FillMembers();

            // A hero joins fresh, whatever state the others are in.
            foreach (PartyMemberVO joined in _hero.Members)
            {
                if (joined.Config == member)
                    joined.Hp = MaxHpOf(joined);
            }
        }

        public void Stack(UpgradeCVO upgrade)
        {
            if (!_hero.Upgrades.TryGetValue(upgrade.Id, out UpgradeVO record) || !CanStack(record))
                return;

            // A Health stack raises max HP; the HP it adds is granted at once, like a level's.
            int[] before = MaxHps();
            record.Save.Count++;
            GrantMaxHpGain(before);
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
                CritChance = _hero.Config.CritChance + Stacked(UpgradeKind.CritChance, null),
                Floor = Floor,
                FloorKills = FloorKills,
                FloorKillsToBoss = Config.FloorKillsToBoss,
                ShadowAttackBonus = ShadowAttackBonus,
                Essence = Essence,
                Reawakenings = _hero.Save.Reawakenings,
                EssenceBonus = EssenceBonus,
                CanReawaken = CanReawaken,
                ReawakenMinFloor = Config.ReawakenMinFloor,
                EssenceOnReawaken = EssenceOnReawaken,
                BestFloor = _hero.Save.BestFloor,
            };

            foreach (ShadowSVO shadow in _hero.Save.Shadows)
                state.Shadows.Add(shadow.Look);

            foreach (ShopItemVO item in _hero.Shop)
            {
                long cost = CostOf(item);
                bool isMaxed = IsMaxed(item);
                state.Shop.Add(new ShopItemStateVO
                {
                    Kind = item.Config.Kind,
                    Title = item.Config.Title,
                    Description = item.Config.Description,
                    Level = item.Save.Count,
                    MaxLevel = item.Config.MaxLevel,
                    Cost = cost,
                    IsMaxed = isMaxed,
                    CanAfford = !isMaxed && _hero.Save.Gold >= cost,
                });
            }

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
            _hero.Save.LastSavedTicks = System.DateTime.UtcNow.Ticks;
            PlayerPrefs.SetString(GameplayConstants.SAVE_KEY, JsonUtility.ToJson(_hero.Save));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// The sum of everything of one kind that reaches this member - or, with no member, the whole
        /// party: level-up stacks, shop levels, and for some kinds the shadow army and Essence.
        /// </summary>
        private float Stacked(UpgradeKind kind, PartyMemberVO member)
        {
            float total = 0f;

            foreach (ShopItemVO item in _hero.Shop)
            {
                if (item.Config.Kind == kind)
                    total += item.Config.Amount * item.Save.Count;
            }

            if (kind == UpgradeKind.Attack)
                total += ShadowAttackBonus;

            if (kind is UpgradeKind.Attack or UpgradeKind.Health or UpgradeKind.Gold or UpgradeKind.Exp)
                total += EssenceBonus;

            foreach (UpgradeVO upgrade in _hero.Upgrades.Values)
            {
                UpgradeCVO config = upgrade.Config;
                if (config.Kind != kind || upgrade.Save.Count == 0)
                    continue;

                if (config.AllRoles || (member != null && config.Role == member.Config.Role))
                    total += config.Amount * upgrade.Save.Count;
            }

            return total;
        }

        private int[] MaxHps()
        {
            var maxHps = new int[_hero.Members.Count];
            for (int i = 0; i < maxHps.Length; i++)
                maxHps[i] = MaxHpOf(_hero.Members[i]);
            return maxHps;
        }

        private void GrantMaxHpGain(int[] before)
        {
            for (int i = 0; i < before.Length; i++)
            {
                PartyMemberVO member = _hero.Members[i];
                if (!member.IsDown)
                    member.Hp += MaxHpOf(member) - before[i];
            }
        }

        /// <summary>The config decides which upgrades exist; each saved stack attaches to its entry, and an entry with none starts at zero.</summary>
        private void FillUpgrades()
        {
            foreach (UpgradeCVO upgrade in Config.Upgrades)
            {
                if (string.IsNullOrEmpty(upgrade.Id) || _hero.Upgrades.ContainsKey(upgrade.Id))
                {
                    FlowLogger.LogError($"CD_Gameplay lists an upgrade with an empty or repeated id '{upgrade.Id}'; it is ignored.", Config);
                    continue;
                }

                _hero.Upgrades[upgrade.Id] = new UpgradeVO { Config = upgrade };
            }

            foreach (UpgradeSVO save in _hero.Save.Upgrades)
            {
                if (save != null && _hero.Upgrades.TryGetValue(save.Id ?? string.Empty, out UpgradeVO record) && record.Save == null)
                {
                    save.Count = Mathf.Max(0, save.Count);
                    record.Save = save;
                }
            }

            foreach (UpgradeVO record in _hero.Upgrades.Values)
            {
                if (record.Save != null)
                    continue;

                record.Save = new UpgradeSVO { Id = record.Config.Id };
                _hero.Save.Upgrades.Add(record.Save);
            }
        }

        /// <summary>The config decides what the shop sells, in its order; the levels bought attach by id, and an item never bought starts at zero.</summary>
        private void FillShop()
        {
            var saved = new Dictionary<string, UpgradeSVO>();
            foreach (UpgradeSVO save in _hero.Save.ShopItems)
            {
                if (save != null && !string.IsNullOrEmpty(save.Id))
                    saved[save.Id] = save;
            }

            foreach (ShopItemCVO config in Config.Shop)
            {
                if (!saved.TryGetValue(config.Id ?? string.Empty, out UpgradeSVO save))
                {
                    save = new UpgradeSVO { Id = config.Id };
                    _hero.Save.ShopItems.Add(save);
                    saved[save.Id ?? string.Empty] = save;
                }

                save.Count = Mathf.Max(0, save.Count);
                _hero.Shop.Add(new ShopItemVO { Config = config, Save = save });
            }
        }

        /// <summary>The recruited heroes in the order the config lists them, so a new recruit takes its place in the formation.</summary>
        private void FillMembers()
        {
            // A fresh save - or one from before recruiting existed - starts with the starting members.
            if (_hero.Save.Recruited.Count == 0)
            {
                foreach (PartyMemberCVO member in Config.Party)
                {
                    if (member.StartsInParty)
                        _hero.Save.Recruited.Add(member.Role);
                }
            }

            var existing = new Dictionary<PartyRole, PartyMemberVO>();
            foreach (PartyMemberVO member in _hero.Members)
                existing[member.Config.Role] = member;

            _hero.Members.Clear();
            foreach (PartyMemberCVO member in Config.Party)
            {
                if (!_hero.Save.Recruited.Contains(member.Role))
                    continue;

                _hero.Members.Add(existing.TryGetValue(member.Role, out PartyMemberVO kept) ? kept : new PartyMemberVO { Config = member });
            }

            if (_hero.Members.Count == 0)
                FlowLogger.LogError("CD_Gameplay starts nobody in the party, so nobody can fight.", Config);
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
            save.Floor = Mathf.Max(1, save.Floor);
            save.FloorKills = Mathf.Max(0, save.FloorKills);
            save.PendingUpgrades = Mathf.Max(0, save.PendingUpgrades);
            save.Recruited ??= new List<PartyRole>();
            save.Upgrades ??= new List<UpgradeSVO>();
            save.ShopItems ??= new List<UpgradeSVO>();
            save.Shadows ??= new List<ShadowSVO>();
            save.Essence = Mathf.Max(0, save.Essence);
            save.Reawakenings = Mathf.Max(0, save.Reawakenings);
            save.BestFloor = Mathf.Max(save.Floor, save.BestFloor);
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
