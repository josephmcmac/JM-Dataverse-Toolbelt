using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Service;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.XrmConnection;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [MenuItemVisibleForPluginProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployAssemblyModule : OptionActionModule
    {
        public override string MainOperationName => "Deploy Assembly";

        public override string MenuGroup => "Plugins";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(DeployAssemblyDialog), null);
        }

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddWorkflowGroupAutocomplete();
        }

        private void AddWorkflowGroupAutocomplete()
        {
            this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            {
                var parentForm = recordForm.ParentForm;
                if (parentForm == null)
                    return null;
                var objectRecord = parentForm.GetRecord() as ObjectRecord;
                if(objectRecord == null)
                    return null;
                var instance = objectRecord.Instance as DeployAssemblyRequest;
                if (instance == null)
                    return null;
                return instance
                    .PluginTypes
                    .Select(pt => pt.GroupName)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Distinct()
                    .Select(s => new AutocompleteOption(s))
                    .ToArray();
            }), typeof(PluginType), nameof(PluginType.GroupName));
        }
    }
}