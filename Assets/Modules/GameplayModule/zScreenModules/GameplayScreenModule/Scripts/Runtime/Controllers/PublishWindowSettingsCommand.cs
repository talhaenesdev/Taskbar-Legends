using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.DesktopWindowModule.Services;
using Modules.GameplayModule.GameplayScreenModule.Models;
using Modules.GameplayModule.GameplayScreenModule.Signals;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    /// <summary>Tells the open settings panel where the settings stand now.</summary>
    internal class PublishWindowSettingsCommand : Command
    {
        [Inject] private IWindowSettingsModel _settingsModel { get; set; }
        [Inject] private IDesktopWindowService _desktopWindowService { get; set; }
        [InjectSignal] private GameplayScreenInternalSignals _internalSignals { get; set; }

        public override void Execute() =>
            _internalSignals.SettingsChanged.Dispatch(_settingsModel.GetState(_desktopWindowService.IsStartingWithWindows));
    }
}
