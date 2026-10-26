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
    /// Picks the next creature from those the hero's level has unlocked, at a level close to the
    /// hero's own, and puts it on the field.
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

            _unlocked.Clear();
            foreach (EnemyCVO enemy in config.Enemies)
            {
                if (enemy.MinHeroLevel <= _heroModel.Level)
                    _unlocked.Add(enemy);
            }

            if (_unlocked.Count == 0)
            {
                FlowLogger.LogError("CD_Gameplay lists no creature for the hero's level, so nothing can be fought.");
                Retain();
                Stop();
                return;
            }

            // The newest creatures the level has unlocked come up more often than the oldest.
            int index = Mathf.Clamp(_unlocked.Count - 1 - Mathf.FloorToInt(Mathf.Pow(Random.value, 2f) * _unlocked.Count), 0, _unlocked.Count - 1);
            EnemyCVO picked = _unlocked[index];
            int level = Mathf.Max(1, _heroModel.Level + Random.Range(-1, 2));

            _combatModel.Spawn(picked, level, config.EnemyGrowthPerLevel);
            _signals.Outgoing.EnemyChanged.Dispatch(_combatModel.GetEnemyState());
            _signals.Outgoing.CombatLogged.Dispatch($"A wild {picked.Name} (Lv {level}) appears.");
        }
    }
}
