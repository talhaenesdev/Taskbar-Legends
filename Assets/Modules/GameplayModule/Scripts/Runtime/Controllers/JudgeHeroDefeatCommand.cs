using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>A hero brought to zero runs the defeat flow: revive, and a fresh creature.</summary>
    internal class JudgeHeroDefeatCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }

        public override void Execute()
        {
            if (_heroModel.IsDead)
                _internalSignals.HeroDefeated.Dispatch();
        }
    }
}
