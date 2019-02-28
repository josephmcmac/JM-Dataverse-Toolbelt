using System;
using System.Linq;
using JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber;
using JosephM.Application.Desktop.Module.Dialog;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Autonumber
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    [MyDescription("Configure Autonumber Fields Using The Native Dynamics Autonumbering")]
    public class AutonumberModule : DialogModule<AutonumberDialog>
    {
        public override string MenuGroup => "Customisations";

        public override string MainOperationName => "Autonumbers";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddNewAutonumberButtonToAutonumberFieldsGrid();
            AddReconfigureAutonumberButtonToAutonumberFieldsGrid();
            AddLoadAuotnumberFieldsTrigger();
            AddOpenMicrosoftDocumentationButton();
        }

        private void AddOpenMicrosoftDocumentationButton()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENSDOCUMENTATION", "Microsoft Article", (g) =>
            {
                ApplicationController.StartProcess("https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/create-auto-number-attributes");
            }), typeof(AutonumberNavigator));
        }

        private void AddNewAutonumberButtonToAutonumberFieldsGrid()
        {
            this.AddCustomGridFunction(new CustomGridFunction("ADDNEWAUTONUMBER", "Configure New", (g) =>
            {
                var parentForm = g.ParentForm;
                //okay need to get the selected record type
                //and load the new autonumber dialog
                ApplicationController.DoOnAsyncThread(() =>
                {
                    if (parentForm == null)
                        throw new NullReferenceException($"Error {nameof(DynamicGridViewModel.ParentForm)} is Null On The {nameof(DynamicGridViewModel)}");

                    try
                    {
                        parentForm.LoadingViewModel.IsLoading = true;

                        var recordType = parentForm.GetRecordTypeFieldViewModel(nameof(AutonumberNavigator.RecordType));
                        if (recordType.Value == null)
                            throw new NullReferenceException($"Error {nameof(AutonumberNavigator.RecordType)} is Null");

                        var req = new ConfigureAutonumberRequest()
                        {
                            RecordType = recordType.Value
                        };
                        var dialogcontroller = (IDialogController)ApplicationController.ResolveType(typeof(IDialogController));
                        var dialog = new ConfigureAutonumberDialog(parentForm.RecordService.LookupService as XrmRecordService, dialogcontroller, req, () => { parentForm.ClearChildForms(); RefreshFieldGrid(parentForm); });
                        parentForm.LoadChildForm(dialog);
                    }
                    finally
                    {
                        parentForm.LoadingViewModel.IsLoading = false;
                    }
                });
            }, (g) => { return true; }), typeof(AutonumberNavigator.AutonumberField));
        }

        private void AddReconfigureAutonumberButtonToAutonumberFieldsGrid()
        {
            this.AddCustomGridFunction(new CustomGridFunction("RECONFIGUREAUTONUMBER", "Reconfigure Selected", (g) =>
            {
                //okay need to get the selected record type
                //and load the new autonumber dialog

                var selectedRows = g.SelectedRows;
                if (selectedRows.Count() != 1)
                {
                    ApplicationController.UserMessage("Please Select 1 Row To Reconfigure An Autonumber");
                }
                else
                {
                    var selectedFieldName = selectedRows.First().GetStringFieldFieldViewModel(nameof(AutonumberNavigator.AutonumberField.SchemaName)).Value;
                    var selectedFieldFormat = selectedRows.First().GetStringFieldFieldViewModel(nameof(AutonumberNavigator.AutonumberField.Format)).Value;

                    var parentForm = g.ParentForm;
                    if (parentForm == null)
                        throw new NullReferenceException($"Error {nameof(DynamicGridViewModel.ParentForm)} is Null On The {nameof(DynamicGridViewModel)}");

                    var recordType = parentForm.GetRecordTypeFieldViewModel(nameof(AutonumberNavigator.RecordType));
                    if (recordType.Value == null)
                        throw new NullReferenceException($"Error {nameof(AutonumberNavigator.RecordType)} is Null");

                    var req = new ConfigureAutonumberRequest()
                    {
                        RecordType = recordType.Value,
                        Field = new RecordField(selectedFieldName, selectedFieldName),
                        AutonumberFormat = selectedFieldFormat
                    };
                    var dialogcontroller = (IDialogController)ApplicationController.ResolveType(typeof(IDialogController));
                    Action closeChildDialog = () => { parentForm.ClearChildForms(); RefreshFieldGrid(parentForm); };
                    var dialog = new ConfigureAutonumberDialog(parentForm.RecordService.LookupService as XrmRecordService, dialogcontroller, req, closeChildDialog);
                    dialog.OverideCompletionScreenMethod = closeChildDialog;
                    parentForm.LoadChildForm(dialog);
                }
            }, (g) => { return true; }), typeof(AutonumberNavigator.AutonumberField));
        }

        private void AddLoadAuotnumberFieldsTrigger()
        {
            this.AddOnChangeFunction(new OnChangeFunction((revm, fieldName) =>
            {
                switch(fieldName)
                {
                    case nameof(AutonumberNavigator.RecordType):
                        {
                            RefreshFieldGrid(revm);
                            break;
                        }
                }
            }), typeof(AutonumberNavigator));
        }

        private void RefreshFieldGrid(RecordEntryViewModelBase revm)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                revm.LoadingViewModel.IsLoading = true;
                try
                {

                    var recordType = revm.GetRecordTypeFieldViewModel(nameof(AutonumberNavigator.RecordType));
                    var recordTypeName = recordType.Value?.Key;
                    var autonumberFieldsField = revm.GetEnumerableFieldViewModel(nameof(AutonumberNavigator.AutonumberFields));
                    foreach (var gridRow in autonumberFieldsField.GridRecords)
                    {
                        ApplicationController.DoOnMainThread(() => autonumberFieldsField.GridRecords.Remove(gridRow));
                    }
                    if (!string.IsNullOrWhiteSpace(recordTypeName))
                    {
                        var fields = revm.RecordService.LookupService
                            .GetFieldMetadata(recordTypeName)
                            .Where(f => !string.IsNullOrWhiteSpace(f.AutonumberFormat))
                            .ToArray();
                        foreach (var field in fields)
                        {
                            var newRecord = revm.RecordService.NewRecord(typeof(AutonumberNavigator.AutonumberField).AssemblyQualifiedName);
                            newRecord.SetField(nameof(AutonumberNavigator.AutonumberField.SchemaName), field.SchemaName, revm.RecordService);
                            newRecord.SetField(nameof(AutonumberNavigator.AutonumberField.Format), field.AutonumberFormat, revm.RecordService);
                            autonumberFieldsField.InsertRecord(newRecord, 0);
                        }
                    }
                }
                finally
                {
                    revm.LoadingViewModel.IsLoading = false;
                }
            });
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }
    }
}