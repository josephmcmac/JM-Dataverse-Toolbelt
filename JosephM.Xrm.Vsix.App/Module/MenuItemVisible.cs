using JosephM.Application.Application;
using JosephM.Xrm.Vsix.App.Application;
using System;

namespace JosephM.Xrm.Vsix.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class MenuItemVisible : Attribute
    {
        public abstract bool IsVisible(IApplicationController applicationController);

        public void Process(IApplicationController applicationController, IMenuCommand menuCommand)
        {
            SetHidden(menuCommand);
            if (IsVisible(applicationController))
                SetVisible(menuCommand);
        }

        private void SetVisible(IMenuCommand menuCommand)
        {
            if (menuCommand != null)
            {
                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void SetHidden(IMenuCommand menuCommand)
        {
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
            }
        }
    }
}
