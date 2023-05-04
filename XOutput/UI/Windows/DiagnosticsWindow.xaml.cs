using System.Windows;

namespace XOutput.UI.Windows;

/// <summary>
///     Interaction logic for AutoConfigureWindow.xaml
/// </summary>
public partial class DiagnosticsWindow : Window, IViewBase<DiagnosticsViewModel, DiagnosticsModel>
{
    public DiagnosticsWindow(DiagnosticsViewModel viewModel)
    {
        this.ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public DiagnosticsViewModel ViewModel { get; }
}