using System.Collections.ObjectModel;
using XOutput.Tools;
using XOutput.UI.Component;

namespace XOutput.UI.Windows;

public class MainWindowModel : ModelBase
{
    private Settings settings;

    public Settings Settings
    {
        get => settings;
        set
        {
            if (settings != value)
            {
                settings = value;
                OnPropertyChanged(nameof(AllDevices));
            }
        }
    }

    public ObservableCollection<InputView> Inputs { get; } = new();

    public bool AllDevices
    {
        get => settings?.ShowAll ?? false;
        set
        {
            if (settings != null && settings.ShowAll != value)
            {
                settings.ShowAll = value;
                OnPropertyChanged(nameof(AllDevices));
            }
        }
    }

    public ObservableCollection<ControllerView> Controllers { get; } = new();
}