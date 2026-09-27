using System.Reflection;
using System.Windows;

namespace Trinket.Studio;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        System.Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = version != null
            ? $"Version {version.Major}.{version.Minor}.{version.Build}"
            : "Version unknown";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}