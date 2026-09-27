using System.Drawing;
using System.Drawing.Drawing2D;

namespace Trinket.Services;

/// <summary>
/// Draws a small original tray icon programmatically (a cord + circular
/// charm) rather than requiring a pre-made .ico binary asset. Easy to swap
/// for real hand-designed artwork later without touching any calling code -
/// TrayService only ever calls CreateTrayIcon().
/// </summary>
public static class TrayIconFactory
{
    public static Icon CreateTrayIcon()
    {
        using var bitmap = new Bitmap(32, 32);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var cordPen = new Pen(Color.FromArgb(255, 180, 180, 180), 2f);
            g.DrawLine(cordPen, 16, 2, 16, 12);

            using var charmBrush = new SolidBrush(Color.FromArgb(255, 253, 243, 217));
            using var charmPen = new Pen(Color.FromArgb(255, 201, 184, 138), 1.5f);
            g.FillEllipse(charmBrush, 8, 12, 16, 16);
            g.DrawEllipse(charmPen, 8, 12, 16, 16);
        }

        // The resulting GDI icon handle is intentionally left undestroyed -
        // it lives for the app's entire lifetime alongside the single
        // NotifyIcon that uses it, and the OS reclaims it when the process
        // exits, so an explicit DestroyIcon call isn't necessary here.
        return Icon.FromHandle(bitmap.GetHicon());
    }
}