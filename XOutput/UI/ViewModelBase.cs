using System.ComponentModel;

namespace XOutput.UI;

public abstract class ViewModelBase<M> where M : ModelBase, INotifyPropertyChanged
{
    protected ViewModelBase(M model)
    {
        this.Model = model;
    }

    public LanguageModel LanguageModel => LanguageModel.Instance;
    public M Model { get; }
}