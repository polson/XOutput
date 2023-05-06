using XOutput.Devices.Input;
using XOutput.Devices.XInput;

namespace XOutput.Devices;

/// <summary>
///     Main class for sources.
/// </summary>
public abstract class InputSource
{
    protected IInputDevice inputDevice;
    protected string name;
    protected int offset;
    protected InputSourceTypes type;
    protected double value;

    protected InputSource(IInputDevice inputDevice, string name, InputSourceTypes type, int offset)
    {
        this.inputDevice = inputDevice;
        this.name = name;
        this.type = type;
        this.offset = offset;
    }

    /// <summary>
    ///     The display name of the source.
    /// </summary>
    public string DisplayName => name;

    /// <summary>
    ///     The type of the source.
    /// </summary>
    public InputSourceTypes Type => type;

    /// <summary>
    ///     The device of the source.
    /// </summary>
    public IInputDevice InputDevice => inputDevice;

    /// <summary>
    ///     The offset of the source.
    /// </summary>
    public int Offset => offset;

    /// <summary>
    ///     The value of the source.
    /// </summary>
    public double Value => value;

    /// <summary>
    ///     If the input is an axis.
    /// </summary>
    public bool IsAxis => InputSourceTypes.Axis.HasFlag(type);

    /// <summary>
    ///     If the input is a button.
    /// </summary>
    public bool IsButton => type == InputSourceTypes.Button;

    /// <summary>
    ///     This event is invoked if the data from the source was updated.
    /// </summary>
    public event SourceChangedEventHandler InputChanged;

    public override string ToString()
    {
        return name;
    }

    protected void InvokeChange()
    {
        InputChanged?.Invoke(this, null);
    }

    protected bool UpdateValue(double newValue)
    {
        if (newValue != value)
        {
            value = newValue;
            InvokeChange();
            return true;
        }

        return false;
    }

    public double Get(XInputTypes type)
    {
        if (inputDevice != null) return inputDevice.Get(this);
        return type.GetDisableValue();
    }
}

public class DisabledInputSource : InputSource
{
    private DisabledInputSource() : base(null, "", InputSourceTypes.Disabled, 0)
    {
    }

    public static InputSource Instance { get; } = new DisabledInputSource();
}