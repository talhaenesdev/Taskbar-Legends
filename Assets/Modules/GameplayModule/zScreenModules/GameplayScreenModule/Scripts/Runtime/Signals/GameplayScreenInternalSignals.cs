using FlowIoC.BaseModule.Signals;

namespace Modules.GameplayModule.GameplayScreenModule.Signals
{
    internal class GameplayScreenInternalSignals : ISignalHolder
    {
        /// <summary>The window has been grabbed: remember where it and the cursor are.</summary>
        public Signal WindowDragBegan = new();

        /// <summary>The cursor moved while the window is held; sent every frame of the drag.</summary>
        public Signal WindowDragged = new(hideCommandLog: true);
    }
}
