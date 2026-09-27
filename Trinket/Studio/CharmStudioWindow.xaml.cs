using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using Trinket.Models;
using Trinket.Services;

namespace Trinket.Studio;

public partial class CharmStudioWindow : Window
{
    private bool _isInitializing = true;

    private readonly List<CharmDefinition> _allCharms;
    private readonly List<TrinketDefinition> _trinkets;

    public event Action<CordStyle>? CordStyleChanged;
    public event Action<Color>? CordColorChanged;
    public event Action<double>? RopeLengthChanged;
    public event Action<string>? CharmSelected;
    public event Action<double>? CharmSizeChanged;
    public event Action<string>? InitialLetterChanged;
    public event Action<BeadShape>? BeadShapeChanged;
    public event Action<AnchorPreset>? AnchorPresetChanged;
    public event Action<string>? MonitorSelected;

    public event Action? SaveTrinketRequested;
    public event Action<TrinketDefinition>? TrinketApplyRequested;
    public event Action<TrinketDefinition>? TrinketDeleteRequested;
    public event Action<CharmDefinition>? CharmDeleteRequested;

    public event Action? ResetToDefaultsRequested;

    public CharmStudioWindow(AppSettings currentSettings, List<CharmDefinition> allCharms, List<TrinketDefinition> trinkets)
    {
        InitializeComponent();

        _allCharms = allCharms;
        _trinkets = trinkets;

        TrinketsCombo.ItemsSource = _trinkets;

        CharmCombo.ItemsSource = _allCharms;
        CharmCombo.SelectedItem = _allCharms.FirstOrDefault(c => c.Id == currentSettings.CharmId) ?? _allCharms[0];
        CharmSizeSlider.Value = currentSettings.CharmSize;
        InitialLetterTextBox.Text = string.IsNullOrEmpty(currentSettings.InitialLetter) ? "T" : currentSettings.InitialLetter;

        CordStyleCombo.SelectedIndex = (int)currentSettings.CordStyle;
        CordLengthSlider.Value = currentSettings.RopeLength;

        BeadsCombo.SelectedIndex = (int)currentSettings.BeadShape;

        AnchorCombo.SelectedIndex = (int)currentSettings.AnchorPreset;

        List<MonitorInfo> monitors = MonitorService.GetMonitors();
        MonitorInfo resolvedCurrent = MonitorService.ResolveMonitor(currentSettings.MonitorDeviceName);

        for (int i = 0; i < monitors.Count; i++)
        {
            MonitorInfo monitor = monitors[i];
            string label = monitor.IsPrimary ? "Primary Monitor" : $"Monitor {i + 1}";
            var item = new ComboBoxItem { Content = label, Tag = monitor.DeviceName };
            MonitorCombo.Items.Add(item);
            if (monitor.DeviceName == resolvedCurrent.DeviceName)
            {
                MonitorCombo.SelectedItem = item;
            }
        }

        _isInitializing = false;
    }

    public void RefreshTrinketsCombo(TrinketDefinition? selectTrinket = null)
    {
        TrinketsCombo.ItemsSource = null;
        TrinketsCombo.ItemsSource = _trinkets;
        if (selectTrinket != null) TrinketsCombo.SelectedItem = selectTrinket;
    }

    public void RefreshCharmCombo(CharmDefinition selectCharm)
    {
        CharmCombo.ItemsSource = null;
        CharmCombo.ItemsSource = _allCharms;
        _isInitializing = true;
        CharmCombo.SelectedItem = selectCharm;
        _isInitializing = false;
    }

    public void RefreshFromSettings(AppSettings settings)
    {
        _isInitializing = true;
        CharmCombo.SelectedItem = _allCharms.FirstOrDefault(c => c.Id == settings.CharmId) ?? _allCharms[0];
        CharmSizeSlider.Value = settings.CharmSize;
        InitialLetterTextBox.Text = string.IsNullOrEmpty(settings.InitialLetter) ? "T" : settings.InitialLetter;
        CordStyleCombo.SelectedIndex = (int)settings.CordStyle;
        CordLengthSlider.Value = settings.RopeLength;
        BeadsCombo.SelectedIndex = (int)settings.BeadShape;
        AnchorCombo.SelectedIndex = (int)settings.AnchorPreset;
        _isInitializing = false;
    }

    private void SaveTrinket_Click(object sender, RoutedEventArgs e) => SaveTrinketRequested?.Invoke();

    private void ApplyTrinket_Click(object sender, RoutedEventArgs e)
    {
        if (TrinketsCombo.SelectedItem is TrinketDefinition selected) TrinketApplyRequested?.Invoke(selected);
    }

    private void DeleteTrinket_Click(object sender, RoutedEventArgs e)
    {
        if (TrinketsCombo.SelectedItem is TrinketDefinition selected) TrinketDeleteRequested?.Invoke(selected);
    }

    private void CharmCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CharmCombo.SelectedItem is CharmDefinition selected) CharmSelected?.Invoke(selected.Id);
    }

    private void CharmSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing) return;
        CharmSizeChanged?.Invoke(e.NewValue);
    }

    private void InitialLetterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitializing) return;
        InitialLetterChanged?.Invoke(InitialLetterTextBox.Text);
    }

    private void CordStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        CordStyleChanged?.Invoke((CordStyle)CordStyleCombo.SelectedIndex);
    }

    private void CordLengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing) return;
        RopeLengthChanged?.Invoke(e.NewValue);
    }

    private void BeadsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        BeadShapeChanged?.Invoke((BeadShape)BeadsCombo.SelectedIndex);
    }

    private void AnchorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        AnchorPresetChanged?.Invoke((AnchorPreset)AnchorCombo.SelectedIndex);
    }

    private void MonitorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (MonitorCombo.SelectedItem is ComboBoxItem item && item.Tag is string deviceName)
        {
            MonitorSelected?.Invoke(deviceName);
        }
    }

    private void ColorSwatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not string hex) return;
        var color = (Color)ColorConverter.ConvertFromString(hex);
        CordColorChanged?.Invoke(color);
    }

    private void CustomColorButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new ColorDialog { FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            System.Drawing.Color picked = dialog.Color;
            var color = Color.FromArgb(picked.A, picked.R, picked.G, picked.B);
            CordColorChanged?.Invoke(color);
        }
    }

    private void ImportCharm_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Choose an image",
            Filter = "Images (*.png;*.webp)|*.png;*.webp"
        };

        if (openDialog.ShowDialog() != true) return;

        string copiedPath = AssetService.CopyIntoAppData(openDialog.FileName);
        var editor = new AttachmentPointEditorWindow(copiedPath) { Owner = this };
        bool? result = editor.ShowDialog();

        if (result != true || !editor.Confirmed)
        {
            AssetService.DeleteFile(copiedPath);
            return;
        }

        string charmId = $"custom:{Guid.NewGuid():N}";
        string charmName = System.IO.Path.GetFileNameWithoutExtension(openDialog.FileName);

        var metadata = new ImportedCharmMetadata
        {
            Id = charmId,
            Name = charmName,
            FileName = System.IO.Path.GetFileName(copiedPath),
            AttachmentX = editor.AttachmentPoint.X,
            AttachmentY = editor.AttachmentPoint.Y
        };

        var importedList = AssetService.LoadMetadata();
        importedList.Add(metadata);
        AssetService.SaveMetadata(importedList);

        var newCharm = new CharmDefinition
        {
            Id = charmId,
            Name = charmName,
            Shape = CharmShape.Custom,
            Size = CharmSizeSlider.Value,
            ImagePath = copiedPath,
            AttachmentPoint = editor.AttachmentPoint
        };

        _allCharms.Add(newCharm);
        RefreshCharmCombo(newCharm);
        CharmSelected?.Invoke(newCharm.Id);
    }

    private void DeleteCharm_Click(object sender, RoutedEventArgs e)
    {
        if (CharmCombo.SelectedItem is not CharmDefinition selected || selected.Shape != CharmShape.Custom)
        {
            MessageBox.Show(
                "Only charms you've imported yourself can be deleted here - built-in charms are always available.",
                "Trinket", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Permanently delete \"{selected.Name}\"? This can't be undone.",
            "Delete Charm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            CharmDeleteRequested?.Invoke(selected);
        }
    }

    private void ResetDefaults_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Reset appearance and behavior to their defaults? Saved Trinkets and imported charms are kept.",
            "Reset to Defaults", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            ResetToDefaultsRequested?.Invoke();
        }
    }
}