﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using XOutput.Logging;
using XOutput.Tools;

namespace XOutput.UI.Windows;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IViewBase<MainWindowViewModel, MainWindowModel>
{
    private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(MainWindow));
    private bool hardExit;
    private WindowState restoreState = WindowState.Normal;

    public MainWindow(MainWindowViewModel viewModel, ArgumentParser argumentParser)
    {
        this.ViewModel = viewModel;
        DataContext = viewModel;
        if (argumentParser.Minimized)
        {
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
            logger.Info("Starting XOutput in minimized to taskbar");
        }
        else
        {
            ShowInTaskbar = true;
            logger.Info("Starting XOutput in normal window");
        }

        new WindowInteropHelper(this).EnsureHandle();
        InitializeComponent();
        viewModel.Initialize(Log);
        Dispatcher.Invoke(Initialize);
    }

    public MainWindowViewModel ViewModel { get; }

    private async Task Initialize()
    {
        await logger.Info("The application has started.");
        await GetData();
    }

    public async Task GetData()
    {
        try
        {
            var result = await new UpdateChecker.UpdateChecker().CompareRelease();
            ViewModel.VersionCompare(result);
        }
        catch (Exception)
        {
            // Version comparison failed
        }
    }

    public void Log(string msg)
    {
        Dispatcher.BeginInvoke((Action)(() =>
        {
            try
            {
                logBox.AppendText(msg + Environment.NewLine);
            }
            catch (Exception ex)
            {
                logger.Error("Cannot log into the log box: " + msg + Environment.NewLine);
                logger.Error(ex);
            }
        }));
    }

    private void AddControllerClick(object sender, RoutedEventArgs e)
    {
        ViewModel.AddController(null);
    }

    private void RefreshClick(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshGameControllers();
    }

    private void ExitClick(object sender, RoutedEventArgs e)
    {
        hardExit = true;
        if (IsLoaded)
        {
            Close();
        }
        else
        {
            logger.Info("The application will exit.");
            Application.Current.Shutdown();
        }
    }

    private void GameControllersClick(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenWindowsGameControllerSettings();
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SaveSettings();
    }

    private void SettingsClick(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenSettings();
    }

    private void DiagnosticsClick(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenDiagnostics();
    }

    private void AboutClick(object sender, RoutedEventArgs e)
    {
        ViewModel.AboutPopupShow();
    }

    private void WindowClosing(object sender, CancelEventArgs e)
    {
        if (ViewModel.GetSettings().CloseToTray && !hardExit)
        {
            e.Cancel = true;
            restoreState = WindowState;
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
            logger.Info("The application is closed to tray.");
        }
    }

    private async void WindowClosed(object sender, EventArgs e)
    {
        ViewModel.Dispose();
        await logger.Info("The application will exit.");
    }

    private void CheckBoxChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshGameControllers();
    }

    private void TaskbarIconTrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = restoreState;
        }
        else if (Visibility == Visibility.Hidden)
        {
            if (!IsLoaded) Show();
            ShowInTaskbar = true;
            Visibility = Visibility.Visible;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    public void ForceShow()
    {
        Dispatcher.Invoke(() => { TaskbarIconTrayMouseDoubleClick(this, null); });
    }
}