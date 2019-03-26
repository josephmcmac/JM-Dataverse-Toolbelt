#region

using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using System;
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
            SettingsClick = new MyCommand(() => { OpenSettings = true; });
            HelpClick = new MyCommand(() => { OpenHelp = true; });
        }

        public IEnumerable<IApplicationOption> GetAllOptions()
        {
            return Options.SelectMany(mg => mg.Options).ToArray();
        }

        public ObservableCollection<MenuGroupViewModel> Options { get; private set; }

        public ObservableCollection<ApplicationOption> Settings { get; private set; }

        public ObservableCollection<ApplicationOption> Helps { get; private set; }

        public void AddOption(string group, string optionLabel, Action action, string description = null, int order = 0)
        {
            var option = new ApplicationOption(optionLabel, action, description);
            
            if (group == "Setting")
                AddToSettingsCollection(optionLabel, action, description, order);
            else
            {
                if(!Options.Any(o => o.Label == group))
                {
                    AddToCollection(new MenuGroupViewModel(group, ApplicationController), Options);
                }
                var groupMenu = Options.First(o => o.Label == group);
                groupMenu.AddOption(optionLabel, action, description);
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

        private void AddToSettingsCollection(string optionLabel, Action action, string description, int order)
        {
            var option = new ApplicationOption(optionLabel, () => { OpenSettings = false; action(); }, description, order: order);
            var index = -1;
            if (!option.Label.IsNullOrWhiteSpace())
            {
                foreach (var item in Settings)
                {
                    if (order < item.Order
                        || (order == item.Order && String.Compare(option.Label, item.Label, StringComparison.Ordinal) < 0))
                    {
                        index = Settings.IndexOf(item);
                        break;
                    }
                }
            }
            if (index != -1)
                Settings.Insert(index, option);
            else
                Settings.Add(option);
        }

        public MyCommand SettingsClick { get; set; }

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

        public MyCommand HelpClick { get; set; }

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