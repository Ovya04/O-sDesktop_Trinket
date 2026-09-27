using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Trinket.Services;

/// <summary>
/// Wraps a global low-level mouse hook (WH_MOUSE_LL), giving purely passive
/// visibility into the cursor's screen position system-wide - never
/// intercepting or blocking any click or movement meant for other windows.
///
/// All Win32 interop for this hook is isolated here, matching the project's
/// convention of keeping raw P/Invoke code out of window/business logic.
/// The hook callback itself does the absolute minimum possible work (just
/// reads a point and raises an event) since it runs on a sensitive system
/// thread - see MouseMoved's usage in CharmOverlayWindow for where the
/// actual "is this near the charm" logic lives instead.
/// </summary>
public class MouseHookService : IDisposable
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;

    // Kept as a field (not a local) so the delegate is never garbage
    // collected while the hook is still installed - a classic pitfall with
    // native callbacks in managed code.
    private readonly LowLevelMouseProc _proc;
    private nint _hookHandle;

    public event Action<Point>? MouseMoved;

    public MouseHookService()
    {
        _proc = HookCallback;
    }

        public void Start()
    {
        if (_hookHandle != 0)
        {
            return;
        }

        // For a low-level hook whose callback lives in the same process
        // that's installing it (our case), hMod can simply be IntPtr.Zero -
        // this is the standard, reliable pattern. The previous approach of
        // resolving GetModuleHandle(currentModule.ModuleName) is a common
        // source of silent failure in modern .NET apps, since the module
        // name reported by Process.MainModule doesn't always match what
        // GetModuleHandle expects.
        _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _proc, nint.Zero, 0);

        if (_hookHandle == 0)
        {
            int errorCode = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine($"MouseHookService: SetWindowsHookEx failed, Win32 error {errorCode}");
        }
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam == WM_MOUSEMOVE)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            MouseMoved?.Invoke(new Point(hookStruct.pt.x, hookStruct.pt.y));
        }

        // ALWAYS pass the message along unmodified - this hook only observes,
        // it must never swallow or alter mouse input for other applications.
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookHandle != 0)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = 0;
        }
    }

    // --- Win32 interop ---

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public nint dwExtraInfo;
    }

    private delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    
}