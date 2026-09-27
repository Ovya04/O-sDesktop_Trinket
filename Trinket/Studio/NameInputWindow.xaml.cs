using System.Windows;

namespace Trinket.Studio;

public partial class NameInputWindow : Window
{
    public string EnteredName { get; private set; } = string.Empty;
    public bool Confirmed { get; private set; }

    public NameInputWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => NameTextBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string trimmed = NameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            // Simple guard: keep the dialog open until something is entered,
            // rather than allowing an unnamed preset.
            return;
        }

        EnteredName = trimmed;
        Confirmed = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        DialogResult = false;
    }
}