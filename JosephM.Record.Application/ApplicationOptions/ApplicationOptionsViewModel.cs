#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.HTML;
using JosephM.Record.Application.Navigation;

#endregion

namespace JosephM.Record.Application.ApplicationOptions
{
    /// <summary>
    ///     The Active Menu Options In The Appplication
    /// </summary>
    public class ApplicationOptionsViewModel : ViewModelBase
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

        public void AddOption(string optionLabel, string menu, Action action)
        {
            var option = new ApplicationOption(optionLabel, menu, action, ApplicationOptionType.Main);
            AddToCollection(option, Options);
        }

        public void AddSetting(string optionLabel, string menu, Action action)
        {
            var option = new ApplicationOption(optionLabel, menu, action, ApplicationOptionType.Setting);
            AddToCollection(option, Settings);
            OnPropertyChanged("HasSettings");
        }

        /// <summary>
        /// !!! YOU NEED TO HAVE THE HTML FILE IN A FOLDER NAMED "HelpFiles" INCLUDED IN THE PROJECT BUILD COPY TO OUTPUT
        /// Then ensure the applications build folder is included in the installer folders
        /// </summary>
        public void AddHelp(string optionLabel, string htmlFileName)
        {
            var option = new ApplicationOption(optionLabel, "Help", () => HelpCommand(htmlFileName),
                ApplicationOptionType.Help);
            AddToCollection(option, Helps);
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

        private void HelpCommand(string htmlFileName)
        {
            var query = new UriQuery();
            query.Add("path", System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpFiles", htmlFileName));
            NavigateTo<HtmlFileModel>(query);
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