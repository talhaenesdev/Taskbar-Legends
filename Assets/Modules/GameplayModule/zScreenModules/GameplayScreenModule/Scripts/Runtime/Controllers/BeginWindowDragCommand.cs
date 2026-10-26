using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.DesktopWindowModule.Services;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    internal class BeginWindowDragCommand : Command
    {
        [Inject] private IDesktopWindowService _desktopWindowService { get; set; }

        public override void Execute() => _desktopWindowService.BeginDrag();
    }
}
