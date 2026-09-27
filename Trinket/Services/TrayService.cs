using System;
using System.Windows.Forms;
using Trinket.Models;

namespace Trinket.Services;

public class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _pauseMenuItem;
    private readonly ToolStripMenuItem[] _breezeMenuItems;
    private readonly ToolStripMenuItem[] _ambientBreezeMenuItems;
    private readonly ToolStripMenuItem _startupMenuItem;

    public event Action? OpenStudioRequested;
    public event Action? ResetRequested;
    public event Action? PauseToggleRequested;
    public event Action? ExitRequested;
    public event Action<BreezeLevel>? MouseBreezeChanged;
    public event Action<AmbientBreezeLevel>? AmbientBreezeChanged;
    public event Action<bool>? StartupToggleRequested;
    public event Action? AboutRequested;

    public TrayService()
    {
        var openStudioItem = new ToolStripMenuItem("Open Charm Studio");
        openStudioItem.Click += (_, _) => OpenStudioRequested?.Invoke();

        _pauseMenuItem = new ToolStripMenuItem("Pause Physics");
        _pauseMenuItem.Click += (_, _) => PauseToggleRequested?.Invoke();

        var resetItem = new ToolStripMenuItem("Reset Position");
        resetItem.Click += (_, _) => ResetRequested?.Invoke();

        var breezeOffItem = new ToolStripMenuItem("Off");
        var breezeGentleItem = new ToolStripMenuItem("Gentle");
        var breezeStrongItem = new ToolStripMenuItem("Strong");
        breezeOffItem.Click += (_, _) => MouseBreezeChanged?.Invoke(BreezeLevel.Off);
        breezeGentleItem.Click += (_, _) => MouseBreezeChanged?.Invoke(BreezeLevel.Gentle);
        breezeStrongItem.Click += (_, _) => MouseBreezeChanged?.Invoke(BreezeLevel.Strong);
        _breezeMenuItems = new[] { breezeOffItem, breezeGentleItem, breezeStrongItem };
        var breezeSubmenu = new ToolStripMenuItem("Mouse Breeze");
        breezeSubmenu.DropDownItems.AddRange(_breezeMenuItems);

        var ambientOffItem = new ToolStripMenuItem("Off");
        var ambientGentleItem = new ToolStripMenuItem("Gentle");
        var ambientBreezyItem = new ToolStripMenuItem("Breezy");
        ambientOffItem.Click += (_, _) => AmbientBreezeChanged?.Invoke(AmbientBreezeLevel.Off);
        ambientGentleItem.Click += (_, _) => AmbientBreezeChanged?.Invoke(AmbientBreezeLevel.Gentle);
        ambientBreezyItem.Click += (_, _) => AmbientBreezeChanged?.Invoke(AmbientBreezeLevel.Breezy);
        _ambientBreezeMenuItems = new[] { ambientOffItem, ambientGentleItem, ambientBreezyItem };
        var ambientSubmenu = new ToolStripMenuItem("Ambient Breeze");
        ambientSubmenu.DropDownItems.AddRange(_ambientBreezeMenuItems);

        _startupMenuItem = new ToolStripMenuItem("Launch at Startup");
        _startupMenuItem.Click += (_, _) => StartupToggleRequested?.Invoke(!_startupMenuItem.Checked);

        var aboutItem = new ToolStripMenuItem("About");
        aboutItem.Click += (_, _) => AboutRequested?.Invoke();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitRequested?.Invoke();

        var menu = new ContextMenuStrip();
        menu.Items.Add(openStudioItem);
        menu.Items.Add(_pauseMenuItem);
        menu.Items.Add(resetItem);
        menu.Items.Add(breezeSubmenu);
        menu.Items.Add(ambientSubmenu);
        menu.Items.Add(_startupMenuItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(aboutItem);
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = TrayIconFactory.CreateTrayIcon(),
            Text = "Trinket",
            Visible = true,
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => OpenStudioRequested?.Invoke();
    }

    public void SetPausedState(bool isPaused) => _pauseMenuItem.Checked = isPaused;

    public void SetMouseBreezeState(BreezeLevel level)
    {
        _breezeMenuItems[0].Checked = level == BreezeLevel.Off;
        _breezeMenuItems[1].Checked = level == BreezeLevel.Gentle;
        _breezeMenuItems[2].Checked = level == BreezeLevel.Strong;
    }

    public void SetAmbientBreezeState(AmbientBreezeLevel level)
    {
        _ambientBreezeMenuItems[0].Checked = level == AmbientBreezeLevel.Off;
        _ambientBreezeMenuItems[1].Checked = level == AmbientBreezeLevel.Gentle;
        _ambientBreezeMenuItems[2].Checked = level == AmbientBreezeLevel.Breezy;
    }

    public void SetStartupState(bool isEnabled) => _startupMenuItem.Checked = isEnabled;

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}