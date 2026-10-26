using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Every member still standing regains a little HP, and the turn ends by publishing where the party stands.</summary>
    internal class RegenerateHeroCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            foreach (PartyMemberVO member in _heroModel.Members)
                _heroModel.Heal(member, _heroModel.RegenOf(member));

            _signals.Outgoing.HeroChanged.Dispatch(_heroModel.GetState());
        }
    }
}
