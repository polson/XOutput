using System.Collections.ObjectModel;
using XOutput.Diagnostics;

namespace XOutput.UI;

public class DiagnosticsItemModel : ModelBase
{
    private string source;
    public ObservableCollection<DiagnosticsResult> Results { get; } = new();

    public string Source
    {
        get => source;
        set
        {
            if (source != value)
            {
                source = value;
                OnPropertyChanged(nameof(Source));
            }
        }
    }
}