using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// A creature still walking in spends the turn arriving. Once it has arrived it strikes the
    /// front member still standing, on the turns its pace allows.
    /// </summary>
    internal class EnemyStrikeCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (_combatModel.IsEnemyDead)
                return;

            if (_combatModel.Enemy.IsArriving)
            {
                _combatModel.Enemy.ArrivalTurns--;
                return;
            }

            if (!_combatModel.TakeEnemyTurn(_heroModel.Config.EnemyAttackEveryTurns))
                return;

            PartyMemberVO target = _heroModel.FrontMember();
            if (target == null)
                return;

            int damage = Mathf.Max(1, Mathf.RoundToInt(_combatModel.Enemy.Attack * Random.Range(0.8f, 1.2f)));
            _heroModel.Damage(target, damage);

            _signals.Outgoing.HeroHit.Dispatch(target.Config.Role, damage);
        }
    }
}
