using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Moves the hero to the rank the new level has reached, and announces it when it changed.</summary>
    internal class ApplyRankCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (!_heroModel.RefreshRank())
                return;

            _signals.Outgoing.RankedUp.Dispatch(_heroModel.Rank.Rank);
            _signals.Outgoing.SystemAnnounced.Dispatch($"RANK UP!  You are now a {_heroModel.Rank.Title}.");
        }
    }
}
