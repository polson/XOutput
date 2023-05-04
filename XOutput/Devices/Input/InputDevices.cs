using System.Collections.Generic;

namespace XOutput.Devices.Input;

public class InputDevices
{
    private readonly List<IInputDevice> inputDevices = new();

    private InputDevices()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the class.
    /// </summary>
    public static InputDevices Instance { get; } = new();

    public void Add(IInputDevice inputDevice)
    {
        inputDevices.Add(inputDevice);
        Controllers.Instance.Update(inputDevices);
    }

    public void Remove(IInputDevice inputDevice)
    {
        inputDevices.Remove(inputDevice);
        Controllers.Instance.Update(inputDevices);
    }

    public IEnumerable<IInputDevice> GetDevices()
    {
        return inputDevices.ToArray();
    }
}