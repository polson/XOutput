﻿using XOutput.Devices.XInput;

namespace XOutput.UI;

public class XBoxModel : ModelBase
{
    private bool highlight;
    private XInputTypes xInputType;

    public XInputTypes XInputType
    {
        get => xInputType;
        set
        {
            if (xInputType != value)
            {
                xInputType = value;
                OnPropertyChanged(nameof(XInputType));
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
}