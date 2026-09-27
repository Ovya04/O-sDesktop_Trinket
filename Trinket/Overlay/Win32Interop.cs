using System.Runtime.InteropServices; // add to top of file

namespace Trinket.Overlay;
    
/// <summary>
/// Win32 constants used to implement manual mouse hit-testing on the overlay window.
/// Kept isolated here so raw Win32 message codes don't leak into window/business logic
/// elsewhere in the app.
/// </summary>
internal static class Win32Interop
{
    // Windows message sent continuously, asking "what part of the window is
    // under the cursor right now?" (title bar, resize border, client area, etc.).
    public const int WM_NCHITTEST = 0x0084;

    // Answer meaning "this is ordinary clickable client area" - normal mouse
    // events (click, move, drag) are delivered to our window as usual.
    public const nint HTCLIENT = 1;

    // Answer meaning "there is nothing here" - Windows responds by forwarding
    // the mouse message to whatever window is underneath ours in z-order,
    // instead of delivering it to us. This is what makes the invisible parts
    // of the overlay click-through.
    public const nint HTTRANSPARENT = -1;

// --- Topmost enforcement ---
    // Topmost="True" set purely via WPF/XAML can silently stop being honored
    // after certain window-activation sequences (e.g. opening/closing other
    // windows like Charm Studio or About). Explicitly re-asserting via
    // SetWindowPos is the standard, robust fix used by real always-on-top
    // utilities - it directly tells the OS "keep this above everything
    // non-topmost," bypassing whatever caused WPF's own flag to lapse.

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOACTIVATE = 0x0010;
    public static readonly nint HWND_TOPMOST = new nint(-1);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags); }