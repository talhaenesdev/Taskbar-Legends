using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Spends the experience on every level it pays for - a big kill may be worth more than one - and announces the last.</summary>
    internal class ApplyLevelUpsCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            int gained = 0;
            while (_heroModel.TryLevelUp())
                gained++;

            if (gained == 0)
                return;

            _signals.Outgoing.SystemAnnounced.Dispatch($"LEVEL UP!  You have reached Lv {_heroModel.Level}.");
        }
    }
}
