using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Models
{
    internal class CombatModel : ICombatModel
    {
        private int _spawnCount;
        private int _memberTurn;
        private int _enemyTurn;

        public bool IsRunning { get; set; }

        public EnemyRVO Enemy { get; private set; }
        public bool IsEnemyDead => Enemy == null || Enemy.Hp <= 0;

        public void Spawn(EnemySpawnVO spawn, float growthPerLevel, int arrivalTurns)
        {
            EnemyCVO config = spawn.Config;
            int level = Mathf.Max(1, spawn.Level);
            float growth = 1f + growthPerLevel * (level - 1);
            int maxHp = Mathf.Max(1, Mathf.RoundToInt(config.BaseHp * growth * spawn.HpMultiplier));

            string name = spawn.IsBoss ? $"[BOSS] {config.Name}" : spawn.IsElite ? $"Elite {config.Name}" : config.Name;

            Enemy = new EnemyRVO
            {
                SpawnId = ++_spawnCount,
                Config = config,
                Name = name,
                Level = level,
                Scale = config.Scale * spawn.ScaleMultiplier,
                MaxHp = maxHp,
                Hp = maxHp,
                Attack = Mathf.Max(1, Mathf.RoundToInt(config.BaseAttack * growth * spawn.AttackMultiplier)),
                Exp = Mathf.Max(1, Mathf.RoundToInt(config.BaseExp * growth * spawn.RewardMultiplier)),
                Gold = Mathf.Max(0, Mathf.RoundToInt(config.BaseGold * growth * spawn.RewardMultiplier)),
                IsElite = spawn.IsElite,
                IsBoss = spawn.IsBoss,
                ArrivalTurns = Mathf.Max(0, arrivalTurns),
            };

            _enemyTurn = 0;
        }

        public void DamageEnemy(int amount)
        {
            if (Enemy == null) return;
            Enemy.Hp = Mathf.Max(0, Enemy.Hp - Mathf.Max(0, amount));
        }

        public int TakeMemberTurn(int partySize)
        {
            if (partySize <= 0) return -1;

            int index = _memberTurn % partySize;
            _memberTurn = (_memberTurn + 1) % partySize;
            return index;
        }

        public bool TakeEnemyTurn(int attackEveryTurns)
        {
            _enemyTurn++;
            return _enemyTurn % Mathf.Max(1, attackEveryTurns) == 0;
        }

        public EnemyStateVO GetEnemyState() => Enemy == null
            ? new EnemyStateVO { Name = string.Empty, Tint = Color.gray }
            : new EnemyStateVO
            {
                SpawnId = Enemy.SpawnId,
                Name = Enemy.Name,
                Level = Enemy.Level,
                Hp = Enemy.Hp,
                MaxHp = Enemy.MaxHp,
                Look = Enemy.Config.Look,
                Scale = Enemy.Scale,
                Tint = Enemy.Config.Tint,
                IsElite = Enemy.IsElite,
                IsBoss = Enemy.IsBoss,
            };
    }
}
