using System.Collections.ObjectModel;
using System.Windows.Controls;
using XOutput.UI.Component;

namespace XOutput.UI.Windows;

public class ControllerSettingsModel : ModelBase
{
    private ComboBoxItem forceFeedback;

    private bool startWhenConnected;

    private string title;
    public ObservableCollection<MappingView> MapperAxisViews { get; } = new();

    public ObservableCollection<MappingView> MapperDPadViews { get; } = new();

    public ObservableCollection<MappingView> MapperButtonViews { get; } = new();

    public ObservableCollection<IUpdatableView> XInputAxisViews { get; } = new();

    public ObservableCollection<IUpdatableView> XInputDPadViews { get; } = new();

    public ObservableCollection<IUpdatableView> XInputButtonViews { get; } = new();

    public ObservableCollection<ComboBoxItem> ForceFeedbacks { get; } = new();

    public ComboBoxItem ForceFeedback
    {
        get => forceFeedback;
        set
        {
            if (forceFeedback != value)
            {
                forceFeedback = value;
                OnPropertyChanged(nameof(ForceFeedback));
            }
        }
    }

    public string Title
    {
        get => title;
        set
        {
            if (title != value)
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public bool StartWhenConnected
    {
        get => startWhenConnected;
        set
        {
            if (startWhenConnected != value)
            {
                startWhenConnected = value;
                OnPropertyChanged(nameof(StartWhenConnected));
            }
        }
    }
}