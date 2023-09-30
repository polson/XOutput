using System.Collections.Generic;
using System.Linq;
using Serilog;

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

    public IInputDevice GetDeviceByName(string deviceName)
    {
        return inputDevices.FirstOrDefault(device => device.DisplayName == deviceName);
    }
    
    public IInputDevice GetDeviceByGuid(string guid)
    {
        return inputDevices.FirstOrDefault(device => device.UniqueId == guid);
    }

    public IEnumerable<IInputDevice> GetDevices()
    {
        return inputDevices.ToArray();
    }
}