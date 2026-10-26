using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;

namespace Modules.DesktopWindowModule.Services
{
    public partial interface IDesktopWindowService
    {
        public static partial class Commands
        {
            /// <summary>
            /// Docks the window: <c>.ToSequence&lt;IDesktopWindowService.Commands.DockToTaskbar&gt;(560, 180)</c>.
            /// Synchronous - the frame is stripped a couple of frames later, once the resize has landed.
            /// </summary>
            public class DockToTaskbar : Command<int, int>
            {
                [Inject] private IDesktopWindowService _desktopWindowService { get; set; }

                public override void Execute(int width, int height) => _desktopWindowService.DockToTaskbar(width, height);
            }
        }
    }
}
