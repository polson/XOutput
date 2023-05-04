using XOutput.Devices;

namespace XOutput.UI.Component;

public class AxisModel : ModelBase
{
    private int max;
    private InputSource type;
    private int value;

    public InputSource Type
    {
        get => type;
        set
        {
            if (type != value)
            {
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }
    }

    public int Value
    {
        get => value;
        set
        {
            if (this.value != value)
            {
                this.value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public int Max
    {
        get => max;
        set
        {
            if (max != value)
            {
                max = value;
                OnPropertyChanged(nameof(Max));
            }
        }
    }
}