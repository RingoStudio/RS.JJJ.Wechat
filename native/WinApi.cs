using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Snail.JJJ.Wechat.native
{
    internal class WinApi
    {
        [DllImport("Kernel32.dll")]
        //LPVOID VirtualAllocEx(
        //  HANDLE hProcess,
        //  LPVOID lpAddress,
        //  SIZE_T dwSize,
        //  DWORD flAllocationType,
        //  DWORD flProtect
        //);
        public static extern int VirtualAllocEx(int hProcess, int lpAddress, int dwSize, int flAllocationType, int flProtect);

        [DllImport("Kernel32.dll")]
        //BOOL WriteProcessMemory(
        //  HANDLE hProcess,
        //  LPVOID lpBaseAddress,
        //  LPCVOID lpBuffer,
        //  SIZE_T nSize,
        //  SIZE_T* lpNumberOfBytesWritten
        //);
        public static extern Boolean WriteProcessMemory(int hProcess, int lpBaseAddress, String lpBuffer, int nSize, int lpNumberOfBytesWritten);

        [DllImport("Kernel32.dll")]
        //HMODULE GetModuleHandleA(
        //  LPCSTR lpModuleName
        //);
        public static extern int GetModuleHandleA(String lpModuleName);

        [DllImport("Kernel32.dll")]
        //FARPROC GetProcAddress(
        //  HMODULE hModule,
        //  LPCSTR lpProcName
        //);
        public static extern int GetProcAddress(int hModule, String lpProcName);

        [DllImport("Kernel32.dll")]
        //HANDLE CreateRemoteThread(
        //  HANDLE hProcess,
        //  LPSECURITY_ATTRIBUTES lpThreadAttributes,
        //  SIZE_T dwStackSize,
        //  LPTHREAD_START_ROUTINE lpStartAddress,
        //  LPVOID lpParameter,
        //  DWORD dwCreationFlags,
        //  LPDWORD lpThreadId
        //);
        public static extern int CreateRemoteThread(int hProcess, int lpThreadAttributes, int dwStackSize, int lpStartAddress, int lpParameter, int dwCreationFlags, int lpThreadId);


        [DllImport("Kernel32.dll")]
        //BOOL VirtualFreeEx(
        //  HANDLE hProcess,
        //  LPVOID lpAddress,
        //  SIZE_T dwSize,
        //  DWORD dwFreeType
        //);
        public static extern Boolean VirtualFreeEx(int hProcess, int lpAddress, int dwSize, int dwFreeType);
        /// <summary>
        /// 默认的查找窗口的过滤条件。可见 + 非最小化 + 包含窗口标题。
        /// </summary>
        private static readonly Predicate<WindowInfo> DefaultPredicate = x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;

        private delegate bool WndEnumProc(IntPtr hWnd, int lParam);

        [DllImport("user32")]
        private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

        [DllImport("user32")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct LPRECT
    {
        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;
    }

    /// <summary>
    /// 获取 Win32 窗口的一些基本信息
    /// </summary>
    internal readonly struct WindowInfo
    {
        public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, Rectangle bounds) : this()
        {
            Hwnd = hWnd;
            ClassName = className;
            Title = title;
            IsVisible = isVisible;
            Bounds = bounds;
        }

        /// <summary>
        /// 获取窗口句柄。
        /// </summary>
        public IntPtr Hwnd { get; }

        /// <summary>
        /// 获取窗口类名。
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// 获取窗口标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 获取当前窗口是否可见。
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// 获取窗口当前的位置和尺寸。
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// 获取窗口当前是否是最小化的。
        /// </summary>
        public bool IsMinimized => Bounds.Left == -32000 && Bounds.Top == -32000;
    }
}
