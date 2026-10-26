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

        public void Spawn(EnemyCVO config, int level, float growthPerLevel, int arrivalTurns)
        {
            level = Mathf.Max(1, level);
            float scale = 1f + growthPerLevel * (level - 1);
            int maxHp = Mathf.Max(1, Mathf.RoundToInt(config.BaseHp * scale));

            Enemy = new EnemyRVO
            {
                SpawnId = ++_spawnCount,
                Config = config,
                Level = level,
                MaxHp = maxHp,
                Hp = maxHp,
                Attack = Mathf.Max(1, Mathf.RoundToInt(config.BaseAttack * scale)),
                Exp = Mathf.Max(1, Mathf.RoundToInt(config.BaseExp * scale)),
                Gold = Mathf.Max(0, Mathf.RoundToInt(config.BaseGold * scale)),
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
                Name = Enemy.Config.Name,
                Level = Enemy.Level,
                Hp = Enemy.Hp,
                MaxHp = Enemy.MaxHp,
                Look = Enemy.Config.Look,
                Scale = Enemy.Config.Scale,
                Tint = Enemy.Config.Tint,
            };
    }
}
