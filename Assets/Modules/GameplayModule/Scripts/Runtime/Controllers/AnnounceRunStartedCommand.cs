using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Announces the run with the hero and the first creature, so whatever shows them opens already filled.</summary>
    internal class AnnounceRunStartedCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute() =>
            _signals.Outgoing.RunStarted.Dispatch(_heroModel.GetState(), _combatModel.GetEnemyState());
    }
}
