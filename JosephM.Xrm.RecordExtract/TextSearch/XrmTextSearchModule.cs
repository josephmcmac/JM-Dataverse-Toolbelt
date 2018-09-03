using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmModule.XrmConnection;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(XrmConnectionModule))]
    [DependantModule(typeof(XrmRecordExtractModule))]
    [MyDescription("Search Records In Dynamics For A Specific Piece Of Text")]
    public class XrmTextSearchModule :
        TextSearchModuleBase
            <XrmTextSearchDialog, XrmTextSearchService>
    {
        public override string MainOperationName
        {
            get { return "Text Search"; }
        }

        public override string MenuGroup => "Reports";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddTextSearchButtonToSavedConnectionsGrid();
        }

        private void AddTextSearchButtonToSavedConnectionsGrid()
        {
            var customGridFunction = new CustomGridFunction("TEXTSEARCH", "Text Search Selected", (g) =>
            {
                if (g.SelectedRows.Count() != 1)
                {
                    g.ApplicationController.UserMessage("Please Select One Row To Search The Connection");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance, formService: new XrmFormService());
                        var xrmTextSearchService = new XrmTextSearchService(xrmRecordService, new DocumentWriter.DocumentWriter());
                        var dialog = new XrmTextSearchDialog(xrmTextSearchService, new DialogController(ApplicationController), xrmRecordService);
                        dialog.SetTabLabel(instance.Name + " " + dialog.TabLabel);
                        g.ApplicationController.NavigateTo(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}