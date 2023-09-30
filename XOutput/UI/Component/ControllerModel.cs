using System.Windows.Media;
using XOutput.Devices;

namespace XOutput.UI.Component;

public class ControllerModel : ModelBase
{
    private Brush background;

    private string buttonText;

    private bool canStart;
    private GameController controller;

    private int selectedOutputIndex;
    private bool started;

    public GameController Controller
    {
        get => controller;
        set
        {
            if (controller != value)
            {
                controller = value;
                OnPropertyChanged(nameof(Controller));
            }
        }
    }

    public string ButtonText
    {
        get => buttonText;
        set
        {
            if (buttonText != value)
            {
                buttonText = value;
                OnPropertyChanged(nameof(ButtonText));
            }
        }
    }

    public bool Started
    {
        get => started;
        set
        {
            if (started != value)
            {
                started = value;
                OnPropertyChanged(nameof(Started));
            }
        }
    }

    public bool CanStart
    {
        get => canStart;
        set
        {
            if (canStart != value)
            {
                canStart = value;
                OnPropertyChanged(nameof(CanStart));
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

    public string DisplayName => Controller.ToString();

    public int SelectedOutputIndex
    {
        // get => OutputDevices.Instance.GetDevices().IndexOf(controller.XOutputInterface);
        get => selectedOutputIndex;
        set
        {
            if (selectedOutputIndex != value)
            {
                selectedOutputIndex = value;
                OnPropertyChanged(nameof(SelectedOutputIndex));
            }
        }
    }


    public void RefreshName()
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}