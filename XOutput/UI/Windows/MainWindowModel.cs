﻿using System.Collections.ObjectModel;
using XOutput.Tools;
using XOutput.UI.Component;

namespace XOutput.UI.Windows
{
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

        private readonly ObservableCollection<InputView> inputs = new ObservableCollection<InputView>();
        public ObservableCollection<InputView> Inputs { get { return inputs; } }

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

        private bool isAdmin;
        public bool IsAdmin
        {
            get => isAdmin;
            set
            {
                if (isAdmin != value)
                {
                    isAdmin = value;
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        private readonly ObservableCollection<ControllerView> controllers = new ObservableCollection<ControllerView>();
        public ObservableCollection<ControllerView> Controllers { get { return controllers; } }
    }
}
