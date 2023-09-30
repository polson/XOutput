using System.Collections.Generic;
using System.Linq;
using Serilog;
using XOutput.Devices.XInput;
using XOutput.Devices.XInput.Vigem;


namespace XOutput.Devices;

public class OutputDevices
{
    public const int MaxOutputDevices = 4;

    private readonly List<IXOutputInterface> outputDevices = new();

    private OutputDevices()
    {
        InitializeDevices();
    }

    public static OutputDevices Instance { get; } = new();

    private void InitializeDevices()
    {
        if (outputDevices.Any()) return;
        for (var i = 0; i < MaxOutputDevices; i++)
        {
            var device = CreateDevice();
            device.Plugin(i);
            outputDevices.Add(device);
        }
    }

    private IXOutputInterface CreateDevice()
    {
        if (VigemDevice.IsAvailable())
        {
            Log.Information("ViGEm devices are used.");
            return new VigemDevice();
        }

        Log.Error("Neither ViGEm nor SCP devices can be used.");
        return null;
    }

    public List<IXOutputInterface> GetDevices()
    {
        return outputDevices;
    }
}