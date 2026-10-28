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

        private const float MIN_OPACITY = 0.2f;

        [Inject] private ICoroutineProvider _coroutineProvider { get; set; }

        private bool _isTopmost = true;

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
            SetWindowPos(window, ZOrder, x, y, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE);
#endif
        }

        public void SetOpacity(float opacity)
        {
            opacity = Mathf.Clamp(opacity, MIN_OPACITY, 1f);

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr window = Window;
            if (window == IntPtr.Zero)
                return;

            // A fully opaque window drops the layered style, which costs a copy every frame.
            uint style = GetWindowLong(window, GWL_EXSTYLE);
            if (opacity >= 1f)
            {
                SetWindowLong(window, GWL_EXSTYLE, style & ~WS_EX_LAYERED);
                return;
            }

            SetWindowLong(window, GWL_EXSTYLE, style | WS_EX_LAYERED);
            SetLayeredWindowAttributes(window, 0, (byte) Mathf.RoundToInt(opacity * 255f), LWA_ALPHA);
#endif
        }

        public void SetTopmost(bool isTopmost)
        {
            _isTopmost = isTopmost;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr window = Window;
            if (window != IntPtr.Zero)
                SetWindowPos(window, ZOrder, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
#endif
        }

        public bool IsStartingWithWindows
        {
            get
            {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                if (RegOpenKeyEx(HKEY_CURRENT_USER, RUN_KEY, 0, KEY_QUERY_VALUE, out IntPtr key) != 0)
                    return false;

                int size = 0;
                bool exists = RegQueryValueEx(key, Application.productName, IntPtr.Zero, out _, IntPtr.Zero, ref size) == 0;
                RegCloseKey(key);
                return exists;
#else
                return false;
#endif
            }
        }

        public void SetStartWithWindows(bool starts)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (RegOpenKeyEx(HKEY_CURRENT_USER, RUN_KEY, 0, KEY_SET_VALUE, out IntPtr key) != 0)
                return;

            if (starts)
            {
                string command = $"\"{ExecutablePath()}\"";
                RegSetValueEx(key, Application.productName, 0, REG_SZ, command, (command.Length + 1) * 2);
            }
            else
            {
                RegDeleteValue(key, Application.productName);
            }

            RegCloseKey(key);
#endif
        }

        public void HideToTray(string tooltip)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr window = Window;
            if (window == IntPtr.Zero)
                return;

            HookWindowProc(window);

            var icon = new NotifyIconData
            {
                Size = Marshal.SizeOf<NotifyIconData>(),
                Window = window,
                Id = TRAY_ICON_ID,
                Flags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                CallbackMessage = WM_TRAY,
                Icon = AppIcon(),
                Tip = tooltip,
            };

            if (!Shell_NotifyIcon(NIM_ADD, ref icon))
                return;

            _isInTray = true;
            ShowWindow(window, SW_HIDE);
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private IntPtr _window;
        private bool _isInTray;

        private IntPtr ZOrder => _isTopmost ? HWND_TOPMOST : HWND_NOTOPMOST;

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
            SetWindowPos(window, ZOrder, x, y, width, height, SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        }

        private void RestoreFromTray()
        {
            if (!_isInTray)
                return;

            _isInTray = false;
            var icon = new NotifyIconData { Size = Marshal.SizeOf<NotifyIconData>(), Window = _window, Id = TRAY_ICON_ID };
            Shell_NotifyIcon(NIM_DELETE, ref icon);

            ShowWindow(_window, SW_SHOW);
            SetWindowPos(_window, ZOrder, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            SetForegroundWindow(_window);
        }

        private void RemoveTrayIcon()
        {
            if (!_isInTray)
                return;

            _isInTray = false;
            var icon = new NotifyIconData { Size = Marshal.SizeOf<NotifyIconData>(), Window = _window, Id = TRAY_ICON_ID };
            Shell_NotifyIcon(NIM_DELETE, ref icon);
        }

        private static string ExecutablePath()
        {
            var path = new System.Text.StringBuilder(1024);
            GetModuleFileName(IntPtr.Zero, path, path.Capacity);
            return path.ToString();
        }

        private static IntPtr AppIcon()
        {
            IntPtr icon = ExtractIcon(GetModuleHandle(null), ExecutablePath(), 0);
            return icon.ToInt64() > 1 ? icon : LoadIcon(IntPtr.Zero, IDI_APPLICATION);
        }

        // The tray icon reports its clicks as messages to the player window, so the window's
        // procedure is wrapped once: the tray message is answered here, everything else goes on to
        // Unity's own. Native code calls the wrapper, which is what forces the one static owner -
        // a callback the engine hands to Windows cannot reach an instance any other way.
        private static DesktopWindowService _hooked;
        private static IntPtr _originalWindowProc;
        private static WindowProc _windowProc;

        private void HookWindowProc(IntPtr window)
        {
            if (_hooked != null)
                return;

            _hooked = this;
            _windowProc = OnWindowMessage;
            _originalWindowProc = SetWindowLongPtr(window, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_windowProc));
            Application.quitting += RemoveTrayIcon;
        }

        [AOT.MonoPInvokeCallback(typeof(WindowProc))]
        private static IntPtr OnWindowMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam)
        {
            if (message == WM_TRAY)
            {
                int mouse = lParam.ToInt32() & 0xFFFF;
                if (mouse is WM_LBUTTONUP or WM_LBUTTONDBLCLK or WM_RBUTTONUP)
                    _hooked?.RestoreFromTray();
                return IntPtr.Zero;
            }

            return CallWindowProc(_originalWindowProc, window, message, wParam, lParam);
        }

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int GWLP_WNDPROC = -4;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const uint LWA_ALPHA = 0x2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SPI_GETWORKAREA = 0x0030;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new(-2);

        private const uint WM_TRAY = 0x8000 + 0x51;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONUP = 0x0205;
        private const uint TRAY_ICON_ID = 1;
        private const uint NIM_ADD = 0;
        private const uint NIM_DELETE = 2;
        private const uint NIF_MESSAGE = 0x1;
        private const uint NIF_ICON = 0x2;
        private const uint NIF_TIP = 0x4;
        private static readonly IntPtr IDI_APPLICATION = new(32512);

        private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly IntPtr HKEY_CURRENT_USER = new(unchecked((int) 0x80000001));
        private const int KEY_QUERY_VALUE = 0x0001;
        private const int KEY_SET_VALUE = 0x0002;
        private const int REG_SZ = 1;

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NotifyIconData
        {
            public int Size;
            public IntPtr Window;
            public uint Id;
            public uint Flags;
            public uint CallbackMessage;
            public IntPtr Icon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Tip;
            public uint State;
            public uint StateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Info;
            public uint TimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string InfoTitle;
            public uint InfoFlags;
            public Guid Item;
            public IntPtr BalloonIcon;
        }

        private delegate bool EnumWindowsProc(IntPtr window, IntPtr param);
        private delegate IntPtr WindowProc(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr param);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, System.Text.StringBuilder name, int capacity);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentProcessId();
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr window, int index, uint value);
        [DllImport("user32.dll")] private static extern uint GetWindowLong(IntPtr window, int index);
        [DllImport("user32.dll")] private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
        [DllImport("user32.dll")] private static extern IntPtr CallWindowProc(IntPtr previous, IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool SetLayeredWindowAttributes(IntPtr window, uint colorKey, byte alpha, uint flags);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr window);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rect rect);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint action, uint param, ref Rect rect, uint winIni);
        [DllImport("user32.dll")] private static extern IntPtr LoadIcon(IntPtr instance, IntPtr name);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr ExtractIcon(IntPtr instance, string file, int index);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "Shell_NotifyIconW")] private static extern bool Shell_NotifyIcon(uint message, ref NotifyIconData data);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string name);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern uint GetModuleFileName(IntPtr module, System.Text.StringBuilder path, int capacity);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW")] private static extern int RegOpenKeyEx(IntPtr key, string subKey, int options, int access, out IntPtr result);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW")] private static extern int RegQueryValueEx(IntPtr key, string name, IntPtr reserved, out int type, IntPtr data, ref int size);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW")] private static extern int RegSetValueEx(IntPtr key, string name, int reserved, int type, string data, int size);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegDeleteValueW")] private static extern int RegDeleteValue(IntPtr key, string name);
        [DllImport("advapi32.dll")] private static extern int RegCloseKey(IntPtr key);
#endif
    }
}
