namespace Modules.DesktopWindowModule.Services
{
    /// <summary>
    /// The player window as a strip on the desktop: borderless, always on top, docked to the
    /// bottom right of the work area, and moved by dragging it. Every call does nothing outside a
    /// Windows player build, so the Editor plays as usual.
    /// </summary>
    public partial interface IDesktopWindowService
    {
        /// <summary>Resizes the window to width x height, strips its frame and docks it above the taskbar.</summary>
        void DockToTaskbar(int width, int height);

        /// <summary>Remembers where the cursor and the window are, for the drag that follows.</summary>
        void BeginDrag();

        /// <summary>Moves the window by as much as the cursor has moved since BeginDrag.</summary>
        void Drag();

        /// <summary>The steps a chain binds, each a file of its own beside this one.</summary>
        public static partial class Commands
        {
        }
    }
}
