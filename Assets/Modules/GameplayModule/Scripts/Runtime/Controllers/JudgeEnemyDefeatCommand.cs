using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>A creature brought to zero runs the defeat flow: rewards, levels, rank, the next creature.</summary>
    internal class JudgeEnemyDefeatCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }

        public override void Execute()
        {
            if (_combatModel.IsEnemyDead)
                _internalSignals.EnemyDefeated.Dispatch();
        }
    }
}
