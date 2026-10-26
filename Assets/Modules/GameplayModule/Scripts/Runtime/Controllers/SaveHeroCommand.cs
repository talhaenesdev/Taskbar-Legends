using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    internal class SaveHeroCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute() => _heroModel.Save();
    }
}
