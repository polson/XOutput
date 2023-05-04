using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace XOutput.UI.Windows;

/// <summary>
///     Interaction logic for AutoConfigureWindow.xaml
/// </summary>
public partial class AutoConfigureWindow : Window, IViewBase<AutoConfigureViewModel, AutoConfigureModel>
{
    private readonly bool timed;
    private readonly DispatcherTimer timer = new();

    public AutoConfigureWindow(AutoConfigureViewModel viewModel, bool timed)
    {
        this.ViewModel = viewModel;
        this.timed = timed;
        DataContext = viewModel;
        InitializeComponent();
    }

    public AutoConfigureViewModel ViewModel { get; }

    private async void WindowLoaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(100);
        ViewModel.Initialize();
        ViewModel.IsMouseOverButtons = () => { return DisableButton.IsMouseOver || SaveButton.IsMouseOver; };
        if (timed)
        {
            timer.Interval = TimeSpan.FromMilliseconds(25);
            timer.Tick += TimerTick;
            timer.Start();
        }
    }

    private void TimerTick(object sender, EventArgs e)
    {
        if (ViewModel.IncreaseTime())
        {
            var hasNextInput = ViewModel.SaveValues();
            if (!hasNextInput) Close();
        }
    }

    private void DisableClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.SaveDisableValues()) Close();
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.SaveValues()) Close();
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void WindowClosed(object sender, EventArgs e)
    {
        timer.Tick -= TimerTick;
        timer.Stop();
        ViewModel.Close();
    }
}