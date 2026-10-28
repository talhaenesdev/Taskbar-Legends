using FlowIoC.BaseModule.Signals;
using Modules.GameplayModule.GameplayScreenModule.Data.ValueObjects;

namespace Modules.GameplayModule.GameplayScreenModule.Signals
{
    internal class GameplayScreenInternalSignals : ISignalHolder
    {
        /// <summary>The window has been grabbed: remember where it and the cursor are.</summary>
        public Signal WindowDragBegan = new();

        /// <summary>The cursor moved while the window is held; sent every frame of the drag.</summary>
        public Signal WindowDragged = new(hideCommandLog: true);

        // The settings panel's switches, and where the settings stand after one is flipped.
        public Signal OpacityClicked = new();
        public Signal TopmostClicked = new();
        public Signal StartWithWindowsClicked = new();
        public Signal TrayClicked = new();
        public Signal<WindowSettingsVO> SettingsChanged = new();
    }
}
