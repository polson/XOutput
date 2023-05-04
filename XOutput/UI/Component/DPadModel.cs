using XOutput.Devices;

namespace XOutput.UI.Component;

public class DPadModel : ModelBase
{
    private DPadDirection direction;

    private string label;

    private int valuex;

    private int valuey;

    public DPadDirection Direction
    {
        get => direction;
        set
        {
            if (direction != value)
            {
                direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }
    }

    public int ValueX
    {
        get => valuex;
        set
        {
            if (valuex != value)
            {
                valuex = value;
                OnPropertyChanged(nameof(ValueX));
            }
        }
    }

    public int ValueY
    {
        get => valuey;
        set
        {
            if (valuey != value)
            {
                valuey = value;
                OnPropertyChanged(nameof(ValueY));
            }
        }
    }

    public string Label
    {
        get => label;
        set
        {
            if (label != value)
            {
                label = value;
                OnPropertyChanged(nameof(Label));
            }
        }
    }
}