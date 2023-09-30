using System.Windows;
using XOutput.Devices;
using XOutput.Devices.XInput;

namespace XOutput.UI.Windows;

public class AutoConfigureModel : ModelBase
{
    private Visibility buttonsVisibility;
    private bool highlight;
    private bool isAuto = true;
    private InputSource maxType;
    private double maxValue;
    private double minValue;
    private double timerMaxValue;
    private double timerValue;
    private Visibility timerVisibility;
    private XInputTypes xInput;

    public XInputTypes XInput
    {
        get => xInput;
        set
        {
            if (xInput != value)
            {
                xInput = value;
                OnPropertyChanged(nameof(XInput));
            }
        }
    }

    public bool IsAuto
    {
        get => isAuto;
        set
        {
            if (isAuto != value)
            {
                isAuto = value;
                OnPropertyChanged(nameof(IsAuto));
                if (value) MaxType = null;
            }
        }
    }

    public bool Highlight
    {
        get => highlight;
        set
        {
            if (highlight != value)
            {
                highlight = value;
                OnPropertyChanged(nameof(Highlight));
            }
        }
    }

    public InputSource MaxType
    {
        get => maxType;
        set
        {
            if (maxType != value)
            {
                maxType = value;
                OnPropertyChanged(nameof(MaxType));
            }
        }
    }

    public double MinValue
    {
        get => minValue;
        set
        {
            if (minValue != value)
            {
                minValue = value;
                OnPropertyChanged(nameof(MinValue));
            }
        }
    }

    public double MaxValue
    {
        get => maxValue;
        set
        {
            if (maxValue != value)
            {
                maxValue = value;
                OnPropertyChanged(nameof(MaxValue));
            }
        }
    }

    public Visibility ButtonsVisibility
    {
        get => buttonsVisibility;
        set
        {
            if (buttonsVisibility != value)
            {
                buttonsVisibility = value;
                OnPropertyChanged(nameof(ButtonsVisibility));
            }
        }
    }

    public double TimerMaxValue
    {
        get => timerMaxValue;
        set
        {
            if (timerMaxValue != value)
            {
                timerMaxValue = value;
                OnPropertyChanged(nameof(TimerMaxValue));
            }
        }
    }

    public double TimerValue
    {
        get => timerValue;
        set
        {
            if (timerValue != value)
            {
                timerValue = value;
                OnPropertyChanged(nameof(TimerValue));
            }
        }
    }

    public Visibility TimerVisibility
    {
        get => timerVisibility;
        set
        {
            if (timerVisibility != value)
            {
                timerVisibility = value;
                OnPropertyChanged(nameof(TimerVisibility));
            }
        }
    }
}