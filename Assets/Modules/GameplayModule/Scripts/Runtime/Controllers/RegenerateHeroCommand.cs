using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>The hero regains a little HP every turn, and the turn ends by publishing where the hero stands.</summary>
    internal class RegenerateHeroCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            _heroModel.Heal(_heroModel.RegenPerTurn);
            _signals.Outgoing.HeroChanged.Dispatch(_heroModel.GetState());
        }
    }
}
