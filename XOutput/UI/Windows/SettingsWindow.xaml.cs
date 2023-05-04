using System.Windows;

namespace XOutput.UI.Windows;

/// <summary>
///     Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window, IViewBase<SettingsViewModel, SettingsModel>
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        this.ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}