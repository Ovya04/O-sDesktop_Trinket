using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Trinket.Studio;

/// <summary>
/// Lets the user click a point on an imported charm image to mark where the
/// cord should visually attach. Note: uses a simple normalized-click
/// mapping against the Image control's own rendered bounds - very accurate
/// for roughly square images, with minor imprecision near padded edges on
/// images with unusual aspect ratios (acceptable for this first
/// implementation per the project spec).
/// </summary>
public partial class AttachmentPointEditorWindow : Window
{
    public Point AttachmentPoint { get; private set; } = new(0.5, 0.1);
    public bool Confirmed { get; private set; }

    public AttachmentPointEditorWindow(string imagePath)
    {
        InitializeComponent();

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        PreviewImage.Source = bitmap;
    }

    private void PreviewImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickPosition = e.GetPosition(PreviewImage);

        if (PreviewImage.ActualWidth <= 0 || PreviewImage.ActualHeight <= 0)
        {
            return;
        }

        double relativeX = Math.Clamp(clickPosition.X / PreviewImage.ActualWidth, 0.0, 1.0);
        double relativeY = Math.Clamp(clickPosition.Y / PreviewImage.ActualHeight, 0.0, 1.0);

        AttachmentPoint = new Point(relativeX, relativeY);

        AttachmentMarker.Visibility = Visibility.Visible;
        Canvas.SetLeft(AttachmentMarker, clickPosition.X - AttachmentMarker.Width / 2);
        Canvas.SetTop(AttachmentMarker, clickPosition.Y - AttachmentMarker.Height / 2);
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        DialogResult = false;
    }
}