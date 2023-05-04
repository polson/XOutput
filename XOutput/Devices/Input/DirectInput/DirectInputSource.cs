using System;
using SharpDX.DirectInput;

namespace XOutput.Devices.Input.DirectInput;

/// <summary>
///     Direct input source.
/// </summary>
public class DirectInputSource : InputSource
{
    private readonly Func<JoystickState, double> valueGetter;

    public DirectInputSource(IInputDevice inputDevice, string name, InputSourceTypes type, int offset,
        Func<JoystickState, double> valueGetter) : base(inputDevice, name, type, offset)
    {
        this.valueGetter = valueGetter;
    }

    internal bool Refresh(JoystickState state)
    {
        var newValue = valueGetter(state);
        return RefreshValue(newValue);
    }
}