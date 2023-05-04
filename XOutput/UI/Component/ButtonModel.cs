using XOutput.Devices;

namespace XOutput.UI.Component;

public class ButtonModel : ModelBase
{
    private InputSource type;
    private bool value;

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

    public bool Value
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
}