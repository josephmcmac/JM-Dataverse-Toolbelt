using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Application;
using Microsoft.VisualStudio.Shell;
using System;

namespace JosephM.Xrm.Vsix.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class MenuItemVisible : Attribute
    {
        public abstract bool IsVisible(IApplicationController applicationController);

        public void Process(IApplicationController applicationController, OleMenuCommand menuCommand)
        {
            SetHidden(menuCommand);
            if (IsVisible(applicationController))
                SetVisible(menuCommand);
        }

        private void SetVisible(OleMenuCommand menuCommand)
        {
            if (menuCommand != null)
            {
                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void SetHidden(OleMenuCommand menuCommand)
        {
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
            }
        }
    }
}
