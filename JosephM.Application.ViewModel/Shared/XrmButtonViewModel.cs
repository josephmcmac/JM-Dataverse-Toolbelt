#region

using JosephM.Application.Application;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace JosephM.Application.ViewModel.Shared
{
    public class XrmButtonViewModel : ViewModelBase
    {
        private bool _saveButtonVisible = true;

        public Action ClickAction { get; set; }

        public XrmButtonViewModel(string label, Action clickAction, IApplicationController applicationController, string description = null)
            : this(label, label, clickAction, applicationController, description)
        {
        }

        public XrmButtonViewModel(string id, string label, Action clickAction, IApplicationController applicationController, string description = null)
    : base(applicationController)
        {
            Id = id;
            Label = label;
            ClickAction = clickAction;
            Command = new MyCommand(clickAction);
            Description = description;
        }

        public XrmButtonViewModel(string id, string label, IEnumerable<XrmButtonViewModel> childButtons, IApplicationController applicationController)
            : base(applicationController)
        {
            Id = id;
            Label = label;
            Command = new MyCommand(() => { OpenChildButtons = true; });
            ChildButtons = childButtons;
            foreach(var button in childButtons)
            {
                button.Command = new MyCommand(() => { OpenChildButtons = false; button.ClickAction(); });
            }
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

        public bool HasChildOptions {  get { return ChildButtons != null && ChildButtons.Any(); } }
        public IEnumerable<XrmButtonViewModel> ChildButtons { get; set; }

        public string Id { get; set; }

        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public MyCommand Command { get; private set; }

        public bool IsVisible
        {
            get { return _saveButtonVisible; }
            set
            {
                _saveButtonVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public void Invoke()
        {
            Command.Execute();
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
}