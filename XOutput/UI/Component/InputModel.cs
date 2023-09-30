using System.Windows.Media;
using XOutput.Devices.Input;

namespace XOutput.UI.Component;

public class InputModel : ModelBase
{
    private Brush background;
    private IInputDevice device;

    public IInputDevice Device
    {
        get => device;
        set
        {
            if (device != value)
            {
                device = value;
                OnPropertyChanged(nameof(Device));
            }
        }
    }

    public Brush Background
    {
        get => background;
        set
        {
            if (background != value)
            {
                background = value;
                OnPropertyChanged(nameof(Background));
            }
        }
    }

    public string DisplayName => string.Format("{0} ({1})", device.DisplayName, device.UniqueId);
}