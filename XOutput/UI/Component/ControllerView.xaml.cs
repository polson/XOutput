using System;
using System.Windows;
using System.Windows.Controls;

namespace XOutput.UI.Component;

/// <summary>
///     Interaction logic for ControllerView.xaml
/// </summary>
public partial class ControllerView : UserControl, IViewBase<ControllerViewModel, ControllerModel>
{
    protected readonly ControllerViewModel viewModel;

    public ControllerView(ControllerViewModel viewModel)
    {
        this.viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public ControllerViewModel ViewModel => viewModel;
    public event Action<ControllerView> RemoveClicked;

    private void OpenClick(object sender, RoutedEventArgs e)
    {
        viewModel.Edit();
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
        viewModel.StartStop();
    }

    private void RemoveClick(object sender, RoutedEventArgs e)
    {
        RemoveClicked?.Invoke(this);
    }

    private void GroupSelect(object sender, RoutedEventArgs e)
    {
        // Get the selected index of the ComboBox
        if (!(sender is ComboBox comboBox)) return;
        var selectedIndex = comboBox.SelectedIndex;
        viewModel.SetOutputDevice(selectedIndex);
    }
}