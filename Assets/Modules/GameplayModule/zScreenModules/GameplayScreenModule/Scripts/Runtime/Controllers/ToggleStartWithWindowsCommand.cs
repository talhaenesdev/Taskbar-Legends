using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.DesktopWindowModule.Services;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    /// <summary>Windows itself remembers whether the game starts with it, so the switch flips what Windows answers.</summary>
    internal class ToggleStartWithWindowsCommand : Command
    {
        [Inject] private IDesktopWindowService _desktopWindowService { get; set; }

        public override void Execute() => _desktopWindowService.SetStartWithWindows(!_desktopWindowService.IsStartingWithWindows);
    }
}
