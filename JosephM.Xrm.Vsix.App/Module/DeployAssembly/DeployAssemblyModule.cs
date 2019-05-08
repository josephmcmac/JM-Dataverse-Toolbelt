using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Service;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.XrmConnection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [MenuItemVisibleForPluginProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployAssemblyModule : ServiceRequestModule<DeployAssemblyDialog, DeployAssemblyService, DeployAssemblyRequest, DeployAssemblyResponse, DeployAssemblyResponseItem>
    {
        public override string MenuGroup => "Plugins";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddWorkflowGroupAutocomplete();
        }

        private void AddWorkflowGroupAutocomplete()
        {
            Func<RecordEntryViewModelBase, IEnumerable<AutocompleteOption>> getExistingWorkflowGroups = (recordForm) =>
            {
                var parentForm = recordForm.ParentForm;
                if (parentForm == null)
                    return new AutocompleteOption[0];
                var objectRecord = parentForm.GetRecord() as ObjectRecord;
                if (objectRecord == null)
                    return new AutocompleteOption[0];
                var instance = objectRecord.Instance as DeployAssemblyRequest;
                if (instance == null)
                    return new AutocompleteOption[0];
                return instance
                    .PluginTypes
                    .Select(pt => pt.GroupName)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Distinct()
                    .Select(s => new AutocompleteOption(s))
                    .ToArray();
            };
            this.AddAutocompleteFunction(new AutocompleteFunction(getExistingWorkflowGroups, isValidForFormFunction: (form) => getExistingWorkflowGroups(form).Any(), cacheAsStaticList: true), typeof(PluginType), nameof(PluginType.GroupName));
        }
    }
}