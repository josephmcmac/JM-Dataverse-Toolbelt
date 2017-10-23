using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Prism.Infrastructure.Module.Crud;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Prism.XrmModule.Crud
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmCrudModule : CrudModule<XrmCrudDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            var customGridFunction = new CustomGridFunction("CRUD", "Browse Selected", (g) =>
            {
                var selectedRow = g.SelectedRows.First();
                var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                if(instance != null)
                {
                    var xrmRecordService = new XrmRecordService(instance, formService: new XrmFormService());
                    var dialog = new CrudDialog(new DialogController(ApplicationController), xrmRecordService);
                    dialog.SetTabLabel("Browse " + instance.Name);
                    g.LoadDialog(dialog);
                }

            }, (g) => g.GridRecords != null && g.SelectedRows.Count() == 1);
            var functions = new CustomGridFunctions();
            functions.AddFunction(customGridFunction);
            //todo this should add the function not just inject it
            ApplicationController.RegisterInstance(typeof(CustomGridFunctions), typeof(SavedXrmRecordConfiguration).AssemblyQualifiedName, functions);
        }

        protected override string MainOperationName
        {
            get { return "Browse/Update Data"; }
        }
    }
}
