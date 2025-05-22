using JosephM.Application.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            foreach (var button in ChildButtons)
            {
                button.ParentButton = this;
            }
        }

        public XrmButtonViewModel ParentButton { get; set; }

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
                //cascade close of dropdown closing to parent if it doesn't have any other child branch open
                if (ParentButton != null)
                {
                    ApplicationController.DoOnAsyncThread(() =>
                    {
                        Thread.Sleep(200);
                        if(!ParentButton.ChildButtons.Any(cb => cb.OpenChildButtons))
                        {
                            ParentButton.OpenChildButtons = false;
                        }
                    });
                }
            }
        }

        public bool HasChildOptions { get { return ChildButtons != null && ChildButtons.Any(); } }
        public IEnumerable<XrmButtonViewModel> ChildButtons
        {
            get { return _childButtons; }
            set
            {
                _childButtons = value;
                if (value != null)
                {
                    foreach (var button in value)
                    {
                        if (!button.HasChildOptions)
                        {
                            button.Command = new MyCommand(() => { OpenChildButtons = false; button.ClickAction(); });
                        }
                    }
                }
                OnPropertyChanged(nameof(ChildButtons));
            }
        }

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
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public void Invoke()
        {
            Command.Execute();
        }

        private string _description;
        private IEnumerable<XrmButtonViewModel> _childButtons;

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