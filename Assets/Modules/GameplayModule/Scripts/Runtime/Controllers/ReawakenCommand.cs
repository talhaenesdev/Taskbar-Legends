using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Starts the climb over for Essence once the party is deep enough; before that, says how deep it has to go and ends the flow.</summary>
    internal class ReawakenCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (!_heroModel.CanReawaken)
            {
                _signals.Outgoing.SystemAnnounced.Dispatch($"Reach floor {_heroModel.Config.ReawakenMinFloor} to reawaken.");
                Retain();
                Stop();
                return;
            }

            int gained = _heroModel.Reawaken();
            _signals.Outgoing.SystemAnnounced.Dispatch(
                $"REAWAKENED!  +{gained} Essence  -  all stats +{Mathf.RoundToInt(_heroModel.EssenceBonus * 100f)}%. The climb begins again.");
        }
    }
}
