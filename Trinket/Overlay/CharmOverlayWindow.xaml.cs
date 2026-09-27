using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Trinket.Models;
using Trinket.Physics;
using Trinket.Reactions;
using Trinket.Rendering;
using Trinket.Services;
using Trinket.Studio;

namespace Trinket.Overlay;

public partial class CharmOverlayWindow : Window
{
    private const double HitTestPadding = 10;
    private const double MinVelocitySampleDt = 0.0005;
    private const double AnchorSideMargin = 150.0;
    private static readonly int[] BeadNodeIndices = { 2, 4 };

    private readonly AppSettings _settings;
    private readonly PhysicsSettings _physicsSettings = new();
    private RopeSimulation _rope = null!;

    private MonitorInfo _currentMonitor = null!;
    private double _dpiScale = 1.0;
    private nint _hwnd;

    private CordStyle _currentCordStyle;
    private Color _currentCordColor;
    private double _currentRopeLength;
    private string _currentCharmId;
    private double _currentCharmSize;
    private string _currentInitialLetter;
    private BeadShape _currentBeadShape;
    private AnchorPreset _currentAnchorPreset;
       private Color? _currentCharmColorOverride;
    private CharmDefinition _currentCharmDefinition = null!;

    private FrameworkElement _charmVisual = null!;
    private readonly List<Path> _beadVisuals = new();
    private CordVisualHandle _cordVisual = null!;

    private readonly List<CharmDefinition> _allCharms;
    private readonly List<TrinketDefinition> _trinkets;

    private readonly Stopwatch _clock = new();
    private double _lastRenderTimeSeconds;
    private double _accumulator;

    private Point _lastDragMousePosition;
    private double _lastDragMouseTimeSeconds;
    private Point _mouseDownPosition;

    private bool _isAsleep;
    private double _quietSeconds;

    private bool _isPaused;
    public bool IsPaused => _isPaused;

    private CharmStudioWindow? _studioWindow;

    private readonly MouseHookService _mouseHook = new();
    private BreezeLevel _currentMouseBreeze;
    private Point? _lastGlobalMousePosition;
    private double _lastGlobalMouseTimeSeconds;
    private double _lastBreezeAppliedTimeSeconds;

    private readonly AmbientBreezeScheduler _ambientBreezeScheduler;
    private AmbientBreezeLevel _currentAmbientBreeze;
    private DispatcherTimer? _ambientBreezeTimer;

    public event Action? SettingsReset;
    public BreezeLevel CurrentMouseBreeze => _currentMouseBreeze;
    public AmbientBreezeLevel CurrentAmbientBreeze => _currentAmbientBreeze;

    public CharmOverlayWindow(AppSettings settings)
    {
        InitializeComponent();

        _settings = settings;
        _isPaused = settings.IsPaused;

        _currentCordStyle = settings.CordStyle;
        _currentCordColor = (Color)ColorConverter.ConvertFromString(settings.CordColorHex);
        _currentRopeLength = settings.RopeLength;
        _currentCharmId = settings.CharmId;
        _currentCharmSize = settings.CharmSize;
        _currentInitialLetter = string.IsNullOrEmpty(settings.InitialLetter) ? "T" : settings.InitialLetter;
        _currentBeadShape = settings.BeadShape;
        _currentAnchorPreset = settings.AnchorPreset;

        _currentMouseBreeze = settings.MouseBreeze;
        _currentAmbientBreeze = settings.AmbientBreeze;
        _ambientBreezeScheduler = new AmbientBreezeScheduler(_physicsSettings);
             _currentCharmColorOverride = string.IsNullOrEmpty(settings.CharmColorHex)
            ? null
            : (Color)ColorConverter.ConvertFromString(settings.CharmColorHex);
        _allCharms = CharmRegistry.All.ToList();
        foreach (ImportedCharmMetadata metadata in AssetService.LoadMetadata())
        {
            _allCharms.Add(new CharmDefinition
            {
                Id = metadata.Id,
                Name = metadata.Name,
                Shape = CharmShape.Custom,
                Size = settings.CharmSize,
                ImagePath = AssetService.GetFullPath(metadata.FileName),
                AttachmentPoint = new Point(metadata.AttachmentX, metadata.AttachmentY)
            });
        }

        _trinkets = TrinketService.Load();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _dpiScale = VisualTreeHelper.GetDpi(this).DpiScaleX;

        _currentMonitor = MonitorService.ResolveMonitor(_settings.MonitorDeviceName);
        ApplyMonitorBounds(_currentMonitor);

        var anchor = new Point(ComputeAnchorX(_currentAnchorPreset), _settings.AnchorTopMargin);
        _rope = new RopeSimulation(anchor, _currentRopeLength, _physicsSettings);

        RebuildCord();
        RebuildCharmVisual();
        RebuildBeads();

        UpdateVisualPosition();

        _clock.Start();
        _lastRenderTimeSeconds = _clock.Elapsed.TotalSeconds;
        CompositionTarget.Rendering += OnRendering;

        var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        hwndSource.AddHook(WndProc);
        _hwnd = hwndSource.Handle;
        EnsureTopmost();

        _mouseHook.MouseMoved += OnGlobalMouseMoved;
        _mouseHook.Start();

        _ambientBreezeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _ambientBreezeTimer.Tick += (_, _) =>
        {
            CheckAmbientBreeze();
            EnsureTopmost();
        };
        _ambientBreezeTimer.Start();
    }

    private void CharmOverlayWindow_Closed(object? sender, EventArgs e)
    {
        _mouseHook.Dispose();
        _ambientBreezeTimer?.Stop();
    }

    public void EnsureTopmost()
    {
        if (_hwnd == 0) return;
        Win32Interop.SetWindowPos(_hwnd, Win32Interop.HWND_TOPMOST, 0, 0, 0, 0,
            Win32Interop.SWP_NOMOVE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE);
    }

    private double ComputeAnchorX(AnchorPreset preset) => preset switch
    {
        AnchorPreset.TopLeft => AnchorSideMargin,
        AnchorPreset.TopCenter => Width / 2,
        AnchorPreset.TopRight => Width - AnchorSideMargin,
        _ => Width - AnchorSideMargin
    };

    private void ApplyMonitorBounds(MonitorInfo monitor)
    {
        Left = monitor.X / _dpiScale;
        Top = monitor.Y / _dpiScale;
        Width = monitor.Width / _dpiScale;
        Height = monitor.Height / _dpiScale;
    }

    private void PersistSettings()
    {
        SettingsService.Save(GetCurrentSettings());
    }

    // --- Tray-triggered actions ---

    public void OpenStudio()
    {
        if (_studioWindow == null || !_studioWindow.IsVisible)
        {
            _studioWindow = new CharmStudioWindow(GetCurrentSettings(), _allCharms, _trinkets);
            _studioWindow.CordStyleChanged += SetCordStyle;
            _studioWindow.CordColorChanged += SetCordColor;
            _studioWindow.RopeLengthChanged += SetRopeLength;
            _studioWindow.CharmSelected += SetCharm;
            _studioWindow.CharmSizeChanged += SetCharmSize;
                        _studioWindow.CharmColorChanged += SetCharmColor;
            _studioWindow.CharmColorResetRequested += ResetCharmColor;
            _studioWindow.InitialLetterChanged += SetInitialLetter;
            _studioWindow.BeadShapeChanged += SetBeadShape;
            _studioWindow.AnchorPresetChanged += SetAnchorPreset;
            _studioWindow.MonitorSelected += SetMonitor;
            _studioWindow.SaveTrinketRequested += OnSaveTrinketRequested;
            _studioWindow.TrinketApplyRequested += OnTrinketApplyRequested;
            _studioWindow.TrinketDeleteRequested += OnTrinketDeleteRequested;
            _studioWindow.CharmDeleteRequested += OnCharmDeleteRequested;
            _studioWindow.ResetToDefaultsRequested += ResetToDefaults;
            _studioWindow.Show();
            _studioWindow.Closed += (_, _) => { _studioWindow = null; EnsureTopmost(); };
        }
        else
        {
            _studioWindow.Activate();
        }
    }

    public void ResetPosition()
    {
        _rope.Reset();
        WakeUp();
        UpdateVisualPosition();
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        if (!_isPaused) WakeUp();
        PersistSettings();
    }

    // --- Studio-driven live updates ---

    public void SetCordStyle(CordStyle style)
    {
        _currentCordStyle = style;
        RebuildCord();
        WakeUp();
    }

    public void SetCordColor(Color color)
    {
        _currentCordColor = color;
        RebuildCord();
        WakeUp();
    }

    private void RebuildCord()
    {
        RopeRenderer.ApplyBackboneStyle(RopePolyline, _currentCordStyle, _currentCordColor);
        double segmentLength = _currentRopeLength / (_rope.Nodes.Count - 1);
        _cordVisual = RopeRenderer.BuildCordVisual(CordDecorationCanvas, _currentCordStyle, _currentCordColor, _rope.Nodes.Count, segmentLength);
        PersistSettings();
    }

    public void SetRopeLength(double length)
    {
        _currentRopeLength = length;
        _rope.SetLength(length);
        WakeUp();
        PersistSettings();
    }

        public void SetCharm(string charmId)
    {
        _currentCharmId = charmId;
        _currentCharmColorOverride = null; // switching charms resets to that charm's default color
        RebuildCharmVisual();
    }

    public void SetCharmSize(double size)
    {
        _currentCharmSize = size;
        RebuildCharmVisual();
    }
        public void SetCharmColor(Color color)
    {
        _currentCharmColorOverride = color;
        RebuildCharmVisual();
    }

    public void ResetCharmColor()
    {
        _currentCharmColorOverride = null;
        RebuildCharmVisual();
    }

    public void SetInitialLetter(string letter)
    {
        _currentInitialLetter = string.IsNullOrEmpty(letter) ? "T" : letter.Substring(0, 1).ToUpperInvariant();
        RebuildCharmVisual();
    }

    public void SetBeadShape(BeadShape shape)
    {
        _currentBeadShape = shape;
        RebuildBeads();
    }

    public void SetAnchorPreset(AnchorPreset preset)
    {
        _currentAnchorPreset = preset;
        _rope.Anchor = new Point(ComputeAnchorX(preset), _settings.AnchorTopMargin);
        WakeUp();
        PersistSettings();
    }

    public void SetMonitor(string deviceName)
    {
        _currentMonitor = MonitorService.ResolveMonitor(deviceName);
        ApplyMonitorBounds(_currentMonitor);
        _rope.Anchor = new Point(ComputeAnchorX(_currentAnchorPreset), _settings.AnchorTopMargin);
        _rope.Reset();
        WakeUp();
        PersistSettings();
    }

    public void SetMouseBreeze(BreezeLevel level)
    {
        _currentMouseBreeze = level;
        PersistSettings();
    }

    public void SetAmbientBreeze(AmbientBreezeLevel level)
    {
        _currentAmbientBreeze = level;
        PersistSettings();
    }

    private void RebuildCharmVisual()
    {
        if (_charmVisual != null)
        {
            OverlayCanvas.Children.Remove(_charmVisual);
        }

        CharmDefinition definition = ResolveCharmDefinition(_currentCharmId, _currentCharmSize);
        _currentCharmDefinition = definition;

        _charmVisual = CharmRenderer.CreateCharmVisual(definition);
        _charmVisual.Cursor = Cursors.Hand;
        _charmVisual.MouseLeftButtonDown += CharmVisual_MouseLeftButtonDown;
        _charmVisual.MouseMove += CharmVisual_MouseMove;
        _charmVisual.MouseLeftButtonUp += CharmVisual_MouseLeftButtonUp;
        OverlayCanvas.Children.Add(_charmVisual);

        WakeUp();
        UpdateVisualPosition();
        PersistSettings();
    }

    private CharmDefinition ResolveCharmDefinition(string id, double size)
    {
        CharmDefinition baseDefinition = _allCharms.FirstOrDefault(c => c.Id == id) ?? _allCharms[0];

                return new CharmDefinition
        {
            Id = baseDefinition.Id,
            Name = baseDefinition.Name,
            Shape = baseDefinition.Shape,
            Size = size,
            FillColor = _currentCharmColorOverride ?? baseDefinition.FillColor,
            StrokeColor = _currentCharmColorOverride.HasValue
                ? Color.FromArgb(255, (byte)(_currentCharmColorOverride.Value.R * 0.75), (byte)(_currentCharmColorOverride.Value.G * 0.75), (byte)(_currentCharmColorOverride.Value.B * 0.75))
                : baseDefinition.StrokeColor,
            Reaction = baseDefinition.Reaction,
            ImagePath = baseDefinition.ImagePath,
            AttachmentPoint = baseDefinition.AttachmentPoint,
            InitialLetter = baseDefinition.Shape == CharmShape.Initial ? _currentInitialLetter : baseDefinition.InitialLetter
        };
    }

    private void RebuildBeads()
    {
        foreach (Path visual in _beadVisuals)
        {
            OverlayCanvas.Children.Remove(visual);
        }
        _beadVisuals.Clear();

        if (_currentBeadShape != BeadShape.None)
        {
            foreach (int nodeIndex in BeadNodeIndices)
            {
                var beadDef = new BeadDefinition
                {
                    Id = $"bead-{nodeIndex}",
                    Shape = _currentBeadShape,
                    Size = 16,
                    FillColor = Color.FromRgb(0xF5, 0xF0, 0xE6),
                    StrokeColor = Color.FromRgb(0xD8, 0xCE, 0xB8)
                };

                Path visual = BeadRenderer.CreateBeadVisual(beadDef);
                OverlayCanvas.Children.Add(visual);
                _beadVisuals.Add(visual);
            }
        }

        WakeUp();
        UpdateVisualPosition();
        PersistSettings();
    }

    private void OnCharmDeleteRequested(CharmDefinition charmToDelete)
    {
        var importedList = AssetService.LoadMetadata();
        importedList.RemoveAll(m => m.Id == charmToDelete.Id);
        AssetService.SaveMetadata(importedList);

        if (charmToDelete.ImagePath != null)
        {
            AssetService.DeleteFile(charmToDelete.ImagePath);
        }

        _allCharms.RemoveAll(c => c.Id == charmToDelete.Id);

        if (_currentCharmId == charmToDelete.Id)
        {
            SetCharm(_allCharms[0].Id);
        }

        _studioWindow?.RefreshCharmCombo(_allCharms.FirstOrDefault(c => c.Id == _currentCharmId) ?? _allCharms[0]);
    }

    // --- Trinket presets ---

    private void OnSaveTrinketRequested()
    {
        var nameWindow = new NameInputWindow { Owner = _studioWindow };
        bool? result = nameWindow.ShowDialog();
        if (result != true || !nameWindow.Confirmed) return;

        var trinket = new TrinketDefinition
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = nameWindow.EnteredName,
            CharmId = _currentCharmId,
            CharmSize = _currentCharmSize,
            CordStyle = _currentCordStyle,
            CordColorHex = _currentCordColor.ToString(),
            RopeLength = _currentRopeLength,
            BeadShape = _currentBeadShape,
            AnchorPreset = _currentAnchorPreset
        };

        _trinkets.Add(trinket);
        TrinketService.Save(_trinkets);
        _studioWindow?.RefreshTrinketsCombo(trinket);
    }

    private void OnTrinketApplyRequested(TrinketDefinition trinket) => ApplyTrinket(trinket);

    private void OnTrinketDeleteRequested(TrinketDefinition trinket)
    {
        _trinkets.Remove(trinket);
        TrinketService.Save(_trinkets);
        _studioWindow?.RefreshTrinketsCombo();
    }

    public void ApplyTrinket(TrinketDefinition trinket)
    {
        _currentCharmId = trinket.CharmId;
        _currentCharmSize = trinket.CharmSize;
        _currentCordStyle = trinket.CordStyle;
        _currentCordColor = (Color)ColorConverter.ConvertFromString(trinket.CordColorHex);
        _currentRopeLength = trinket.RopeLength;
        _currentBeadShape = trinket.BeadShape;
        _currentAnchorPreset = trinket.AnchorPreset;

        RebuildCord();
        _rope.SetLength(_currentRopeLength);
        _rope.Anchor = new Point(ComputeAnchorX(_currentAnchorPreset), _settings.AnchorTopMargin);

        RebuildCharmVisual();
        RebuildBeads();

        WakeUp();
        PersistSettings();
    }

    public void ResetToDefaults()
    {
        var defaults = new AppSettings();

        _isPaused = false;
        _currentMouseBreeze = defaults.MouseBreeze;
        _currentAmbientBreeze = defaults.AmbientBreeze;
        _currentInitialLetter = defaults.InitialLetter;

        ApplyTrinket(new TrinketDefinition
        {
            CharmId = defaults.CharmId,
            CharmSize = defaults.CharmSize,
            CordStyle = defaults.CordStyle,
            CordColorHex = defaults.CordColorHex,
            RopeLength = defaults.RopeLength,
            BeadShape = defaults.BeadShape,
            AnchorPreset = defaults.AnchorPreset
        });

        PersistSettings();
        _studioWindow?.RefreshFromSettings(GetCurrentSettings());
        SettingsReset?.Invoke();
    }

    public AppSettings GetCurrentSettings()
    {
        return new AppSettings
        {
            IsPaused = _isPaused,
            RopeLength = _currentRopeLength,
            AnchorPreset = _currentAnchorPreset,
            AnchorTopMargin = _settings.AnchorTopMargin,
            CharmId = _currentCharmId,
            CharmSize = _currentCharmSize,
            InitialLetter = _currentInitialLetter,
            
            CordStyle = _currentCordStyle,
            CordColorHex = _currentCordColor.ToString(),
            BeadShape = _currentBeadShape,
            MouseBreeze = _currentMouseBreeze,
            AmbientBreeze = _currentAmbientBreeze,
            MonitorDeviceName = _currentMonitor.DeviceName
        };
    }

    // --- Render/physics loop ---

    private void OnRendering(object? sender, EventArgs e)
    {
        double now = _clock.Elapsed.TotalSeconds;
        double frameDelta = now - _lastRenderTimeSeconds;
        _lastRenderTimeSeconds = now;

        frameDelta = Math.Min(frameDelta, _physicsSettings.MaxFrameDelta);
        _accumulator += frameDelta;

        while (_accumulator >= _physicsSettings.FixedTimeStep)
        {
            if (!_isPaused || _rope.IsDragging)
            {
                _rope.Step(_physicsSettings.FixedTimeStep);
            }
            _accumulator -= _physicsSettings.FixedTimeStep;
        }

        UpdateVisualPosition();

        if (!_isPaused)
        {
            UpdateSleepState(frameDelta);
        }
    }

    private void UpdateSleepState(double frameDelta)
    {
        if (_rope.IsDragging)
        {
            _quietSeconds = 0;
            return;
        }

        double maxSpeed = _rope.GetMaxNodeSpeed(_physicsSettings.FixedTimeStep);

        if (maxSpeed < _physicsSettings.SleepVelocityThreshold)
        {
            _quietSeconds += frameDelta;
            if (_quietSeconds >= _physicsSettings.SleepDelaySeconds)
            {
                GoToSleep();
            }
        }
        else
        {
            _quietSeconds = 0;
        }
    }

    private void GoToSleep()
    {
        if (_isAsleep) return;
        _isAsleep = true;
        CompositionTarget.Rendering -= OnRendering;
    }

    private void WakeUp()
    {
        if (!_isAsleep) return;
        _isAsleep = false;
        _quietSeconds = 0;
        _lastRenderTimeSeconds = _clock.Elapsed.TotalSeconds;
        _accumulator = 0;
        CompositionTarget.Rendering += OnRendering;
    }

    private void UpdateVisualPosition()
    {
        RopePolyline.Points.Clear();
        foreach (RopeNode node in _rope.Nodes)
        {
            RopePolyline.Points.Add(node.CurrentPosition);
        }

        if (_cordVisual != null)
        {
            RopeRenderer.UpdateCordVisual(_cordVisual, _rope.Nodes);
        }

        Point charmCenter = _rope.CharmNode.CurrentPosition;
        Point attachmentPoint = _currentCharmDefinition.AttachmentPoint;
        Canvas.SetLeft(_charmVisual, charmCenter.X - _charmVisual.Width * attachmentPoint.X);
        Canvas.SetTop(_charmVisual, charmCenter.Y - _charmVisual.Height * attachmentPoint.Y);

        Point secondToLastNode = _rope.Nodes[_rope.Nodes.Count - 2].CurrentPosition;
        CharmRenderer.UpdateRotation(_charmVisual, secondToLastNode, charmCenter);

        for (int i = 0; i < _beadVisuals.Count; i++)
        {
            int nodeIndex = BeadNodeIndices[i];
            Point nodePosition = _rope.Nodes[nodeIndex].CurrentPosition;
            Path beadVisual = _beadVisuals[i];
            Canvas.SetLeft(beadVisual, nodePosition.X - beadVisual.Width / 2);
            Canvas.SetTop(beadVisual, nodePosition.Y - beadVisual.Height / 2);
        }
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == Win32Interop.WM_NCHITTEST)
        {
            int x = unchecked((short)(long)lParam);
            int y = unchecked((short)((long)lParam >> 16));
            var screenPoint = new Point(x, y);
            Point windowPoint = PointFromScreen(screenPoint);

            handled = true;
            return IsOverCharm(windowPoint) ? Win32Interop.HTCLIENT : Win32Interop.HTTRANSPARENT;
        }
        return 0;
    }

    private bool IsOverCharm(Point point)
    {
        double left = Canvas.GetLeft(_charmVisual) - HitTestPadding;
        double top = Canvas.GetTop(_charmVisual) - HitTestPadding;
        double right = left + _charmVisual.Width + HitTestPadding * 2;
        double bottom = top + _charmVisual.Height + HitTestPadding * 2;
        return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
    }

    // --- Dragging ---

    private void CharmVisual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        WakeUp();
        _rope.BeginDrag();
        _mouseDownPosition = e.GetPosition(OverlayCanvas);
        _lastDragMousePosition = _mouseDownPosition;
        _lastDragMouseTimeSeconds = _clock.Elapsed.TotalSeconds;
        Mouse.Capture(_charmVisual);
    }

    private void CharmVisual_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_rope.IsDragging) return;

        Point currentPos = e.GetPosition(OverlayCanvas);
        double now = _clock.Elapsed.TotalSeconds;
        double dt = now - _lastDragMouseTimeSeconds;

        Vector velocity = default;
        if (dt > MinVelocitySampleDt)
        {
            velocity = new Vector(
                (currentPos.X - _lastDragMousePosition.X) / dt,
                (currentPos.Y - _lastDragMousePosition.Y) / dt);
        }

        _lastDragMousePosition = currentPos;
        _lastDragMouseTimeSeconds = now;
        _rope.SetDragTarget(currentPos, velocity);
    }

    private void CharmVisual_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Point releasePosition = e.GetPosition(OverlayCanvas);
        double distanceMoved = (releasePosition - _mouseDownPosition).Length;

        _rope.EndDrag();
        Mouse.Capture(null);

        const double clickMovementThreshold = 6.0;
        if (distanceMoved < clickMovementThreshold)
        {
            PlayCharmReaction();
        }
    }

    private void PlayCharmReaction()
    {
        ICharmReaction? reaction = CharmReactionFactory.GetReaction(_currentCharmDefinition.Reaction);
        if (reaction == null) return;

        Point charmCenter = _rope.CharmNode.CurrentPosition;
        reaction.Play(_charmVisual, OverlayCanvas, charmCenter);
    }

    // --- Mouse Breeze ---

    private void OnGlobalMouseMoved(Point rawScreenPoint)
    {
        if (_currentMouseBreeze == BreezeLevel.Off) return;

        Point windowPoint = PointFromScreen(rawScreenPoint);
        double now = _clock.Elapsed.TotalSeconds;

        if (_lastGlobalMousePosition.HasValue)
        {
            double dt = now - _lastGlobalMouseTimeSeconds;
            if (dt > MinVelocitySampleDt)
            {
                Vector cursorVelocity = (windowPoint - _lastGlobalMousePosition.Value) / dt;
                ApplyMouseBreezeIfDue(windowPoint, cursorVelocity, now);
            }
        }

        _lastGlobalMousePosition = windowPoint;
        _lastGlobalMouseTimeSeconds = now;
    }

    private void ApplyMouseBreezeIfDue(Point cursorScreenPoint, Vector cursorVelocity, double now)
    {
        if (_currentMouseBreeze == BreezeLevel.Off || _rope == null) return;
        if (now - _lastBreezeAppliedTimeSeconds < _physicsSettings.MouseBreezeCooldownSeconds) return;

        double cursorSpeed = cursorVelocity.Length;
        if (cursorSpeed < _physicsSettings.MouseBreezeMinCursorSpeed) return;

        Point charmPosition = _rope.CharmNode.CurrentPosition;
        double distance = (cursorScreenPoint - charmPosition).Length;
        if (distance > _physicsSettings.MouseBreezeRadius) return;

        double strengthFactor = _currentMouseBreeze == BreezeLevel.Strong
            ? _physicsSettings.MouseBreezeStrongStrength
            : _physicsSettings.MouseBreezeGentleStrength;

        Vector impulse = cursorVelocity * strengthFactor * _physicsSettings.FixedTimeStep;
        _rope.ApplyImpulseToCharm(impulse);
        _lastBreezeAppliedTimeSeconds = now;
        WakeUp();
    }

    // --- Ambient Breeze ---

    private void CheckAmbientBreeze()
    {
        if (_currentAmbientBreeze == AmbientBreezeLevel.Off || _isPaused || _rope == null) return;

        double now = _clock.Elapsed.TotalSeconds;
        if (_ambientBreezeScheduler.TryGetGust(now, _currentAmbientBreeze, out Vector impulse))
        {
            _rope.ApplyImpulseToCharm(impulse);
            WakeUp();
        }
    }
}