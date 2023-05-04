using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using XOutput.Devices.Input.Mouse;
using XOutput.Logging;
using XOutput.Tools;
using XOutput.UI;
using XOutput.UI.Windows;

namespace XOutput;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(App));
    private readonly ArgumentParser argumentParser;

    private MainWindowViewModel mainWindowViewModel;

    public App()
    {
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

    public async Task UnhandledException(Exception exceptionObject)
    {
        await logger.Error(exceptionObject);
        MessageBox.Show(exceptionObject.Message + Environment.NewLine + exceptionObject.StackTrace);
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
            logger.Error(ex);
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
        catch (Exception ex)
        {
            //YOLO
        }
    }
}