using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>A fallen hero is brought back at full HP. Progress is kept; the creature that won is replaced.</summary>
    internal class ReviveHeroCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute() => _heroModel.RestoreFullHp();
    }
}
