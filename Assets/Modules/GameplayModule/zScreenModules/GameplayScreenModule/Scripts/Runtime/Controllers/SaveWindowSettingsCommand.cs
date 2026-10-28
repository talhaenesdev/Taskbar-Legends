using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.GameplayScreenModule.Models;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    internal class SaveWindowSettingsCommand : Command
    {
        [Inject] private IWindowSettingsModel _settingsModel { get; set; }

        public override void Execute() => _settingsModel.Save();
    }
}
