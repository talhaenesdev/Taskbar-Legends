using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>A defeated boss clears the floor and sends the party one deeper; any other kill counts towards the boss.</summary>
    internal class JudgeFloorProgressCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (!_combatModel.Enemy.IsBoss)
            {
                _heroModel.CountFloorKill();
                return;
            }

            int cleared = _heroModel.Floor;
            _heroModel.AdvanceFloor();
            _signals.Outgoing.SystemAnnounced.Dispatch($"FLOOR {cleared} CLEARED!  Descending to floor {_heroModel.Floor}...");
        }
    }
}
