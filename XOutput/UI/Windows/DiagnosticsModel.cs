using System.Collections.Generic;
using System.Collections.ObjectModel;
using XOutput.Diagnostics;
using XOutput.UI.Component;

namespace XOutput.UI.Windows;

public class DiagnosticsModel : ModelBase
{
    public DiagnosticsModel(IEnumerable<IDiagnostics> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            Diagnostics.Add(
                new DiagnosticsItemView(new DiagnosticsItemViewModel(new DiagnosticsItemModel(), diagnostic)));
    }

    public ObservableCollection<DiagnosticsItemView> Diagnostics { get; } = new();
}