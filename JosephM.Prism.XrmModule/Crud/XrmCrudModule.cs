using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.XrmModule.XrmConnection;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;
using JosephM.Application.Desktop.Module.Crud;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.XrmModule.Crud
{
    [MyDescription("Query And Create Or Update Records In The CRM Instance")]
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmCrudModule : CrudModule<XrmCrudDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddBrowseButtonToSavedConnectionsGrid();
        }

        public override string MainOperationName
        {
            get { return "Crud/Query Data"; }
        }

        private void AddBrowseButtonToSavedConnectionsGrid()
        {
            var customGridFunction = new CustomGridFunction("CRUD", "Crud/Query Selected", (g) =>
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
                        var xrmRecordService = new XrmRecordService(instance, formService: new XrmFormService());
                        var dialog = new XrmCrudDialog(xrmRecordService, new DialogController(ApplicationController));
                        dialog.SetTabLabel(instance.ToString() + " " + dialog.TabLabel);
                        g.ApplicationController.NavigateTo(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}
