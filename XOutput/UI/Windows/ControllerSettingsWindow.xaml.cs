using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using XOutput.Devices;

namespace XOutput.UI.Windows;

/// <summary>
///     Interaction logic for ControllerSettings.xaml
/// </summary>
public partial class ControllerSettingsWindow : Window, IViewBase<ControllerSettingsViewModel, ControllerSettingsModel>
{
    private readonly GameController controller;
    private readonly DispatcherTimer timer = new();

    public ControllerSettingsWindow(ControllerSettingsViewModel viewModel, GameController controller)
    {
        this.controller = controller;
        this.ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public ControllerSettingsViewModel ViewModel { get; }

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Update();
        timer.Interval = TimeSpan.FromMilliseconds(10);
        timer.Tick += TimerTick;
        timer.Start();
    }

    private void TimerTick(object sender, EventArgs e)
    {
        ViewModel.Update();
    }

    protected override void OnClosed(EventArgs e)
    {
        timer.Tick -= TimerTick;
        timer.Stop();
        ViewModel.Dispose();
        base.OnClosed(e);
    }

    private void ConfigureAllButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ConfigureAll();
    }

    private void CheckBoxChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.SetStartWhenConnected();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetForceFeedback();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        controller.Mapper.Name = ViewModel.Model.Title;
    }
}