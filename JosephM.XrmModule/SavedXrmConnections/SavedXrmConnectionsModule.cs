using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.XrmModule.XrmConnection;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using System.Diagnostics;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Record.Xrm.Mappers;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [MyDescription("Multiple Saved Connections To CRM Instances")]
    [DependantModule(typeof(XrmConnectionModule))]
    public class SavedXrmConnectionsModule : SettingsModule<SavedXrmConnectionsDialog, ISavedXrmConnections, SavedXrmConnections>
    {
        public override string MainOperationName => "Saved Connections";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }

        public override void RegisterTypes()
        {
            var configManager = Resolve<ISettingsManager>();
            configManager.ProcessNamespaceChange(GetType().Namespace, "JosephM.Prism.XrmModule.SavedXrmConnections");
            base.RegisterTypes();
            AddWebBrowseGridFunction();
            AddConnectionFieldsAutocomplete();
        }

        private void AddConnectionFieldsAutocomplete()
        {
            //existing values for these
            var propertiesForAutocompleteExistingValues = new[]
            {
                nameof(SavedXrmRecordConfiguration.Domain),
                nameof(SavedXrmRecordConfiguration.Username),
            };
            foreach (var prop in propertiesForAutocompleteExistingValues)
            {
                this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
                {
                    var parentForm = recordForm.ParentForm;
                    if (parentForm == null)
                        return null;
                    var objectRecord = parentForm.GetRecord() as ObjectRecord;
                    if (objectRecord == null)
                        return null;
                    var instance = objectRecord.Instance as ISavedXrmConnections;
                    if (instance == null)
                        return null;
                    return instance
                        .Connections
                        .Select(pt => (string)pt.GetPropertyValue(prop))
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Distinct()
                        .Select(s => new AutocompleteOption(s))
                        .ToArray();
                }, isValidForFormFunction: (f) => f.ParentForm != null, displayInGrid: false), typeof(SavedXrmRecordConfiguration), prop);
            }

            //existing values + the standard online regional endpoints
            var onlineDiscoveryServices = new[]
            {
                new AutocompleteOption("North America", "https://disco.crm.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("North America 2", "https://disco.crm9.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("EMEA", "https://disco.crm4.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Asia Pacific Area", "https://disco.crm5.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Oceania", "https://disco.crm6.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Japan", "https://disco.crm7.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("South America", "https://disco.crm2.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("India", "https://disco.crm8.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Canada", "https://disco.crm3.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("United Kingdom", "https://disco.crm11.dynamics.com/XRMServices/2011/Discovery.svc"),
            };
            this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            {
                var parentForm = recordForm.ParentForm;
                if (parentForm == null)
                    return onlineDiscoveryServices;
                var objectRecord = parentForm.GetRecord() as ObjectRecord;
                if (objectRecord == null)
                    return onlineDiscoveryServices;
                var instance = objectRecord.Instance as ISavedXrmConnections;
                var otherSavedConnections = instance == null
                    ? new string[0]
                    : instance
                    .Connections
                    .Select(pt => (string)pt.GetPropertyValue(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)))
                    .Where(g => !string.IsNullOrWhiteSpace(g) && !onlineDiscoveryServices.Any(sv => sv.Value.ToLower() == g.ToLower()));
                return onlineDiscoveryServices
                    .Union(otherSavedConnections.Select(s => new AutocompleteOption(" ", s)));
            }, displayInGrid: false, displayNames: true), typeof(SavedXrmRecordConfiguration), nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress));

            //get the organisations based on details entered
            this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            {
                var objectRecord = recordForm.GetRecord() as ObjectRecord;
                if (objectRecord == null)
                    return null;
                var thisConnectionEntered = objectRecord.Instance as XrmRecordConfiguration;
                if (thisConnectionEntered == null)
                    return null;
                var xrmRecordConfiguration = new XrmRecordConfigurationInterfaceMapper().Map(thisConnectionEntered);
                var xrmConfiguration = new XrmConfigurationMapper().Map(xrmRecordConfiguration);
                var xrmConnection = new Xrm.XrmConnection(xrmConfiguration);
                return xrmConnection
                    .GetActiveOrganisations()
                    .Select(org => new AutocompleteOption(org.FriendlyName, org.UniqueName));
            }, displayInGrid: false, autosearch: false, displayNames: true), typeof(SavedXrmRecordConfiguration), nameof(SavedXrmRecordConfiguration.OrganizationUniqueName));
        }

        private void AddWebBrowseGridFunction()
        {
            var customGridFunction = new CustomGridFunction("WEB", "Open In Web", (g) =>
            {
                if (g.SelectedRows.Count() != 1)
                {
                    g.ApplicationController.UserMessage("Please Select One Row To Browse The Connection");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance);
                        Process.Start(xrmRecordService.WebUrl);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}