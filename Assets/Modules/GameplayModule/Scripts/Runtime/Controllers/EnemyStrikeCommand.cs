using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>The creature strikes back - unless it only just appeared this turn.</summary>
    internal class EnemyStrikeCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (_combatModel.IsEnemyDead)
                return;

            if (_combatModel.Enemy.JustSpawned)
            {
                _combatModel.Enemy.JustSpawned = false;
                return;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(_combatModel.Enemy.Attack * Random.Range(0.8f, 1.2f)));
            _heroModel.Damage(damage);

            _signals.Outgoing.HeroHit.Dispatch(damage);
        }
    }
}
