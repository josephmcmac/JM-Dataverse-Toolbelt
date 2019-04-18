﻿using JosephM.Application.Modules;
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
            var propertiesForAutocomplete = new[]
            {
                nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress),
                nameof(SavedXrmRecordConfiguration.Domain),
                nameof(SavedXrmRecordConfiguration.Username),
            };
            foreach (var prop in propertiesForAutocomplete)
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
                        .ToArray();
                }), typeof(SavedXrmRecordConfiguration), prop);
            }
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