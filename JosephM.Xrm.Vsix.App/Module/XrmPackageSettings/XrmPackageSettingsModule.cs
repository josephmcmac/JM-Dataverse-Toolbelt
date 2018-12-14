using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.XrmModule.XrmConnection;
using System;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using System.Windows.Forms;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmPackageSettingsModule : SettingsModule<XrmPackageSettingsDialog, XrmPackageSettings, XrmPackageSettings>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddCopyNewCodeButton();
            InitialisePrefixFields();
        }

        private void InitialisePrefixFields()
        {
            this.AddOnChangeFunction(new OnChangeFunction((recordForm, fieldName) =>
            {
                switch(fieldName)
                {
                    case nameof(XrmPackageSettings.Solution):
                        {
                            var solutionViewModel = recordForm.GetLookupFieldFieldViewModel(nameof(XrmPackageSettings.Solution));
                            var dynamicsPrefixViewModel = recordForm.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionDynamicsCrmPrefix));
                            if(solutionViewModel.Value != null && string.IsNullOrWhiteSpace(dynamicsPrefixViewModel.Value))
                            {
                                var lookupService = solutionViewModel.LookupService;
                                var solution = lookupService.Get(solutionViewModel.Value.RecordType, solutionViewModel.Value.Id);
                                var publisherId = solution.GetLookupId(Fields.solution_.publisherid);
                                if(publisherId != null)
                                {
                                    var publisher = lookupService.Get(Entities.publisher, publisherId);
                                    dynamicsPrefixViewModel.Value = publisher.GetStringField(Fields.publisher_.customizationprefix);
                                }
                            }
                            break;
                        }
                }
            }), typeof(XrmPackageSettings));
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
                    ApplicationController.UserMessage("Code Copied To Clipboard!");
                }
                catch(Exception ex)
                {
                    r.ApplicationController.ThrowException(ex);
                }
            }), typeof(SettingsFolderMoving));
        }
    }
}