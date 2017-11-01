#region

using System;
using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using System.Diagnostics;
using System.Linq;

#endregion

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class SavedXrmConnectionsModule : SettingsModule<SavedXrmConnectionsDialog, ISavedXrmConnections, SavedXrmConnections>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Saved XRM Connections", "SavedXrmConnections");
        }

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddWebBrowseGridFunction();
        }

        private void AddWebBrowseGridFunction()
        {
            var customGridFunction = new CustomGridFunction("WEB", "Open In Web", (g) =>
            {
                var selectedRow = g.SelectedRows.First();
                var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                if (instance != null)
                {
                    var xrmRecordService = new XrmRecordService(instance);
                    Process.Start(xrmRecordService.WebUrl);
                }
            }, (g) => g.GridRecords != null && g.SelectedRows.Count() == 1);
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}