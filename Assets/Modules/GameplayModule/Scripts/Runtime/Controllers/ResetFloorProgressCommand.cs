using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>A fallen party starts its floor over: the kills towards the boss are lost, the floor itself is kept.</summary>
    internal class ResetFloorProgressCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute() => _heroModel.ResetFloorProgress();
    }
}
