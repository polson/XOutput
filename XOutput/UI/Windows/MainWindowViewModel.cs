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

namespace XOutput.UI.Windows
{
    public class MainWindowViewModel : ViewModelBase<MainWindowModel>, IDisposable
    {
        private readonly int pid = Environment.ProcessId;
        private const string SettingsFilePath = "settings.json";
        private const string GameControllersSettings = "joy.cpl";

        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MainWindowViewModel));
        private readonly Dispatcher dispatcher;

        private readonly DispatcherTimer timer = new();
        private readonly DirectInputDevices directInputDevices = new();
        private Action<string> log;
        private Settings settings;
        private bool canStart;
        private List<string> DisplayNames { get; } = new();

        public MainWindowViewModel(MainWindowModel model, Dispatcher dispatcher)
            : base(model)
        {
            this.dispatcher = dispatcher;
        }

        public void Dispose()
        {
            foreach (var device in Model.Inputs.Select(x => x.ViewModel.Model.Device))
            {
                device.Dispose();
            }

            foreach (var controller in Model.Controllers.Select(x => x.ViewModel.Model.Controller))
            {
                controller.Dispose();
            }

            timer.Stop();
            directInputDevices.Dispose();
            GC.SuppressFinalize(this);
        }

        private void LoadSettings(string settingsFilePath)
        {
            try
            {
                settings = Settings.Load(settingsFilePath);
            }
            catch
            {
                settings = new Settings();
                throw;
            }
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
            if (VigemDevice.IsAvailable())
            {
                canStart = true;
            }
            else
            {
                Logger.Error("ViGEm is not installed.");
                var error = Translate("VigemAndScpNotInstalled");
                log(error);
                canStart = false;
                MessageBox.Show(error, Translate("Error"));
            }

            Model.Settings = settings;
            RefreshGameControllers();

            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += (object sender1, EventArgs e1) =>
            {
                if (!settings.DisableAutoRefresh)
                {
                    RefreshGameControllers();
                }
            };
            timer.Start();

            Logger.Debug("Creating keyboard controller");
            Keyboard keyboard = new Keyboard();
            settings.GetOrCreateInputConfiguration(keyboard.ToString(), keyboard.InputConfiguration);
            InputDevices.Instance.Add(keyboard);
            Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), keyboard, false)));
            Logger.Debug("Creating mouse controller");
            Mouse mouse = new Mouse();
            settings.GetOrCreateInputConfiguration(mouse.ToString(), mouse.InputConfiguration);
            InputDevices.Instance.Add(mouse);
            Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), mouse, false)));
            foreach (var mapping in settings.Mapping)
            {
                AddController(mapping);
            }

            logging(string.Format(LanguageModel.Instance.Translate("ControllerConnected"),
                LanguageModel.Instance.Translate("Keyboard")));
            Logger.Info("Keyboard controller is connected");
        }

        private void SetupSettings(LanguageManager languageManager)
        {
            try
            {
                LoadSettings(SettingsFilePath);
                languageManager.Language = settings.Language;
                Logger.Info("Loading settings was successful.");
                log(string.Format(Translate("LoadSettingsSuccess"), SettingsFilePath));
            }
            catch (Exception ex)
            {
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
                string error = string.Format(Translate("SaveSettingsError"), SettingsFilePath) + Environment.NewLine +
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
            IEnumerable<DeviceInstance> dinputDevices =
                directInputDevices.GetInputDevices(Model.AllDevices);

            bool changed = false;
            var deviceInstances = dinputDevices.ToList();

            foreach (var inputView in Model.Inputs.ToArray())
            {
                var modelDevice = inputView.ViewModel.Model.Device;
                if (modelDevice is not DirectDevice ||
                    (deviceInstances.Any(x => x.InstanceGuid == ((DirectDevice)modelDevice).Id))) continue;

                Model.Inputs.Remove(inputView);
                InputDevices.Instance.Remove(modelDevice);
                inputView.ViewModel.Dispose();
                modelDevice.Dispose();
                DisplayNames.Remove(modelDevice.DisplayName);
                changed = true;
            }

            foreach (var dinputDevice in deviceInstances)
            {
                //Check if Model.Inputs already contains this device, if so ignore
                if (Model.Inputs.Select(c => c.ViewModel.Model.Device).OfType<DirectDevice>()
                    .Any(modelDevice => modelDevice.Id == dinputDevice.InstanceGuid)) continue;

                // Calculate display name and add to list
                int count = 1;
                string baseName = $"{dinputDevice.ProductName} {count}";
                while (DisplayNames.Contains(baseName))
                {
                    baseName = $"{dinputDevice.ProductName} {count}";
                    count++;
                }

                DisplayNames.Add(baseName);

                var device = directInputDevices.CreateDirectDevice(dinputDevice, baseName);
                if (device == null)
                {
                    continue;
                }

                InputConfig inputConfig =
                    settings.GetOrCreateInputConfiguration(device.ToString(), device.InputConfiguration);
                device.Disconnected -= DispatchRefreshGameControllers;
                device.Disconnected += DispatchRefreshGameControllers;
                Model.Inputs.Add(new InputView(new InputViewModel(new InputModel(), device, Model.IsAdmin)));
                changed = true;
            }

            if (changed)
            {
                foreach (var controller in Controllers.Instance.GetControllers())
                {
                    var mapper = settings.CreateMapper(controller.Mapper.Id);
                    controller.Mapper.Mappings = mapper.Mappings;
                }

                Controllers.Instance.Update(InputDevices.Instance.GetDevices());
            }
        }

        public void AddController(InputMapper mapper)
        {
            var gameController =
                new GameController(mapper ?? settings.CreateMapper(Guid.NewGuid().ToString()));
            Controllers.Instance.Add(gameController);

            var controllerView =
                new ControllerView(new ControllerViewModel(new ControllerModel(), gameController, Model.IsAdmin, log));
            controllerView.ViewModel.Model.CanStart = canStart;
            controllerView.RemoveClicked += RemoveController;
            Model.Controllers.Add(controllerView);
            log(string.Format(LanguageModel.Instance.Translate("ControllerConnected"), gameController.DisplayName));
            if (mapper?.StartWhenConnected == true)
            {
                controllerView.ViewModel.Start();
                Logger.Info($"{mapper.Name} controller is started automatically.");
            }
        }

        public void RemoveController(ControllerView controllerView)
        {
            var controller = controllerView.ViewModel.Model.Controller;
            controllerView.ViewModel.Dispose();
            controller.Dispose();
            Model.Controllers.Remove(controllerView);
            Logger.Info($"{controller.ToString()} is disconnected.");
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
                    UseShellExecute = false,
                },
            }.Start();
            Logger.Debug("Started " + GameControllersSettings);
        }

        public void OpenSettings()
        {
            ApplicationContext context = ApplicationContext.Global.WithSingletons(settings);
            SettingsWindow settingsWindow = context.Resolve<SettingsWindow>();
            settingsWindow.ShowDialog();
        }

        public void OpenDiagnostics()
        {
            IList<IDiagnostics> elements = InputDevices.Instance.GetDevices()
                .Select(d => new InputDiagnostics(d)).OfType<IDiagnostics>().ToList();
            elements.Insert(0, new XInputDiagnostics());

            ApplicationContext context = ApplicationContext.Global.WithSingletons(new DiagnosticsModel(elements));
            DiagnosticsWindow diagnosticsWindow = context.Resolve<DiagnosticsWindow>();
            diagnosticsWindow.ShowDialog();
        }

        private string Translate(string key)
        {
            return LanguageModel.Instance.Translate(key);
        }

        private void DispatchRefreshGameControllers(object sender, DeviceDisconnectedEventArgs e)
        {
            Thread delayThread = new Thread(() =>
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
}