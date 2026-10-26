using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Models
{
    internal class CombatModel : ICombatModel
    {
        public bool IsRunning { get; set; }

        public EnemyRVO Enemy { get; private set; }
        public bool IsEnemyDead => Enemy == null || Enemy.Hp <= 0;

        public void Spawn(EnemyCVO config, int level, float growthPerLevel)
        {
            level = Mathf.Max(1, level);
            float scale = 1f + growthPerLevel * (level - 1);
            int maxHp = Mathf.Max(1, Mathf.RoundToInt(config.BaseHp * scale));

            Enemy = new EnemyRVO
            {
                Config = config,
                Level = level,
                MaxHp = maxHp,
                Hp = maxHp,
                Attack = Mathf.Max(1, Mathf.RoundToInt(config.BaseAttack * scale)),
                Exp = Mathf.Max(1, Mathf.RoundToInt(config.BaseExp * scale)),
                Gold = Mathf.Max(0, Mathf.RoundToInt(config.BaseGold * scale)),
                JustSpawned = true,
            };
        }

        public void DamageEnemy(int amount)
        {
            if (Enemy == null) return;
            Enemy.Hp = Mathf.Max(0, Enemy.Hp - Mathf.Max(0, amount));
        }

        public EnemyStateVO GetEnemyState() => Enemy == null
            ? new EnemyStateVO { Name = string.Empty, Tint = Color.gray }
            : new EnemyStateVO
            {
                Name = Enemy.Config.Name,
                Level = Enemy.Level,
                Hp = Enemy.Hp,
                MaxHp = Enemy.MaxHp,
                Tint = Enemy.Config.Tint,
            };
    }
}
