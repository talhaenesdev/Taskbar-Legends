using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>The whole party back at full HP. Progress is kept.</summary>
    internal class RestorePartyCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute() => _heroModel.RestoreAll();
    }
}
