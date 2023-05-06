using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Serilog;
using SharpDX.DirectInput;
using XOutput.Devices;
using XOutput.Devices.Input;
using XOutput.Devices.Input.DirectInput;
using XOutput.Devices.Mapper;
using XOutput.Devices.XInput;
using XOutput.Devices.XInput.Vigem;
using XOutput.Diagnostics;

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
        RefreshInputDevices();
        SetupTimer();
        // AddKeyboardController();
        // AddMouseController();
        AddOutputControllers();
    }

    private void CheckVigemAvailable()
    {
        if (HasVigem) return;
        Log.Error("ViGEm is not installed.");
        var error = Translate("VigemAndScpNotInstalled");
        log(error);
        MessageBox.Show(error, Translate("Error"));
    }

    private void SetupTimer()
    {
        timer.Interval = TimeSpan.FromMilliseconds(5000);
        timer.Tick += (sender1, e1) =>
        {
            if (!settings.DisableAutoRefresh) RefreshInputDevices();
        };
        timer.Start();
    }

    private void AddKeyboardController()
    {
        Log.Debug("Creating keyboard controller");
        var keyboard = new Keyboard();
        InputDevices.Instance.Add(keyboard);
        settings.GetOrCreateInputConfiguration(keyboard.ToString(), keyboard.InputConfiguration);
        Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), keyboard)));

        log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"),
            LanguageModel.Instance.Translate("Keyboard")));
        Log.Information("Keyboard controller is connected");
    }

    private void AddMouseController()
    {
        Log.Debug("Creating mouse controller");
        var mouse = new Mouse();
        settings.GetOrCreateInputConfiguration(mouse.ToString(), mouse.InputConfiguration);
        Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), mouse)));
        log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"),
            LanguageModel.Instance.Translate("Mouse")));
        Log.Information("Mouse controller is connected");
    }

    private void AddOutputControllers()
    {
        foreach (var mapping in settings.Mapping)
        {
            // AddController(mapping);
        }
    }


    private void SetupSettings(LanguageManager languageManager)
    {
        try
        {
            settings = Settings.Load(SettingsFilePath);
            languageManager.Language = settings.Language;
            Log.Information("Loading settings was successful.");
            log(string.Format(Translate("LoadSettingsSuccess"), SettingsFilePath));
            Model.Settings = settings;
        }
        catch (Exception ex)
        {
            settings = new Settings();
            Log.Warning("Loading settings was unsuccessful.");
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
            Log.Information("Saving settings was successful.");
            log(string.Format(Translate("SaveSettingsSuccess"), SettingsFilePath));
        }
        catch (Exception ex)
        {
            Log.Warning("Saving settings was unsuccessful.");
            Log.Warning(ex, "Exception");
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
                Log.Warning("Failed to check latest version");
                log(Translate("VersionCheckError"));
                break;
            case UpdateChecker.VersionCompare.NeedsUpgrade:
                Log.Information("New version is available");
                log(Translate("VersionCheckNeedsUpgrade"));
                break;
            case UpdateChecker.VersionCompare.NewRelease:
                log(Translate("VersionCheckNewRelease"));
                break;
            case UpdateChecker.VersionCompare.UpToDate:
                Log.Information("Version is up-to-date");
                log(Translate("VersionCheckUpToDate"));
                break;
            default:
                throw new ArgumentException(nameof(compare));
        }
    }

    public void RefreshInputDevices()
    {
        var dinputDevices = directInputDevices.GetInputDevices(Model.AllDevices).ToList();
        var didRemove = RemoveDisconnectedDevices(dinputDevices);
        var didAdd = AddNewDevices(dinputDevices);
        
        var devicesChanged = didRemove || didAdd;
        
        if (devicesChanged) UpdateControllersMappingsAndInstances();
    }

    private bool RemoveDisconnectedDevices(List<DeviceInstance> dinputDevices)
    {
        var didRemoveDevices = false;
        var itemsToRemove = new List<InputView>();
        foreach (var inputView in Model.Inputs)
        {
            var modelDevice = inputView.ViewModel.Model.Device;
            if (modelDevice is not DirectDevice dModelDevice) 
                continue;
            if (dinputDevices.Any(x => x.InstanceGuid == dModelDevice.Id))
                continue;
            itemsToRemove.Add(inputView);
        }

        // Remove the items from the collection.
        foreach (var item in itemsToRemove)
        {
            Model.Inputs.Remove(item);
            item.ViewModel.Dispose();
            didRemoveDevices = true;
        }

        return didRemoveDevices;
    }

    private bool AddNewDevices(List<DeviceInstance> dinputDevices)
    {
        var didAddDevices = false;
        foreach (var dinputDevice in dinputDevices)
        {
            if (InputViewAlreadyExists(dinputDevice)) continue;

            var device = InputDevices.Instance.GetDeviceByGuid(dinputDevice.InstanceGuid.ToString());
            if (device == null)
            {
                var displayName = CalculateDisplayName(dinputDevice);
                device = directInputDevices.CreateDirectDevice(dinputDevice, displayName);
                if (device == null) continue;

                // var inputConfig = settings.GetOrCreateInputConfiguration(device.ToString(), device.InputConfiguration);
                device.Disconnected -= DispatchRefreshGameControllers;
                device.Disconnected += DispatchRefreshGameControllers;
                InputDevices.Instance.Add(device);
            }

            Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), device)));
            didAddDevices = true;
        }

        return didAddDevices;
    }

    private bool InputViewAlreadyExists(DeviceInstance dinputDevice)
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

        //TODO:
        // Controllers.Instance.Update();
    }

    private string CalculateDisplayName(DeviceInstance dinputDevice)
    {
        var displayNames = InputDevices.Instance.GetDevices().Select(device => device.DisplayName).ToList();
        var count = 1;
        string baseName;
        do
        {
            baseName = $"{dinputDevice.ProductName} {count}";
            count++;
        } while (displayNames.Contains(baseName));
        return baseName;
    }

    public void AddController(InputMapper mapper)
    {
        var controllerMapper = mapper ?? settings.CreateMapper(Guid.NewGuid().ToString());
        var gameController = new GameController(controllerMapper);
        Controllers.Instance.Add(gameController);

        var controllerView = new ControllerView(new ControllerViewModel(gameController, log));
        controllerView.ViewModel.Model.CanStart = HasVigem;
        controllerView.RemoveClicked += RemoveController;
        Model.Controllers.Add(controllerView);
        log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"), gameController.DisplayName));
        if (mapper?.StartWhenConnected != true) return;
        controllerView.ViewModel.Start();
        Log.Information($"{mapper.Name} controller is started automatically.");
    }

    public void RemoveController(ControllerView controllerView)
    {
        var controller = controllerView.ViewModel.Model.Controller;
        controllerView.ViewModel.Dispose();
        controller.Dispose();
        Model.Controllers.Remove(controllerView);
        Log.Information($"{controller} is disconnected.");
        log(string.Format(LanguageModel.Instance.Translate("ControllerDisconnected"), controller.DisplayName));
        Controllers.Instance.Remove(controller);
        settings.Mapping.RemoveAll(m => m.Id == controller.Mapper.Id);
    }

    public void OpenWindowsGameControllerSettings()
    {
        Log.Debug("Starting " + GameControllersSettings);
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
        Log.Debug("Started " + GameControllersSettings);
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
            dispatcher.Invoke(RefreshInputDevices);
        })
        {
            Name = "Device list refresh delay",
            IsBackground = true
        };
        delayThread.Start();
    }
}