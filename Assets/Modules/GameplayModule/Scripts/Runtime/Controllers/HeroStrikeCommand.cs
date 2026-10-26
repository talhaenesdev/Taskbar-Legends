using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>The hero strikes the creature: the attack, a little spread, and a critical now and then.</summary>
    internal class HeroStrikeCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (_combatModel.IsEnemyDead)
                return;

            bool isCritical = Random.value < _heroModel.Config.Hero.CritChance;
            float multiplier = isCritical ? _heroModel.Config.Hero.CritMultiplier : 1f;
            int damage = Mathf.Max(1, Mathf.RoundToInt(_heroModel.Attack * Random.Range(0.85f, 1.15f) * multiplier));

            _combatModel.DamageEnemy(damage);

            _signals.Outgoing.EnemyHit.Dispatch(damage, isCritical);
            _signals.Outgoing.EnemyChanged.Dispatch(_combatModel.GetEnemyState());
        }
    }
}
