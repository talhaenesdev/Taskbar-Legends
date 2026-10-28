using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.DesktopWindowModule.Services;
using Modules.GameplayModule.GameplayScreenModule.Models;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    /// <summary>Puts the saved opacity and stay-on-top choice on the window.</summary>
    internal class ApplyWindowSettingsCommand : Command
    {
        [Inject] private IWindowSettingsModel _settingsModel { get; set; }
        [Inject] private IDesktopWindowService _desktopWindowService { get; set; }

        public override void Execute()
        {
            _desktopWindowService.SetOpacity(_settingsModel.Opacity);
            _desktopWindowService.SetTopmost(_settingsModel.IsTopmost);
        }
    }
}
