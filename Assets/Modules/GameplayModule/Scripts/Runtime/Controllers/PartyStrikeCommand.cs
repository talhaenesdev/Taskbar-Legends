using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// The party member whose turn it is strikes the creature: its attack, a little spread, and a
    /// critical now and then. A member who is down loses the turn; nobody strikes a creature that
    /// is still walking in.
    /// </summary>
    internal class PartyStrikeCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (_combatModel.IsEnemyDead || _combatModel.Enemy.IsArriving)
                return;

            int index = _combatModel.TakeMemberTurn(_heroModel.Members.Count);
            if (index < 0)
                return;

            PartyMemberVO member = _heroModel.Members[index];
            if (member.IsDown)
                return;

            bool isCritical = Random.value < _heroModel.Config.Hero.CritChance;
            float multiplier = isCritical ? _heroModel.Config.Hero.CritMultiplier : 1f;
            int damage = Mathf.Max(1, Mathf.RoundToInt(_heroModel.AttackOf(member) * Random.Range(0.85f, 1.15f) * multiplier));

            _combatModel.DamageEnemy(damage);

            _signals.Outgoing.EnemyHit.Dispatch(member.Config.Role, damage, isCritical);
            _signals.Outgoing.EnemyChanged.Dispatch(_combatModel.GetEnemyState());
        }
    }
}
