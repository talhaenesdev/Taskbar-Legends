namespace Modules.MainModule.Constants
{
    public static class MainConstants
    {
        public const string BOOT_SET = "Boot";
        public const string SCREENS_STEP = "Screens";
        public const string POOLS_STEP = "Pools";

        /// <summary>The size of the strip the game window is docked as, above the taskbar.</summary>
        public const int WINDOW_WIDTH = 640;
        public const int WINDOW_HEIGHT = 240;

        /// <summary>
        /// The pool groups the boot fills before the main screen opens. Empty in the shipped set, so
        /// the Pools step skips; a game lists its group keys here.
        /// </summary>
        public static readonly string[] BootPoolGroups = { };
    }
}
