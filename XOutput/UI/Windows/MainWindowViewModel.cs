using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SharpDX.DirectInput;
using XOutput.Devices;
using XOutput.Devices.Input;
using XOutput.Devices.Input.DirectInput;
using XOutput.Devices.Mapper;
using XOutput.Devices.XInput;
using XOutput.Devices.XInput.Vigem;
using XOutput.Diagnostics;
using XOutput.Logging;
using XOutput.Tools;
using XOutput.UI.Component;
using XOutput.UpdateChecker;
using Keyboard = XOutput.Devices.Input.Keyboard.Keyboard;
using Mouse = XOutput.Devices.Input.Mouse.Mouse;
using Version = XOutput.UpdateChecker.Version;

namespace XOutput.UI.Windows;

public class MainWindowViewModel : ViewModelBase<MainWindowModel>, IDisposable
{
    private const string SettingsFilePath = "settings.json";
    private const string GameControllersSettings = "joy.cpl";

    private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MainWindowViewModel));
    private readonly DirectInputDevices directInputDevices = new();
    private readonly Dispatcher dispatcher;

    private readonly DispatcherTimer timer = new();

    private static bool HasVigem => VigemDevice.IsAvailable();

    private Action<string> log;
    private Settings settings;

    public MainWindowViewModel(MainWindowModel model, Dispatcher dispatcher)
        : base(model)
    {
        this.dispatcher = dispatcher;
    }

    private List<string> DisplayNames { get; } = new();

    public void Dispose()
    {
        foreach (var device in Model.Inputs.Select(x => x.ViewModel.Model.Device)) device.Dispose();

        foreach (var controller in Model.Controllers.Select(x => x.ViewModel.Model.Controller)) controller.Dispose();

        timer.Stop();
        directInputDevices.Dispose();
        GC.SuppressFinalize(this);
    }

    public Settings GetSettings()
    {
        return settings;
    }

    public void Initialize(Action<string> logging)
    {
        log = logging;
        var languageManager = LanguageManager.Instance;
        SetupSettings(languageManager);
        CheckVigemAvailable();
        RefreshGameControllers();
        SetupTimer();
        AddKeyboardController();
        AddMouseController();
        AddControllersFromSettings();
    }

    private void CheckVigemAvailable()
    {
        if (HasVigem) return;
        Logger.Error("ViGEm is not installed.");
        var error = Translate("VigemAndScpNotInstalled");
        log(error);
        MessageBox.Show(error, Translate("Error"));
    }

    private void SetupTimer()
    {
        timer.Interval = TimeSpan.FromMilliseconds(5000);
        timer.Tick += (sender1, e1) =>
        {
            if (!settings.DisableAutoRefresh) RefreshGameControllers();
        };
        timer.Start();
    }

    private void AddKeyboardController()
    {
        Logger.Debug("Creating keyboard controller");
        var keyboard = new Keyboard();
        settings.GetOrCreateInputConfiguration(keyboard.ToString(), keyboard.InputConfiguration);
        InputDevices.Instance.Add(keyboard);
        Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), keyboard)));

        log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"),
            LanguageModel.Instance.Translate("Keyboard")));
        Logger.Info("Keyboard controller is connected");
    }

    private void AddMouseController()
    {
        Logger.Debug("Creating mouse controller");
        var mouse = new Mouse();
        settings.GetOrCreateInputConfiguration(mouse.ToString(), mouse.InputConfiguration);
        InputDevices.Instance.Add(mouse);
        Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), mouse)));

        log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"),
            LanguageModel.Instance.Translate("Mouse")));
        Logger.Info("Mouse controller is connected");
    }

    private void AddControllersFromSettings()
    {
        foreach (var mapping in settings.Mapping)
        {
            AddController(mapping);
        }
    }


    private void SetupSettings(LanguageManager languageManager)
    {
        try
        {
            settings = Settings.Load(SettingsFilePath);
            languageManager.Language = settings.Language;
            Logger.Info("Loading settings was successful.");
            log(string.Format(Translate("LoadSettingsSuccess"), SettingsFilePath));
            Model.Settings = settings;
        }
        catch (Exception ex)
        {
            settings = new Settings();
            Logger.Warning("Loading settings was unsuccessful.");
            var error = string.Format(Translate("LoadSettingsError"), SettingsFilePath) + Environment.NewLine +
                        ex.Message;
            log(error);
            MessageBox.Show(error, Translate("Warning"));
        }
    }

    public void SaveSettings()
    {
        try
        {
            settings.Save(SettingsFilePath);
            Logger.Info("Saving settings was successful.");
            log(string.Format(Translate("SaveSettingsSuccess"), SettingsFilePath));
        }
        catch (Exception ex)
        {
            Logger.Warning("Saving settings was unsuccessful.");
            Logger.Warning(ex);
            var error = string.Format(Translate("SaveSettingsError"), SettingsFilePath) + Environment.NewLine +
                        ex.Message;
            log(error);
            MessageBox.Show(error, Translate("Warning"));
        }
    }

    public void AboutPopupShow()
    {
        MessageBox.Show(
            Translate("AboutContent") + Environment.NewLine +
            string.Format(Translate("Version"), Version.AppVersion), Translate("AboutMenu"));
    }

    public void VersionCompare(VersionCompare compare)
    {
        switch (compare)
        {
            case UpdateChecker.VersionCompare.Error:
                Logger.Warning("Failed to check latest version");
                log(Translate("VersionCheckError"));
                break;
            case UpdateChecker.VersionCompare.NeedsUpgrade:
                Logger.Info("New version is available");
                log(Translate("VersionCheckNeedsUpgrade"));
                break;
            case UpdateChecker.VersionCompare.NewRelease:
                log(Translate("VersionCheckNewRelease"));
                break;
            case UpdateChecker.VersionCompare.UpToDate:
                Logger.Info("Version is up-to-date");
                log(Translate("VersionCheckUpToDate"));
                break;
            default:
                throw new ArgumentException(nameof(compare));
        }
    }

    public void RefreshGameControllers()
    {
        var dinputDevices = directInputDevices.GetInputDevices(Model.AllDevices).ToList();
        var devicesChanged = false;

        var didRemove = RemoveDisconnectedDevices(dinputDevices);
        var didAdd = AddNewDevices(dinputDevices);

        if (devicesChanged) UpdateControllersMappingsAndInstances();
    }

    private bool RemoveDisconnectedDevices(List<DeviceInstance> dinputDevices)
    {
        var didRemoveDevices = false;
        foreach (var inputView in Model.Inputs.ToArray())
        {
            var modelDevice = inputView.ViewModel.Model.Device;
            if (modelDevice is not DirectDevice dModelDevice) continue;
            if (dinputDevices.Any(x => x.InstanceGuid == dModelDevice.Id)) continue;

            Model.Inputs.Remove(inputView);
            InputDevices.Instance.Remove(modelDevice);
            inputView.ViewModel.Dispose();
            DisplayNames.Remove(modelDevice.DisplayName);
            didRemoveDevices = true;
        }

        return didRemoveDevices;
    }

    private bool AddNewDevices(List<DeviceInstance> dinputDevices)
    {
        var didAddDevices = false;
        foreach (var dinputDevice in dinputDevices)
        {
            if (DeviceAlreadyExists(dinputDevice)) continue;

            var displayName = CalculateDisplayName(dinputDevice);
            DisplayNames.Add(displayName);

            var device = directInputDevices.CreateDirectDevice(dinputDevice, displayName);
            if (device == null) continue;

            var inputConfig = settings.GetOrCreateInputConfiguration(device.ToString(), device.InputConfiguration);
            device.Disconnected -= DispatchRefreshGameControllers;
            device.Disconnected += DispatchRefreshGameControllers;
            Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), device)));
            didAddDevices = true;
        }

        return didAddDevices;
    }

    private bool DeviceAlreadyExists(DeviceInstance dinputDevice)
    {
        return Model.Inputs.Select(c => c.ViewModel.Model.Device)
            .OfType<DirectDevice>()
            .Any(modelDevice => modelDevice.Id == dinputDevice.InstanceGuid);
    }

    private void UpdateControllersMappingsAndInstances()
    {
        foreach (var controller in Controllers.Instance.GetControllers())
        {
            var mapper = settings.CreateMapper(controller.Mapper.Id);
            controller.Mapper.Mappings = mapper.Mappings;
        }

        Controllers.Instance.Update(InputDevices.Instance.GetDevices());
    }

    private string CalculateDisplayName(DeviceInstance dinputDevice)
    {
        var count = 1;
        string baseName;
        do
        {
            baseName = $"{dinputDevice.ProductName} {count}";
            count++;
        } while (DisplayNames.Contains(baseName));

        return baseName;
    }

    public void AddController(InputMapper mapper)
    {
        var controllerMapper = mapper ?? settings.CreateMapper(Guid.NewGuid().ToString());
        var gameController = new GameController(controllerMapper);
        Controllers.Instance.Add(gameController);

        var controllerView = new ControllerView(new ControllerViewModel(gameController, log));
        controllerView.ViewModel.Model.CanStart = CanStart;
        controllerView.RemoveClicked += RemoveController;
        Model.Controllers.Add(controllerView);
        log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"), gameController.DisplayName));
        if (mapper?.StartWhenConnected != true) return;
        controllerView.ViewModel.Start();
        Logger.Info($"{mapper.Name} controller is started automatically.");
    }

    public void RemoveController(ControllerView controllerView)
    {
        var controller = controllerView.ViewModel.Model.Controller;
        controllerView.ViewModel.Dispose();
        controller.Dispose();
        Model.Controllers.Remove(controllerView);
        Logger.Info($"{controller} is disconnected.");
        log(string.Format(LanguageModel.Instance.Translate("ControllerDisconnected"), controller.DisplayName));
        Controllers.Instance.Remove(controller);
        settings.Mapping.RemoveAll(m => m.Id == controller.Mapper.Id);
    }

    public void OpenWindowsGameControllerSettings()
    {
        Logger.Debug("Starting " + GameControllersSettings);
        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + GameControllersSettings,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        }.Start();
        Logger.Debug("Started " + GameControllersSettings);
    }

    public void OpenSettings()
    {
        var context = ApplicationContext.Global.WithSingletons(settings);
        var settingsWindow = context.Resolve<SettingsWindow>();
        settingsWindow.ShowDialog();
    }

    public void OpenDiagnostics()
    {
        IList<IDiagnostics> elements = InputDevices.Instance.GetDevices()
            .Select(d => new InputDiagnostics(d)).OfType<IDiagnostics>().ToList();
        elements.Insert(0, new XInputDiagnostics());

        var context = ApplicationContext.Global.WithSingletons(new DiagnosticsModel(elements));
        var diagnosticsWindow = context.Resolve<DiagnosticsWindow>();
        diagnosticsWindow.ShowDialog();
    }

    private string Translate(string key)
    {
        return LanguageModel.Instance.Translate(key);
    }

    private void DispatchRefreshGameControllers(object sender, DeviceDisconnectedEventArgs e)
    {
        var delayThread = new Thread(() =>
        {
            Thread.Sleep(1000);
            dispatcher.Invoke(RefreshGameControllers);
        })
        {
            Name = "Device list refresh delay",
            IsBackground = true
        };
        delayThread.Start();
    }
}