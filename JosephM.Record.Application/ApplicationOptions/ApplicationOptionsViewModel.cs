#region

using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Core.Extentions;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            Options = new ObservableCollection<MenuGroupViewModel>();
            Settings = new ObservableCollection<ApplicationOption>();
            Helps = new ObservableCollection<ApplicationOption>();
            SettingsClick = new DelegateCommand(() => { OpenSettings = true; });
            HelpClick = new DelegateCommand(() => { OpenHelp = true; });
        }

        public IEnumerable<IApplicationOption> GetAllOptions()
        {
            return Options.SelectMany(mg => mg.Options).ToArray();
        }

        public ObservableCollection<MenuGroupViewModel> Options { get; private set; }

        public ObservableCollection<ApplicationOption> Settings { get; private set; }

        public ObservableCollection<ApplicationOption> Helps { get; private set; }

        public void AddOption(string group, string optionLabel, Action action, string description = null)
        {
            var option = new ApplicationOption(optionLabel, action, description);
            
            if (group == "Help")
                AddToCollection(option, Helps);
            else if (group == "Setting")
                AddToCollection(option, Settings);
            else
            {
                if(!Options.Any(o => o.Label == group))
                {
                    AddToCollection(new MenuGroupViewModel(group, ApplicationController), Options);
                }
                var groupMenu = Options.First(o => o.Label == group);
                groupMenu.AddOption(option);
            }

            OnPropertyChanged("HasSettings");
            OnPropertyChanged("HasHelp");
        }

        private void AddToCollection(MenuGroupViewModel menuGroup, ObservableCollection<MenuGroupViewModel> menuGroups)
        {
            var forceLastLabel = "Other";
            if (menuGroup.Label == forceLastLabel)
                menuGroups.Add(menuGroup);
            else
            {
                var index = -1;
                if (!menuGroup.Label.IsNullOrWhiteSpace())
                {
                    foreach (var item in menuGroups)
                    {
                        if (item.Label == forceLastLabel || String.Compare(menuGroup.Label, item.Label, StringComparison.Ordinal) < 0)
                        {
                            index = menuGroups.IndexOf(item);
                            break;
                        }
                    }
                }
                if (index != -1)
                    menuGroups.Insert(index, menuGroup);
                else
                    menuGroups.Add(menuGroup);
            }
        }

        private void AddToCollection(ApplicationOption option, ObservableCollection<ApplicationOption> options)
        {
            var forceLastLabel = "About";
            if (option.Label == forceLastLabel)
                options.Add(option);
            else
            {
                var index = -1;
                if (!option.Label.IsNullOrWhiteSpace())
                {
                    foreach (var item in options)
                    {
                        if (item.Label == forceLastLabel || String.Compare(option.Label, item.Label, StringComparison.Ordinal) < 0)
                        {
                            index = options.IndexOf(item);
                            break;
                        }
                    }
                }
                if (index != -1)
                    options.Insert(index, option);
                else
                    options.Add(option);
            }
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

        public string ApplicationName
        {
            get { return ApplicationController.ApplicationName; }
        }
    }
}