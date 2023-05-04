using System;
using System.Windows;
using System.Windows.Threading;
using XOutput.Devices.Input;

namespace XOutput.UI.Windows;

/// <summary>
///     Interaction logic for ControllerSettings.xaml
/// </summary>
public partial class InputSettingsWindow : Window, IViewBase<InputSettingsViewModel, InputSettingsModel>
{
    private readonly IInputDevice device;
    private readonly DispatcherTimer timer = new();

    public InputSettingsWindow(InputSettingsViewModel viewModel, IInputDevice device)
    {
        this.device = device;
        this.ViewModel = viewModel;
        device.Disconnected += Disconnected;
        DataContext = viewModel;
        InitializeComponent();
    }

    public InputSettingsViewModel ViewModel { get; }

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
        device.Disconnected -= Disconnected;
        timer.Tick -= TimerTick;
        timer.Stop();
        ViewModel.Dispose();
        base.OnClosed(e);
    }

    private void Disconnected(object sender, DeviceDisconnectedEventArgs e)
    {
        Dispatcher.Invoke(() => { Close(); });
    }

    private void ForceFeedbackButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.TestForceFeedback();
    }

    private void ForceFeedbackCheckBoxChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.SetForceFeedbackEnabled();
    }
}