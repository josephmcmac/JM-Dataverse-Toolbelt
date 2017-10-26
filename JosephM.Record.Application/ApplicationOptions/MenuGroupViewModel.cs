#region

using System;
using JosephM.Application.Options;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using JosephM.Application.Application;
using System.Linq;

#endregion

namespace JosephM.Application.ViewModel.ApplicationOptions
{
    /// <summary>
    ///     An Option Available In Menus For The Appplication
    /// </summary>
    public class MenuGroupViewModel : ViewModelBase
    {
        public MenuGroupViewModel(string label, IApplicationController controller)
            : base(controller)
        {
            Label = label;
            Options = new ObservableCollection<ApplicationOption>();
            DelegateCommand = new DelegateCommand(OpenChildren);
        }

        private void OpenChildren()
        {
            OpenChildButtons = true;
        }

        private bool _openChildButtons;
        public bool OpenChildButtons
        {
            get
            {
                return _openChildButtons;
            }
            set
            {
                _openChildButtons = value;
                OnPropertyChanged(nameof(OpenChildButtons));
            }
        }

        public bool HasOneOption
        {
            get
            {
                return Options.Count == 1;
            }
        }

        public ApplicationOption FirstOption
        {
            get
            {
                return Options.Any() ? Options.First() : null;
            }
        }

        public ObservableCollection<ApplicationOption> Options { get; private set; }

        public DelegateCommand DelegateCommand { get; private set; }

        public string Label { get; private set; }

        public void AddOption(ApplicationOption option)
        {
            var index = -1;
            if (!string.IsNullOrWhiteSpace(option.Label))
            {
                foreach (var item in Options)
                {
                    if (String.Compare(option.Label, item.Label, StringComparison.Ordinal) < 0)
                    {
                        index = Options.IndexOf(item);
                        break;
                    }
                }
            }
            if (index != -1)
                Options.Insert(index, option);
            else
                Options.Add(option);
            OnPropertyChanged(nameof(HasOneOption));
        }
    }
}