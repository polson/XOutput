using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using Serilog.Events;
using XOutput.Devices.Input.Mouse;
using XOutput.Tools;
using XOutput.UI;
using XOutput.UI.Windows;

namespace XOutput;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ArgumentParser argumentParser;

    private MainWindowViewModel mainWindowViewModel;

    public App()
    {
        SetupLogging();
        SetupInputRefresher();
        var dependencyEmbedder = new DependencyEmbedder();
        dependencyEmbedder.AddPackage("Newtonsoft.Json");
        dependencyEmbedder.AddPackage("SharpDX.DirectInput");
        dependencyEmbedder.AddPackage("SharpDX");
        dependencyEmbedder.AddPackage("Hardcodet.Wpf.TaskbarNotification");
        dependencyEmbedder.AddPackage("Nefarius.ViGEm.Client");
        dependencyEmbedder.Initialize();
        var exePath = AppContext.BaseDirectory;
        var cwd = Path.GetDirectoryName(exePath);
        Directory.SetCurrentDirectory(cwd);

        var globalContext = ApplicationContext.Global;
        globalContext.Resolvers.Add(Resolver.CreateSingleton(Dispatcher));
        globalContext.AddFromConfiguration(typeof(ApplicationConfiguration));
        globalContext.AddFromConfiguration(typeof(UIConfiguration));

        argumentParser = globalContext.Resolve<ArgumentParser>();
#if !DEBUG
        Dispatcher.UnhandledException += async (sender, e) => await UnhandledException(e.Exception);
#endif
    }

    private void SetupInputRefresher()
    {
        var refresher = new InputRefresher(0.5);
        refresher.Start();
    }

    private void SetupLogging()
    {
        const string logFilePath = "xoutput.log";
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(logFilePath)
            .CreateLogger();
    }

    public Task UnhandledException(Exception exceptionObject)
    {
        Log.Error(exceptionObject, "Exception");
        MessageBox.Show(exceptionObject.Message + Environment.NewLine + exceptionObject.StackTrace);
        return Task.CompletedTask;
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            var mainWindow = ApplicationContext.Global.Resolve<MainWindow>();
            mainWindowViewModel = mainWindow.ViewModel;
            MainWindow = mainWindow;
            if (!argumentParser.Minimized) mainWindow.Show();
            ApplicationContext.Global.Resolve<MouseHook>().StartHook();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception");
            MessageBox.Show(ex.ToString());
            Current.Shutdown();
        }
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        try
        {
            mainWindowViewModel?.Dispose();
            ApplicationContext.Global.Close();
        }
        catch (Exception)
        {
            //YOLO
        }
    }
}