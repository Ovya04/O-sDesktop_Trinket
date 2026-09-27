using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Trinket.Services;

/// <summary>Plain description of one connected monitor, independent of any Win32/WinForms types leaking elsewhere in the app.</summary>
public record MonitorInfo(string DeviceName, bool IsPrimary, int X, int Y, int Width, int Height);

/// <summary>
/// Detects connected monitors via System.Windows.Forms.Screen (already
/// available since UseWindowsForms is enabled for the tray icon). Bounds
/// are reported in physical pixels - CharmOverlayWindow is responsible for
/// converting to DIPs using the app's DPI scale factor.
/// </summary>
public static class MonitorService
{
    public static List<MonitorInfo> GetMonitors()
    {
        return Screen.AllScreens
            .Select(s => new MonitorInfo(s.DeviceName, s.Primary, s.Bounds.X, s.Bounds.Y, s.Bounds.Width, s.Bounds.Height))
            .ToList();
    }

    /// <summary>
    /// Resolves a saved monitor selection by device name. Falls back safely
    /// to the primary monitor if the saved device name is missing/null, or
    /// no longer present (e.g. that monitor was disconnected since the app
    /// last ran) - the app should never fail to start over a missing monitor.
    /// </summary>
    public static MonitorInfo ResolveMonitor(string? savedDeviceName)
    {
        List<MonitorInfo> monitors = GetMonitors();

        if (!string.IsNullOrEmpty(savedDeviceName))
        {
            MonitorInfo? match = monitors.FirstOrDefault(m => m.DeviceName == savedDeviceName);
            if (match != null)
            {
                return match;
            }
        }

        return monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors[0];
    }
}