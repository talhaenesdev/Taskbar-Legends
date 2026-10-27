using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>The defeated creature's experience and gold go to the party, with its upgrades, and the kill is counted.</summary>
    internal class GrantRewardsCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            EnemyRVO enemy = _combatModel.Enemy;

            (int exp, int gold) = _heroModel.GrantRewards(enemy.Exp, enemy.Gold);
            _heroModel.CountKill();

            _signals.Outgoing.CombatLogged.Dispatch($"{enemy.Name} defeated. +{exp} EXP  +{gold} G");
        }
    }
}
