using XOutput.Devices;

namespace XOutput.UI.Component;

public class Axis2DModel : ModelBase
{
    private int maxx;

    private int maxy;

    private int valuex;

    private int valuey;
    public InputSource TypeX { get; set; }
    public InputSource TypeY { get; set; }

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

    public int MaxX
    {
        get => maxx;
        set
        {
            if (maxx != value)
            {
                maxx = value;
                OnPropertyChanged(nameof(MaxX));
            }
        }
    }

    public int MaxY
    {
        get => maxy;
        set
        {
            if (maxy != value)
            {
                maxy = value;
                OnPropertyChanged(nameof(MaxY));
            }
        }
    }
}