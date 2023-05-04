using System;
using System.Linq;
using System.Threading;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using XOutput.Devices.Input;
using XOutput.Devices.Input.DirectInput;
using XOutput.Devices.Mapper;
using XOutput.Devices.XInput;
using XOutput.Devices.XInput.Vigem;
using XOutput.Logging;

namespace XOutput.Devices;

/// <summary>
///     GameController is a container for input devices, mappers and output devices.
/// </summary>
public sealed class GameController : IDisposable
{
    private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(GameController));

    private IXbox360Controller controller;
    private int controllerCount = -1;
    private bool running;
    private Thread thread;

    public GameController(InputMapper mapper)
    {
        this.Mapper = mapper;
        var xOutputInterface = OutputDevices.Instance.GetDevices().ElementAtOrDefault(mapper.OutputDeviceIndex);
        XOutputInterface = xOutputInterface;
        XInput = new XOutputDevice(mapper);
        if (!string.IsNullOrEmpty(mapper.ForceFeedbackDevice))
        {
            var device = InputDevices.Instance.GetDevices().OfType<DirectDevice>()
                .FirstOrDefault(d => d.DisplayName == mapper.ForceFeedbackDevice);
            if (device != null) ForceFeedbackDevice = device;
        }

        running = false;
    }

    /// <summary>
    ///     Gets the output device.
    /// </summary>
    public XOutputDevice XInput { get; }

    /// <summary>
    ///     Gets the mapping of the input device.
    /// </summary>
    public InputMapper Mapper { get; }

    /// <summary>
    ///     Gets the name of the input device.
    /// </summary>
    public string DisplayName => Mapper.Name;

    /// <summary>
    ///     Gets if any XInput emulation is installed.
    /// </summary>
    public bool HasXOutputInstalled => XOutputInterface != null;

    /// <summary>
    ///     Gets if force feedback is supported.
    /// </summary>
    public bool ForceFeedbackSupported => XOutputInterface is VigemDevice;

    /// <summary>
    ///     Gets the force feedback device.
    /// </summary>
    public IInputDevice ForceFeedbackDevice { get; set; }

    /// <summary>
    ///     Gets the output device associated with this controller.
    /// </summary>
    public IXOutputInterface XOutputInterface { get; set; }

    /// <summary>
    ///     Disposes all used resources
    /// </summary>
    public void Dispose()
    {
        Stop();
        XInput?.Dispose();
        XOutputInterface?.Dispose();
    }

    ~GameController()
    {
        Dispose();
    }

    /// <summary>
    ///     Starts the emulation of the device
    /// </summary>
    public int Start(Action onStop = null)
    {
        if (!HasXOutputInstalled) return 0;
        controllerCount = OutputDevices.Instance.GetDevices().IndexOf(XOutputInterface);
        if (controller != null) controller.FeedbackReceived -= ControllerFeedbackReceived;
        thread = new Thread(() => ReadAndReportValues(onStop));
        running = true;
        thread.Name = $"Emulated controller {controllerCount} output refresher";
        thread.IsBackground = true;
        thread.Start();
        logger.Info($"Emulation started on {ToString()}.");
        if (ForceFeedbackSupported)
        {
            logger.Info($"Force feedback mapping is connected on {ToString()}.");
            controller = ((VigemDevice)XOutputInterface).GetController(controllerCount);
            controller.FeedbackReceived += ControllerFeedbackReceived;
        }

        return controllerCount;
    }

    /// <summary>
    ///     Stops the emulation of the device
    /// </summary>
    public void Stop()
    {
        if (running)
        {
            running = false;
            XInput.InputChanged -= XInputInputChanged;
            if (ForceFeedbackSupported)
            {
                controller.FeedbackReceived -= ControllerFeedbackReceived;
                logger.Info($"Force feedback mapping is disconnected on {ToString()}.");
            }

            logger.Info($"Emulation stopped on {ToString()}.");
            resetId();
            thread?.Interrupt();
        }
    }

    public override string ToString()
    {
        return DisplayName;
    }

    private void ReadAndReportValues(Action onStop)
    {
        try
        {
            XInput.InputChanged += XInputInputChanged;
            while (running) Thread.Sleep(100);
        }
        catch (ThreadInterruptedException)
        {
        }
        finally
        {
            onStop?.Invoke();
            Stop();
        }
    }

    private void XInputInputChanged(object sender, DeviceInputChangedEventArgs e)
    {
        if (!XOutputInterface.Report(controllerCount, XInput.GetValues())) Stop();
    }

    private void ControllerFeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
    {
        ForceFeedbackDevice?.SetForceFeedback((double)e.LargeMotor / byte.MaxValue,
            (double)e.SmallMotor / byte.MaxValue);
    }

    private void resetId()
    {
        if (controllerCount > -1)
        {
            Controllers.Instance.DisposeId(controllerCount);
            controllerCount = -1;
        }
    }
}