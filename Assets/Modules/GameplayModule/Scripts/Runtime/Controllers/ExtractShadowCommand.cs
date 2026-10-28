using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>A defeated boss may rise as a shadow soldier - by chance, and while the army has room.</summary>
    internal class ExtractShadowCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            EnemyRVO enemy = _combatModel.Enemy;
            if (!enemy.IsBoss || !_heroModel.HasShadowRoom || Random.value >= _heroModel.Config.ShadowChance)
                return;

            _heroModel.AddShadow(enemy.Config.Name, enemy.Config.Look);
            _signals.Outgoing.SystemAnnounced.Dispatch(
                $"ARISE!  {enemy.Config.Name} joins your shadow army.  ({_heroModel.ShadowCount} shadows, +{Mathf.RoundToInt(_heroModel.ShadowAttackBonus * 100f)}% ATK)");
        }
    }
}
