using JosephM.Xrm.Vsix.App.Application;
using Microsoft.VisualStudio.Shell;
using System;

namespace JosephM.Xrm.Vsix.Application
{
    public class MenuCommand : IMenuCommand
    {
        private readonly OleMenuCommand _oleMenuCommand;

        public MenuCommand(OleMenuCommand oleMenuCommand)
        {
            _oleMenuCommand = oleMenuCommand;
        }

        public bool Visible
        {
            get
            {
                return _oleMenuCommand.Visible;
            }
            set
            {
                _oleMenuCommand.Visible = value;
            }
        }
        public bool Enabled
        {
            get
            {
                return _oleMenuCommand.Enabled;
            }
            set
            {
                _oleMenuCommand.Enabled = value;
            }
        }

        public void AddBeforeQueryStatusHandler(EventHandler eventHandler)
        {
            _oleMenuCommand.BeforeQueryStatus += eventHandler;
        }
    }
}
