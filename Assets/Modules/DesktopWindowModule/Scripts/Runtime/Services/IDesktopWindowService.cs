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

        /// <summary>How see-through the window is, from 0.2 to 1 (fully opaque).</summary>
        void SetOpacity(float opacity);

        /// <summary>Keeps the window above every other window, or lets others cover it.</summary>
        void SetTopmost(bool isTopmost);

        /// <summary>True when Windows starts this game when the user signs in.</summary>
        bool IsStartingWithWindows { get; }

        /// <summary>Registers or unregisters this game to start when the user signs in, for the current user only.</summary>
        void SetStartWithWindows(bool starts);

        /// <summary>Hides the window and its taskbar button behind an icon in the notification area; clicking the icon brings it back.</summary>
        void HideToTray(string tooltip);

        /// <summary>The steps a chain binds, each a file of its own beside this one.</summary>
        public static partial class Commands
        {
        }
    }
}
