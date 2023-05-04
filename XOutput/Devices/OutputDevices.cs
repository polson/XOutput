using System.Collections.Generic;
using System.Linq;
using XOutput.Devices.XInput;
using XOutput.Devices.XInput.Vigem;
using XOutput.Logging;

namespace XOutput.Devices
{
    public class OutputDevices
    {
        private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(OutputDevices));

        public static OutputDevices Instance { get; } = new();

        private readonly List<IXOutputInterface> outputDevices = new();
        public const int MaxOutputDevices = 4;
        
        private OutputDevices()
        {
            InitializeDevices();
        }

        private void InitializeDevices()
        {
            if (outputDevices.Any())
            {
                return;
            }
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
                logger.Info("ViGEm devices are used.");
                return new VigemDevice();
            }
            else
            {
                logger.Error("Neither ViGEm nor SCP devices can be used.");
                return null;
            }
        }

        public List<IXOutputInterface> GetDevices()
        {
            return outputDevices;
        }
    }
}