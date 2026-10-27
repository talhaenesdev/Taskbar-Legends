using System.Collections.Generic;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// Decides the next creature from the floor the party is on: the creatures the floor has
    /// unlocked, a level that climbs with the floor and the kills on it, now and then an elite -
    /// and, once the floor's kills are done, its boss: the strongest creature the floor knows.
    /// </summary>
    internal class SpawnEnemyCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        private readonly List<EnemyCVO> _unlocked = new();

        public override void Execute()
        {
            CD_Gameplay config = _heroModel.Config;
            int floor = _heroModel.Floor;

            _unlocked.Clear();
            foreach (EnemyCVO enemy in config.Enemies)
            {
                if (enemy.MinFloor <= floor)
                    _unlocked.Add(enemy);
            }

            if (_unlocked.Count == 0)
            {
                FlowLogger.LogError($"CD_Gameplay lists no creature for floor {floor}, so nothing can be fought.");
                Retain();
                Stop();
                return;
            }

            int baseLevel = 1 + (floor - 1) * config.EnemyLevelsPerFloor + _heroModel.FloorKills / 4;
            var spawn = new EnemySpawnVO();

            if (_heroModel.IsBossDue)
            {
                spawn.Config = Strongest(_unlocked);
                spawn.Level = baseLevel + 2;
                spawn.IsBoss = true;
                spawn.HpMultiplier = config.BossHpMultiplier;
                spawn.AttackMultiplier = config.BossAttackMultiplier;
                spawn.RewardMultiplier = config.BossRewardMultiplier;
                spawn.ScaleMultiplier = config.BossScaleMultiplier;
            }
            else
            {
                // The newest creatures the floor has unlocked come up more often than the oldest.
                int index = Mathf.Clamp(_unlocked.Count - 1 - Mathf.FloorToInt(Mathf.Pow(Random.value, 2f) * _unlocked.Count), 0, _unlocked.Count - 1);
                spawn.Config = _unlocked[index];
                spawn.Level = Mathf.Max(1, baseLevel + Random.Range(0, 2));

                if (Random.value < config.EliteChance)
                {
                    spawn.IsElite = true;
                    spawn.HpMultiplier = config.EliteHpMultiplier;
                    spawn.AttackMultiplier = config.EliteAttackMultiplier;
                    spawn.RewardMultiplier = config.EliteRewardMultiplier;
                    spawn.ScaleMultiplier = 1.1f;
                }
            }

            int arrivalTurns = Mathf.CeilToInt(config.EnemyArrivalSeconds / _heroModel.TurnSeconds);
            _combatModel.Spawn(spawn, config.EnemyGrowthPerLevel, arrivalTurns);
            _signals.Outgoing.EnemyChanged.Dispatch(_combatModel.GetEnemyState());

            if (spawn.IsBoss)
                _signals.Outgoing.SystemAnnounced.Dispatch($"The boss of floor {floor} has appeared: {spawn.Config.Name}!");

            _signals.Outgoing.CombatLogged.Dispatch($"{_combatModel.Enemy.Name} (Lv {_combatModel.Enemy.Level}) appears.");
        }

        private static EnemyCVO Strongest(List<EnemyCVO> enemies)
        {
            EnemyCVO strongest = enemies[0];
            foreach (EnemyCVO enemy in enemies)
            {
                if (enemy.BaseHp * enemy.BaseAttack > strongest.BaseHp * strongest.BaseAttack)
                    strongest = enemy;
            }

            return strongest;
        }
    }
}
