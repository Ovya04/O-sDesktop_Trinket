using System;
using System.Windows;
using System.Windows.Threading;
using Trinket.Models;
using Trinket.Overlay;
using Trinket.Services;
using Trinket.Studio;

namespace Trinket;

public partial class App : Application
{
    private TrayService? _trayService;
    private CharmOverlayWindow? _overlayWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch anything unhandled anywhere in the app rather than letting
        // it crash without explanation. UI-thread errors can be marked
        // "handled" and recovered from; background-thread errors cannot be
        // recovered from by .NET's design, but are still logged.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;

        AppSettings settings = SettingsService.Load();

        _overlayWindow = new CharmOverlayWindow(settings);
        MainWindow = _overlayWindow;
        _overlayWindow.Show();
        _overlayWindow.SettingsReset += OnSettingsReset;

        _trayService = new TrayService();
        _trayService.OpenStudioRequested += OnOpenStudioRequested;
        _trayService.ResetRequested += OnResetRequested;
        _trayService.PauseToggleRequested += OnPauseToggleRequested;
        _trayService.ExitRequested += OnExitRequested;
        _trayService.MouseBreezeChanged += OnMouseBreezeChanged;
        _trayService.AmbientBreezeChanged += OnAmbientBreezeChanged;
        _trayService.StartupToggleRequested += OnStartupToggleRequested;
        _trayService.AboutRequested += OnAboutRequested;

        _trayService.SetPausedState(_overlayWindow.IsPaused);
        _trayService.SetMouseBreezeState(settings.MouseBreeze);
        _trayService.SetAmbientBreezeState(settings.AmbientBreeze);
        _trayService.SetStartupState(StartupService.IsEnabled);
                SessionEnding += OnSessionEnding;
    }
        private void OnSessionEnding(object? sender, SessionEndingCancelEventArgs e)
    {
        if (_overlayWindow != null)
        {
            SettingsService.Save(_overlayWindow.GetCurrentSettings());
        }
    }

    private void OnOpenStudioRequested() => _overlayWindow?.OpenStudio();

    private void OnResetRequested() => _overlayWindow?.ResetPosition();

    private void OnPauseToggleRequested()
    {
        if (_overlayWindow == null) return;
        _overlayWindow.TogglePause();
        _trayService?.SetPausedState(_overlayWindow.IsPaused);
    }

    private void OnMouseBreezeChanged(BreezeLevel level)
    {
        _overlayWindow?.SetMouseBreeze(level);
        _trayService?.SetMouseBreezeState(level);
    }

    private void OnAmbientBreezeChanged(AmbientBreezeLevel level)
    {
        _overlayWindow?.SetAmbientBreeze(level);
        _trayService?.SetAmbientBreezeState(level);
    }

    private void OnStartupToggleRequested(bool enable)
    {
        StartupService.SetEnabled(enable);
        _trayService?.SetStartupState(StartupService.IsEnabled);
    }

            private void OnAboutRequested()
    {
        try
        {
            var aboutWindow = new AboutWindow { Topmost = true };
            aboutWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open About window: {ex}");
            MessageBox.Show(
                $"Couldn't open the About window:\n{ex.Message}",
                "Trinket", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _overlayWindow?.EnsureTopmost();
        }
    }

    private void OnSettingsReset()
    {
        if (_overlayWindow == null) return;

        _trayService?.SetPausedState(_overlayWindow.IsPaused);
        _trayService?.SetMouseBreezeState(_overlayWindow.CurrentMouseBreeze);
        _trayService?.SetAmbientBreezeState(_overlayWindow.CurrentAmbientBreeze);
    }

    private void OnExitRequested()
    {
        if (_overlayWindow != null)
        {
            SettingsService.Save(_overlayWindow.GetCurrentSettings());
        }

        _trayService?.Dispose();
        Shutdown();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Unhandled UI exception: {e.Exception}");

        MessageBox.Show(
            "Trinket ran into an unexpected problem and needs to recover. Your settings are safe.",
            "Trinket", MessageBoxButton.OK, MessageBoxImage.Warning);

        // Prevents the default behavior of crashing the whole app for an
        // isolated, likely-recoverable error on the UI thread.
        e.Handled = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Unhandled fatal exception: {e.ExceptionObject}");
        // Errors reaching here are on non-UI threads and cannot be marked
        // "handled" - the process will terminate regardless. Settings were
        // already saved on the last successful Exit, so at worst the user
        // loses only changes made since then.
    }
}