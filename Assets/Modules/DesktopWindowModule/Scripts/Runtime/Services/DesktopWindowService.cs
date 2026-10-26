using System;
using System.Runtime.InteropServices;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Coroutine;
using UnityEngine;

namespace Modules.DesktopWindowModule.Services
{
    /// <summary>
    /// user32 does the work. The window handle is the process's main window, which is the player
    /// window whether or not it has focus - GetActiveWindow would answer null when the game starts
    /// behind another window.
    /// </summary>
    public class DesktopWindowService : IDesktopWindowService
    {
        /// <summary>Frames to wait after a resize before restyling, so the restyle lands on the new size.</summary>
        private const int FRAMES_AFTER_RESIZE = 2;

        /// <summary>Gap kept between the window and the edge of the work area.</summary>
        private const int DOCK_MARGIN = 8;

        [Inject] private ICoroutineProvider _coroutineProvider { get; set; }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private Vector2Int _dragCursorStart;
        private Vector2Int _dragWindowStart;
#endif

        public void DockToTaskbar(int width, int height)
        {
            // An idle game keeps fighting while another window has focus.
            Application.runInBackground = true;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            _coroutineProvider.WaitForEndOfFrames(FRAMES_AFTER_RESIZE, () => ApplyDock(width, height));
#endif
        }

        public void BeginDrag()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr window = Window;
            if (window == IntPtr.Zero || !GetCursorPos(out Point cursor) || !GetWindowRect(window, out Rect rect))
                return;

            _dragCursorStart = new Vector2Int(cursor.X, cursor.Y);
            _dragWindowStart = new Vector2Int(rect.Left, rect.Top);
#endif
        }

        public void Drag()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr window = Window;
            if (window == IntPtr.Zero || !GetCursorPos(out Point cursor))
                return;

            int x = _dragWindowStart.x + cursor.X - _dragCursorStart.x;
            int y = _dragWindowStart.y + cursor.Y - _dragCursorStart.y;
            SetWindowPos(window, HWND_TOPMOST, x, y, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE);
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private IntPtr _window;

        /// <summary>The player's own top-level window: the one of class UnityWndClass that this process owns. Found once.</summary>
        private IntPtr Window
        {
            get
            {
                if (_window != IntPtr.Zero)
                    return _window;

                uint processId = GetCurrentProcessId();
                var className = new System.Text.StringBuilder(64);

                EnumWindows((candidate, _) =>
                {
                    GetWindowThreadProcessId(candidate, out uint owner);
                    if (owner != processId)
                        return true;

                    className.Clear();
                    GetClassName(candidate, className, className.Capacity);
                    if (className.ToString() != "UnityWndClass")
                        return true;

                    _window = candidate;
                    return false;
                }, IntPtr.Zero);

                return _window;
            }
        }

        private void ApplyDock(int width, int height)
        {
            IntPtr window = Window;
            if (window == IntPtr.Zero)
                return;

            SetWindowLong(window, GWL_STYLE, WS_POPUP | WS_VISIBLE);

            var workArea = new Rect();
            SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0);

            int x = workArea.Right - width - DOCK_MARGIN;
            int y = workArea.Bottom - height - DOCK_MARGIN;
            SetWindowPos(window, HWND_TOPMOST, x, y, width, height, SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        }

        private const int GWL_STYLE = -16;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SPI_GETWORKAREA = 0x0030;
        private static readonly IntPtr HWND_TOPMOST = new(-1);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int X;
            public int Y;
        }

        private delegate bool EnumWindowsProc(IntPtr window, IntPtr param);

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr param);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, System.Text.StringBuilder name, int capacity);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentProcessId();
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr window, int index, uint value);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rect rect);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint action, uint param, ref Rect rect, uint winIni);
#endif
    }
}
