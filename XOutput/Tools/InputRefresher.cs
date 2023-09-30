using System.Collections.Generic;
using System.Threading.Tasks;
using XOutput.Devices;
using XOutput.Devices.Input;

namespace XOutput.Tools;

public class InputRefresher
{
    private System.Timers.Timer timer;
    
    public InputRefresher(double interval)
    {
        timer = new System.Timers.Timer(interval);
        timer.Elapsed += (sender, e) =>
        {
            foreach (var inputDevice in InputDevices.Instance.GetDevices())
            {
                if (inputDevice.Connected)
                {
                    Task.Run(() => inputDevice.RefreshInput());
                }
            }
        };
    }
    
    public void Start()
    {
        timer.Start();
    }
    
    public void Stop()
    {
        timer.Stop();
    }
}