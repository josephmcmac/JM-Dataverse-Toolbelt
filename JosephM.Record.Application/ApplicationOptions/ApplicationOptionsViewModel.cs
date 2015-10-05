#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Core.Extentions;
using Microsoft.Practices.Prism.Commands;

#endregion

namespace JosephM.Application.ViewModel.ApplicationOptions
{
    /// <summary>
    ///     The Active Menu Options In The Appplication
    /// </summary>
    public class ApplicationOptionsViewModel : ViewModelBase, IApplicationOptions
    {
        public ApplicationOptionsViewModel(IApplicationController controller)
            : base(controller)
        {
            Options = new ObservableCollection<ApplicationOption>();
            Settings = new ObservableCollection<ApplicationOption>();
            Helps =new ObservableCollection<ApplicationOption>();
            SettingsClick = new DelegateCommand(() => { OpenSettings = true; });
            HelpClick = new DelegateCommand(() => { OpenHelp = true; });
        }

        public ObservableCollection<ApplicationOption> Options { get; private set; }

        public ObservableCollection<ApplicationOption> Settings { get; private set; }

        public ObservableCollection<ApplicationOption> Helps { get; private set; }

        public void AddOption(string optionLabel, Action action, ApplicationOptionType type)
        {
            var option = new ApplicationOption(optionLabel, action, type);
            if(type == ApplicationOptionType.Help)
                AddToCollection(option, Helps);
            else if (type == ApplicationOptionType.Setting)
                AddToCollection(option, Settings);
            else
                AddToCollection(option, Options);
           
            OnPropertyChanged("HasSettings");
            OnPropertyChanged("HasHelp");
        }

        private void AddToCollection(ApplicationOption option, ObservableCollection<ApplicationOption> options)
        {
            var index = -1;
            if (!option.Label.IsNullOrWhiteSpace())
            {
                foreach (var item in options)
                {
                    if (String.Compare(option.Label, item.Label, StringComparison.Ordinal) < 0)
                    {
                        index = options.IndexOf(item);
                        break;
                    }
                }
            }
            if(index != -1)
                options.Insert(index, option);
            else
                options.Add(option);
        }

        public DelegateCommand SettingsClick { get; set; }

        private bool _openSettings;

        public bool OpenSettings
        {
            get { return _openSettings; }
            set
            {
                _openSettings = value;
                OnPropertyChanged("OpenSettings");
            }
        }

        public bool HasSettings
        {
            get { return Settings.Any(); }
        }

        public DelegateCommand HelpClick { get; set; }

        private bool _openHelp;

        public bool OpenHelp
        {
            get { return _openHelp; }
            set
            {
                _openHelp = value;
                OnPropertyChanged("OpenHelp");
            }
        }

        public bool HasHelp
        {
            get { return Helps.Any(); }
        }
    }
}