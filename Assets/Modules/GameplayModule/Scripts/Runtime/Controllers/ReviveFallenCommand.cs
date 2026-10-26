using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Members felled by the creature just defeated get back up with part of their HP.</summary>
    internal class ReviveFallenCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute() => _heroModel.ReviveFallen(_heroModel.Config.ReviveShareAfterKill);
    }
}
