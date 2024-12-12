using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class XrmPackageSettingsModule : SettingsModule<XrmPackageSettingsDialog, XrmPackageSettings, XrmPackageSettings>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            InitialisePrefixFields();
            AddProjectAutocomplete();
        }

        private void AddProjectAutocomplete()
        {
            var props = new[]
            {
                new KeyValuePair<Type,string>(typeof(XrmPackageSettings.PluginPackageProject), nameof(XrmPackageSettings.PluginPackageProject.ProjectName)),
                new KeyValuePair<Type,string>(typeof(XrmPackageSettings.PluginProject), nameof(XrmPackageSettings.PluginProject.ProjectName)),
                new KeyValuePair<Type,string>(typeof(XrmPackageSettings.WebResourceProject), nameof(XrmPackageSettings.WebResourceProject.ProjectName)),
            };
            Func<RecordEntryViewModelBase, IEnumerable<AutocompleteOption>> getProjectsFunc = (recordForm) =>
            {
                var visualStudioService = recordForm.ApplicationController.ResolveType<IVisualStudioService>();
                if (visualStudioService == null)
                    return new AutocompleteOption[0];
                return visualStudioService.GetProjects().Select(p => new AutocompleteOption(p.Name)).ToArray();
            };
            foreach (var prop in props)
            {
                this.AddAutocompleteFunction(new AutocompleteFunction(getProjectsFunc, isValidForFormFunction: (form) => getProjectsFunc(form).Any()), prop.Key, prop.Value);
            }
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
    }
}