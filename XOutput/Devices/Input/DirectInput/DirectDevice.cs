using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Serilog;
using SharpDX;
using SharpDX.DirectInput;
using XOutput.Tools;
using Timer = System.Timers.Timer;

namespace XOutput.Devices.Input.DirectInput;

/// <summary>
///     Device that contains data for a DirectInput device
/// </summary>
public sealed class DirectDevice : IInputDevice
{
    private const int MaxButtons = 128;
    private const int MaxAxes = 24;
    private const int DPadOffsetBase = 1000;
    private const int InputReadDelayMs = 1;

    private readonly List<DirectDeviceForceFeedback> actuators = new();
    private readonly DeviceInputChangedEventArgs deviceInputChangedEventArgs;
    private readonly DeviceInstance deviceInstance;
    private readonly Thread inputRefresher;
    private readonly Joystick joystick;
    private readonly DirectInputSource[] inputSources;
    private bool connected;
    private bool disposed;
    private readonly int dpadCount;
    private readonly List<InputSource> dpadSources = new();
    private JoystickState joystickState;

    /// <summary>
    ///     Creates a new DirectDevice instance.
    /// </summary>
    /// <param name="deviceInstance">SharpDX device instance.</param>
    /// <param name="joystick">SharpDX joystick.</param>
    /// <param name="displayName">The display name for the device.</param>
    public DirectDevice(DeviceInstance deviceInstance, Joystick joystick, string displayName)
    {
        this.deviceInstance = deviceInstance;
        this.joystick = joystick;
        dpadCount = joystick.Capabilities.PovCount;
        DisplayName = displayName;
        InitializeJoystick();
        InitializeForceFeedback();
        LogDeviceInformation();
        inputSources = GetInputSources();
        deviceInputChangedEventArgs = new DeviceInputChangedEventArgs(this);
        InputConfiguration = new InputConfig(ForceFeedbackCount);
        Connected = true;
    }

    private DirectInputSource[] GetInputSources()
    {
        var buttons = GetButtonSources();
        var axes = GetAxisSources();
        var sliders = GetSliderSources();
        var dpads = GetDpadSources();
        dpadSources.AddRange(dpads);
        return buttons.Concat(axes).Concat(sliders).Concat(dpads).ToArray();
    }

    /// <summary>
    ///     Gets the current state of the inputTpye.
    ///     <para>Implements <see cref="IDevice.Get(InputSource)" /></para>
    /// </summary>
    /// <param name="source">Type of input</param>
    /// <returns>Value</returns>
    public double Get(InputSource source)
    {
        return source.Value;
    }

    /// <summary>
    ///     Sets the force feedback motor values.
    ///     <para>Implements <see cref="IInputDevice.SetForceFeedback(double, double)" /></para>
    /// </summary>
    /// <param name="big">Big motor value</param>
    /// <param name="small">Small motor value</param>
    public void SetForceFeedback(double big, double small)
    {
        if (ForceFeedbackCount == 0) return;
        if (!InputConfiguration.ForceFeedback)
        {
            big = 0;
            small = 0;
        }

        foreach (var actuator in actuators) actuator.SetForceFeedback(big, small);
    }

    /// <summary>
    ///     Refreshes the current state. Triggers <see cref="InputChanged" /> event.
    /// </summary>
    /// <returns>if the input was available</returns>
    public void RefreshInput()
    {
        try
        {
            joystickState = joystick.GetCurrentState();
            var changedSources = inputSources
                .Where(source => source.Refresh(joystickState))
                .Cast<InputSource>()
                .ToList();
            
            if (!changedSources.Any()) return;
            deviceInputChangedEventArgs.Refresh(changedSources);
            InputChanged?.Invoke(this, deviceInputChangedEventArgs);
        }
        catch (Exception)
        {
            Connected = false;
            Log.Warning($"Poll failed for {ToString()}");
        }
    }

    /// <summary>
    ///     Gets the current value of a DPad.
    /// </summary>
    /// <param name="dpadIndex">DPad index</param>
    /// <param name="state">Joystick state</param>
    /// <returns>Value</returns>
    private DPadDirection GetDPadValue(int dpadIndex, JoystickState state)
    {
        return state.PointOfViewControllers[dpadIndex] switch
        {
            -1 => DPadDirection.None,
            0 => DPadDirection.Up,
            4500 => DPadDirection.Up | DPadDirection.Right,
            9000 => DPadDirection.Right,
            13500 => DPadDirection.Down | DPadDirection.Right,
            18000 => DPadDirection.Down,
            22500 => DPadDirection.Down | DPadDirection.Left,
            27000 => DPadDirection.Left,
            31500 => DPadDirection.Up | DPadDirection.Left,
            _ => throw new ArgumentException(nameof(dpadIndex))
        };
    }

    private IEnumerable<DirectInputSource> GetButtonSources()
    {
        return joystick.GetObjects(DeviceObjectTypeFlags.Button)
            .Where(b => b.Usage > 0)
            .OrderBy(b => b.ObjectId.InstanceNumber)
            .Take(MaxButtons)
            .Select((b, i) => new DirectInputSource(
                inputDevice:this, 
                name: $"Button {b.Usage}",
                type: InputSourceTypes.Button, 
                offset: b.Offset, 
                state => state.Buttons[i] ? 1 : 0));
    }


    private IEnumerable<DirectInputSource> GetAxisSources()
    {
        return GetAxes()
            .OrderBy(a => a.Usage)
            .Take(MaxAxes)
            .Select(GetAxisSource);
    }

    private IEnumerable<DirectInputSource> GetSliderSources()
    {
        return GetSliders()
            .OrderBy(a => a.Usage)
            .Select(GetSliderSource);
    }

    private IEnumerable<DirectInputSource> GetDpadSources()
    {
        if (dpadCount <= 0) return Array.Empty<DirectInputSource>();
        return Enumerable.Range(0, dpadCount)
            .SelectMany(i => new DirectInputSource[]
            {
                new(this, $"DPad{i + 1} Up", InputSourceTypes.Dpad, DPadOffsetBase + i * 4,
                    state => GetDPadValue(i, state).HasFlag(DPadDirection.Up) ? 1 : 0),
                new(this, $"DPad{i + 1} Down", InputSourceTypes.Dpad, DPadOffsetBase + i * 4 + 1,
                    state => GetDPadValue(i, state).HasFlag(DPadDirection.Down) ? 1 : 0),
                new(this, $"DPad{i + 1} Left", InputSourceTypes.Dpad, DPadOffsetBase + i * 4 + 2,
                    state => GetDPadValue(i, state).HasFlag(DPadDirection.Left) ? 1 : 0),
                new(this, $"DPad{i + 1} Right", InputSourceTypes.Dpad, DPadOffsetBase + i * 4 + 3,
                    state => GetDPadValue(i, state).HasFlag(DPadDirection.Right) ? 1 : 0)
            });
    }

    private void InitializeJoystick()
    {
        joystick.Properties.AxisMode = DeviceAxisMode.Absolute;

        try
        {
            joystick.SetCooperativeLevel(new WindowInteropHelper(Application.Current.MainWindow).Handle,
                CooperativeLevel.Background | CooperativeLevel.Exclusive);
        }
        catch (Exception)
        {
            Log.Warning($"Failed to set cooperative level to exclusive for {ToString()}");
        }

        joystick.Acquire();
    }

    private void InitializeForceFeedback()
    {
        var supportsForceFeedback = deviceInstance.ForceFeedbackDriverGuid != Guid.Empty;
        if (!supportsForceFeedback) return;
        var force = joystick.GetEffects().FirstOrDefault(x => x.Guid == EffectGuid.ConstantForce)
                    ?? joystick.GetEffects().FirstOrDefault();
        var actuatorAxes = joystick.GetObjects()
            .Where(doi => doi.ObjectId.Flags.HasFlag(DeviceObjectTypeFlags.ForceFeedbackActuator))
            .ToArray();

        for (var i = 0; i < actuatorAxes.Length; i += 2)
        {
            var axis1 = actuatorAxes[i];
            var axis2 = (i + 1 < actuatorAxes.Length) ? actuatorAxes[i + 1] : null;
            actuators.Add(new DirectDeviceForceFeedback(joystick, force, axis1, axis2));
        }
    }

    private void LogDeviceInformation()
    {
        Log.Information(joystick.Properties.InstanceName + " " + ToString());
        foreach (var obj in joystick.GetObjects())
            Log.Information(
                $"  {obj.Name} {obj.ObjectId} offset: {obj.Offset} objecttype: {obj.ObjectType} {obj.Usage}");
    }

    ~DirectDevice()
    {
        Dispose();
    }

    /// <summary>
    ///     Display name followed by the deviceID.
    ///     <para>Overrides <see cref="object.ToString()" /></para>
    /// </summary>
    /// <returns>Friendly name</returns>
    public override string ToString()
    {
        return UniqueId;
    }

    /// <summary>
    ///     Gets the current value of an axis.
    /// </summary>
    /// <param name="axis">Axis index</param>
    /// <returns>Value</returns>
    private int GetAxisValue(int instanceNumber, JoystickState joystickState)
    {
        var currentState = joystickState;
        if (instanceNumber < 0) throw new ArgumentException(nameof(instanceNumber));
        switch (instanceNumber)
        {
            case 0:
                return currentState.X;
            case 1:
                return ushort.MaxValue - currentState.Y;
            case 2:
                return currentState.Z;
            case 3:
                return currentState.RotationX;
            case 4:
                return ushort.MaxValue - currentState.RotationY;
            case 5:
                return currentState.RotationZ;
            case 6:
                return currentState.AccelerationX;
            case 7:
                return ushort.MaxValue - currentState.AccelerationY;
            case 8:
                return currentState.AccelerationZ;
            case 9:
                return currentState.AngularAccelerationX;
            case 10:
                return ushort.MaxValue - currentState.AngularAccelerationY;
            case 11:
                return currentState.AngularAccelerationZ;
            case 12:
                return currentState.ForceX;
            case 13:
                return ushort.MaxValue - currentState.ForceY;
            case 14:
                return currentState.ForceZ;
            case 15:
                return currentState.TorqueX;
            case 16:
                return ushort.MaxValue - currentState.TorqueY;
            case 17:
                return currentState.TorqueZ;
            case 18:
                return currentState.VelocityX;
            case 19:
                return ushort.MaxValue - currentState.VelocityY;
            case 20:
                return currentState.VelocityZ;
            case 21:
                return currentState.AngularVelocityX;
            case 22:
                return ushort.MaxValue - currentState.AngularVelocityY;
            case 23:
                return currentState.AngularVelocityZ;
            default:
                return 0;
        }
    }

    /// <summary>
    ///     Gets the current value of a slider.
    /// </summary>
    /// <param name="slider">Slider index</param>
    /// <returns>Value</returns>
    private int GetSliderValue(int slider, JoystickState joystickState)
    {
        var currentState = joystickState;
        if (slider < 1) throw new ArgumentException(nameof(slider));
        return currentState.Sliders[slider - 1];
    }

    /// <summary>
    ///     Gets and initializes available axes for the device.
    /// </summary>
    /// <returns><see cref="DirectInputTypes" /> of the axes</returns>
    private IEnumerable<DeviceObjectInstance> GetAxes()
    {
        var axes = joystick.GetObjects(DeviceObjectTypeFlags.AbsoluteAxis).Where(o => o.ObjectType != ObjectGuid.Slider)
            .ToArray();
        foreach (var axis in axes)
        {
            var properties = joystick.GetObjectPropertiesById(axis.ObjectId);
            try
            {
                properties.Range = new InputRange(ushort.MinValue, ushort.MaxValue);
                properties.DeadZone = 0;
                properties.Saturation = 10000;
            }
            catch (SharpDXException ex)
            {
                Log.Error(ex, "Exception");
            }
        }

        return axes;
    }

    /// <summary>
    ///     Gets available sliders for the device.
    /// </summary>
    /// <returns><see cref="DirectInputTypes" /> of the axes</returns>
    private IEnumerable<DeviceObjectInstance> GetSliders()
    {
        return joystick.GetObjects().Where(o => o.ObjectType == ObjectGuid.Slider).ToArray();
    }

    private DirectInputSource GetAxisSource(DeviceObjectInstance instance)
    {
        var type = InputSourceTypes.AxisX;
        if (instance.ObjectType == ObjectGuid.XAxis || instance.ObjectType == ObjectGuid.RxAxis)
            type = InputSourceTypes.AxisX;
        else if (instance.ObjectType == ObjectGuid.YAxis || instance.ObjectType == ObjectGuid.RyAxis)
            type = InputSourceTypes.AxisY;
        else if (instance.ObjectType == ObjectGuid.ZAxis || instance.ObjectType == ObjectGuid.RzAxis)
            type = InputSourceTypes.AxisZ;
        int axisCount;
        if (instance.Usage >= 48)
            axisCount = instance.Usage - 48;
        else
            axisCount = instance.ObjectId.InstanceNumber;
        var name = instance.Name;
        return new DirectInputSource(this, name, type, instance.Offset,
            state => GetAxisValue(axisCount, state) / (double)ushort.MaxValue);
    }

    private DirectInputSource GetSliderSource(DeviceObjectInstance instance, int i)
    {
        var name = instance.Name;
        return new DirectInputSource(this, name, InputSourceTypes.Slider, instance.Offset,
            state => GetSliderValue(i + 1, state) / (double)ushort.MaxValue);
    }

    #region Events

    /// <summary>
    ///     Triggered periodically to trigger input read from Direct input device.
    ///     <para>Implements <see cref="IDevice.InputChanged" /></para>
    /// </summary>
    public event DeviceInputChangedHandler InputChanged;

    /// <summary>
    ///     Triggered when the any read or write fails.
    ///     <para>Implements <see cref="IInputDevice.Disconnected" /></para>
    /// </summary>
    public event DeviceDisconnectedHandler Disconnected;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the GUID of the controller.
    /// </summary>
    public Guid Id => deviceInstance.InstanceGuid;

    /// <summary>
    ///     <para>Implements <see cref="IInputDevice.UniqueId" /></para>
    /// </summary>
    public string UniqueId => deviceInstance.InstanceGuid.ToString();

    /// <summary>
    ///     Gets the product name of the device.
    ///     <para>Implements <see cref="IInputDevice.DisplayName" /></para>
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///     Gets or sets if the device is connected and ready to use.
    ///     <para>Implements <see cref="IInputDevice.Connected" /></para>
    /// </summary>
    public bool Connected
    {
        get => connected;
        set
        {
            if (value != connected)
            {
                if (!connected) Disconnected?.Invoke(this, new DeviceDisconnectedEventArgs());
                connected = value;
            }
        }
    }

    /// <summary>
    ///     <para>Implements <see cref="IDevice.DPads" /></para>
    /// </summary>
    public IEnumerable<DPadDirection> DPads => dpadSources.Select((source, i) => GetDPadValue(i, joystickState));

    /// <summary>
    ///     <para>Implements <see cref="IDevice.InputSources" /></para>
    /// </summary>
    public IEnumerable<InputSource> InputSources => inputSources;

    /// <summary>
    ///     <para>Implements <see cref="IInputDevice.ForceFeedbackCount" /></para>
    /// </summary>
    public int ForceFeedbackCount => actuators.Count;

    /// <summary>
    ///     <para>Implements <see cref="IInputDevice.InputConfiguration" /></para>
    /// </summary>
    public InputConfig InputConfiguration { get; }

    public string HardwareID
    {
        get
        {
            if (deviceInstance.IsHumanInterfaceDevice) return IdHelper.GetHardwareId(joystick.Properties.InterfacePath);
            return null;
        }
    }
    
    
    /// <summary>
    ///     Disposes all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            disposed = true;
        }
    }
    private void PrintCurrentUnixTimestamp()
    {
        var now = DateTimeOffset.Now;
        var unixTimestamp = now.ToUnixTimeSeconds();
        Log.Information(DisplayName + " " + unixTimestamp);
    }

    #endregion
}