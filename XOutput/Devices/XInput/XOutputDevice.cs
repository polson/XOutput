using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using XOutput.Devices.Input;
using XOutput.Devices.Mapper;


namespace XOutput.Devices.XInput;

/// <summary>
///     Device that contains data for a XInput device
/// </summary>
public sealed class XOutputDevice : IDevice
{
    private readonly DPadDirection[] dPads = new DPadDirection[DPadCount];
    private readonly InputMapper mapper;
    private readonly XOutputSource[] inputSources;

    private IEnumerable<IInputDevice> boundSources = new List<IInputDevice>();
    private readonly DeviceInputChangedEventArgs deviceInputChangedEventArgs;

    /// <summary>
    ///     Creates a new XDevice.
    /// </summary>
    /// <param name="source">Direct input device</param>
    /// <param name="mapper">DirectInput to XInput mapper</param>
    public XOutputDevice(InputMapper mapper)
    {
        this.mapper = mapper;
        inputSources = XInputHelper.Instance.GenerateSources();
        deviceInputChangedEventArgs = new DeviceInputChangedEventArgs(this);
    }

    #region Events

    /// <summary>
    ///     This event is invoked if the data from the device was updated
    ///     <para>Implements <see cref="IDevice.InputChanged" /></para>
    /// </summary>
    public event DeviceInputChangedHandler InputChanged;

    #endregion

    public void Dispose()
    {
        foreach (var source in boundSources) source.InputChanged -= SourceInputChanged;
    }

    /// <summary>
    ///     Gets the current state of the inputTpye.
    ///     <para>Implements <see cref="IDevice.Get(InputSource)" /></para>
    /// </summary>
    /// <param name="inputType">Type of input</param>
    /// <returns>Value</returns>
    public double Get(InputSource source)
    {
        return source.Value;
    }

    /// <summary>
    ///     Refreshes the current state. Triggers <see cref="InputChanged" /> event.
    /// </summary>
    /// <returns>if the input was available</returns>
    public void RefreshInput()
    {
        Log.Information(">> Refreshing XOutput device input");
        var changedSources = inputSources
            .Where(source => source.Refresh(mapper))
            .Cast<InputSource>()
            .ToList();
        
        dPads[0] = DPadHelper.GetDirection(
            GetBool(XInputTypes.UP),
            GetBool(XInputTypes.DOWN),
            GetBool(XInputTypes.LEFT),
            GetBool(XInputTypes.RIGHT));
        
        if (!changedSources.Any()) return;
        
        deviceInputChangedEventArgs.Refresh(changedSources);
        InputChanged?.Invoke(this, deviceInputChangedEventArgs);
    }

    ~XOutputDevice()
    {
        Dispose();
    }

    public void UpdateSources(IEnumerable<IInputDevice> sources)
    {
        Log.Information(">> Updating XOutput device sources: {0}", sources.Count());
        foreach (var source in boundSources) source.InputChanged -= SourceInputChanged;
        boundSources = sources;
        foreach (var source in boundSources) source.InputChanged += SourceInputChanged;
        RefreshInput();
    }

    private void SourceInputChanged(object sender, DeviceInputChangedEventArgs e)
    {
        RefreshInput();
    }

    /// <summary>
    ///     Gets a snapshot of data.
    /// </summary>
    /// <returns></returns>
    public Dictionary<XInputTypes, double> GetValues()
    {
        var newValues = new Dictionary<XInputTypes, double>();
        foreach (var source in inputSources) newValues[source.XInputType] = source.Value;
        return newValues;
    }

    /// <summary>
    ///     Gets boolean output.
    /// </summary>
    /// <param name="inputType">Type of input</param>
    /// <returns>boolean value</returns>
    public bool GetBool(XInputTypes inputType)
    {
        return Get(inputType) > 0.5;
    }

    /// <summary>
    ///     Gets the current state of the inputTpye.
    ///     <para>Implements <see cref="IDevice.Get(Enum)" /></para>
    /// </summary>
    /// <param name="inputType">Type of input</param>
    /// <returns>Value</returns>
    public double Get(Enum inputType)
    {
        var type = inputType as XInputTypes?;
        if (type.HasValue) return inputSources.First(s => s.XInputType == type.Value).Value;
        return 0;
    }

    #region Constants

    /// <summary>
    ///     XInput devices has 1 DPad.
    /// </summary>
    public const int DPadCount = 1;
    
    #endregion

    #region Properties

    /// <summary>
    ///     <para>Implements <see cref="IDevice.DPads" /></para>
    /// </summary>
    public IEnumerable<DPadDirection> DPads => dPads;

    /// <summary>
    ///     <para>Implements <see cref="IDevice.Buttons" /></para>
    /// </summary>
    public IEnumerable<InputSource> InputSources => inputSources;

    #endregion
}