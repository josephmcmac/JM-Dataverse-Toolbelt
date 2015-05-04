#region

using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Record.Application.Shared
{
    public class XrmButtonViewModel : ViewModelBase
    {
        private bool _saveButtonVisible = true;

        public XrmButtonViewModel(string label, Action clickAction, IApplicationController applicationController)
            : base(applicationController)
        {
            Label = label;
            Command = new DelegateCommand(clickAction);
        }

        public string Label { get; set; }
        public ICommand Command { get; private set; }

        public bool IsVisible
        {
            get { return _saveButtonVisible; }
            set
            {
                _saveButtonVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }
    }
}