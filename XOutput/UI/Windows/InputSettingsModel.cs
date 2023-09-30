using System.Collections.ObjectModel;
using XOutput.UI.Component;

namespace XOutput.UI.Windows;

public class InputSettingsModel : ModelBase
{
    private bool forceFeedbackAvailable;

    private bool forceFeedbackEnabled;

    private string forceFeedbackText;

    private string testButtonText;


    private string title;
    public ObservableCollection<IUpdatableView> InputAxisViews { get; } = new();

    public ObservableCollection<IUpdatableView> InputDPadViews { get; } = new();

    public ObservableCollection<IUpdatableView> InputButtonViews { get; } = new();

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

    public string ForceFeedbackText
    {
        get => forceFeedbackText;
        set
        {
            if (forceFeedbackText != value)
            {
                forceFeedbackText = value;
                OnPropertyChanged(nameof(ForceFeedbackText));
            }
        }
    }

    public string TestButtonText
    {
        get => testButtonText;
        set
        {
            if (testButtonText != value)
            {
                testButtonText = value;
                OnPropertyChanged(nameof(TestButtonText));
            }
        }
    }

    public bool ForceFeedbackEnabled
    {
        get => forceFeedbackEnabled;
        set
        {
            if (forceFeedbackEnabled != value)
            {
                forceFeedbackEnabled = value;
                OnPropertyChanged(nameof(ForceFeedbackEnabled));
            }
        }
    }

    public bool ForceFeedbackAvailable
    {
        get => forceFeedbackAvailable;
        set
        {
            if (forceFeedbackAvailable != value)
            {
                forceFeedbackAvailable = value;
                OnPropertyChanged(nameof(ForceFeedbackAvailable));
            }
        }
    }
}