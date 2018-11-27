using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.XrmModule.XrmConnection;
using System;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using System.Windows.Forms;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmPackageSettingsModule : SettingsModule<XrmPackageSettingsDialog, XrmPackageSettings, XrmPackageSettings>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddCopyNewCodeButton();
        }

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(XrmPackageSettingsDialog), null);
        }

        private void AddCopyNewCodeButton()
        {
            this.AddCustomFormFunction(new CustomFormFunction("CopyCode", "Copy C# To Clipboard", (r) =>
            {
                try
                {
                    Clipboard.SetText(r.GetStringFieldFieldViewModel(nameof(SettingsFolderMoving.WithTheseLines)).Value);
                }
                catch(Exception ex)
                {
                    r.ApplicationController.ThrowException(ex);
                }
            }), typeof(SettingsFolderMoving));
        }
    }
}