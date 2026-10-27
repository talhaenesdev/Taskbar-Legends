using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// The party member whose turn it is acts. A healer mends the most hurt member while anybody
    /// is below the healing threshold; everyone else - and a healer with nobody to mend - strikes
    /// the creature as many times as their role allows, each blow with its own chance to be
    /// critical. A member who is down loses the turn; nobody acts while the creature walks in.
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

            if (member.Config.HealPower > 0f && TryHeal(member))
                return;

            for (int hit = 0; hit < member.Config.Hits && !_combatModel.IsEnemyDead; hit++)
                Strike(member);

            _signals.Outgoing.EnemyChanged.Dispatch(_combatModel.GetEnemyState());
        }

        private bool TryHeal(PartyMemberVO healer)
        {
            PartyMemberVO target = _heroModel.MostHurtMember();
            if (target == null || _heroModel.HpShareOf(target) >= _heroModel.Config.HealBelowShare)
                return false;

            int amount = Mathf.Max(1, Mathf.RoundToInt(_heroModel.AttackOf(healer) * healer.Config.HealPower));
            _heroModel.Heal(target, amount);

            _signals.Outgoing.MemberHealed.Dispatch(healer.Config.Role, target.Config.Role, amount);
            return true;
        }

        private void Strike(PartyMemberVO member)
        {
            bool isCritical = Random.value < _heroModel.CritChanceOf(member);
            float multiplier = isCritical ? _heroModel.CritMultiplier : 1f;
            int damage = Mathf.Max(1, Mathf.RoundToInt(_heroModel.AttackOf(member) * Random.Range(0.85f, 1.15f) * multiplier));

            _combatModel.DamageEnemy(damage);
            _signals.Outgoing.EnemyHit.Dispatch(member.Config.Role, damage, isCritical);
        }
    }
}
