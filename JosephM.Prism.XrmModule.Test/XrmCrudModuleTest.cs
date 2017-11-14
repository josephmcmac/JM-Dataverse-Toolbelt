using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module.Crud.BulkDelete;
using JosephM.Prism.Infrastructure.Module.Crud.BulkUpdate;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JosephM.Prism.XrmModule.Test
{
    [TestClass]
    public class XrmCrudModuleTest : XrmModuleTest
    {
        [TestMethod]
        public void XrmCrudModuleTestScript()
        {
            //todo consider anything else necessary e.g. change other field types

            // this script runs through several scenarios in the crud module
            // opening and running quickfind
            // opening a record updating a field and saving
            // selecting 2 records and doing a bulk update on them
            // doing a bulk update on all records
            // selecting 2 records and doing a bulk delete on them
            // doing a bulk delete on all records
            // create a new record
            // create a new record with an error thrown
            var count = XrmRecordService.GetFirstX(Entities.account, 3, null, null).Count();
            while (count < 3)
            {
                CreateAccount();
                count++;
            }

            //Create test app and load query
            var app = CreateAndLoadTestApplication<XrmCrudModule>();
            var dialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();
            var queryViewModel = dialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);

            //select account type and run query
            queryViewModel.SelectedRecordType = queryViewModel.RecordTypeItemsSource.First(r => r.Key == Entities.account);
            queryViewModel.DynamicGridViewModel.GetButton("QUERY").Invoke();
            Assert.IsTrue(queryViewModel.GridRecords.Any());

            //select first record and open it
            queryViewModel.DynamicGridViewModel.EditRow(queryViewModel.DynamicGridViewModel.GridRecords.First());

            var editAccountForm = queryViewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(editAccountForm);
            editAccountForm.LoadFormSections();
            var id = editAccountForm.GetRecord().Id;

            //set its name
            var newName = "Updated " + DateTime.Now.ToFileTime();
            editAccountForm.GetStringFieldFieldViewModel(Fields.account_.name).Value = newName;
            Assert.IsTrue(editAccountForm.ChangedPersistentFields.Count == 1);
            Assert.IsTrue(editAccountForm.ChangedPersistentFields.First() == Fields.account_.name);
            //save
            editAccountForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(queryViewModel.ChildForms.Any());

            //verify record updated
            var record = XrmRecordService.Get(Entities.account, id);
            Assert.AreEqual(newName, record.GetStringField(Fields.account_.name));

            //now do bulk updates selected

            //select 2 record for bulk update
            queryViewModel.GridRecords.First().IsSelected = true;
            queryViewModel.GridRecords.ElementAt(1).IsSelected = true;
            id = queryViewModel.GridRecords.First().GetRecord().Id;
            var id2 = queryViewModel.GridRecords.ElementAt(1).GetRecord().Id;
            //this triggered by ui event
            queryViewModel.DynamicGridViewModel.OnSelectionsChanged();
            //trigger and enter bulk update
            queryViewModel.DynamicGridViewModel.GetButton("BULKUPDATESELECTED").Invoke();
            var newAddressLine1 = "Bulk Selected " + DateTime.Now.ToFileTime();
            DoBulkUpdate(dialog, newAddressLine1, Fields.account_.address1_line1);
            //verify records updated
            record = XrmRecordService.Get(Entities.account, id);
            Assert.AreEqual(newAddressLine1, record.GetStringField(Fields.account_.address1_line1));
            record = XrmRecordService.Get(Entities.account, id2);
            Assert.AreEqual(newAddressLine1, record.GetStringField(Fields.account_.address1_line1));
            Assert.IsFalse(queryViewModel.ChildForms.Any());

            //now do bulk updates all
            queryViewModel.DynamicGridViewModel.GetButton("BULKUPDATEALL").Invoke();
            newAddressLine1 = "Bulk Update All " + DateTime.Now.ToFileTime();
            DoBulkUpdate(dialog, newAddressLine1, Fields.account_.address1_line1);

            var allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);
            foreach(var account in allAccounts)
                Assert.AreEqual(newAddressLine1, account.GetStringField(Fields.account_.address1_line1));

            //select 2 record for bulk delete
            queryViewModel.GridRecords.First().IsSelected = true;
            queryViewModel.GridRecords.ElementAt(1).IsSelected = true;
            id = queryViewModel.GridRecords.First().GetRecord().Id;
            id2 = queryViewModel.GridRecords.ElementAt(1).GetRecord().Id;
            //this triggered by ui event
            queryViewModel.DynamicGridViewModel.OnSelectionsChanged();
            //trigger and enter bulk update
            queryViewModel.DynamicGridViewModel.GetButton("BULKDELETESELECTED").Invoke();
            DoBulkDelete(dialog);
            //verify records deleted
            Assert.IsFalse(queryViewModel.ChildForms.Any());
            Assert.IsNull(XrmRecordService.Get(Entities.account, id));
            Assert.IsNull(XrmRecordService.Get(Entities.account, id2));

            //now do bulk delete all
            queryViewModel.DynamicGridViewModel.GetButton("BULKDELETEALL").Invoke();
            DoBulkDelete(dialog);

            allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);
            Assert.AreEqual(0, allAccounts.Count());
            //verify records deleted

            //add a new row enytering it into the child form
            queryViewModel.DynamicGridViewModel.AddRow();
            EnterNewRecord(dialog);
            //verify created
            allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);
            Assert.AreEqual(1, allAccounts.Count());

            //okay well this just verifies an error is thrown to the user if the create fails (i set an explicit duplicate id)
            queryViewModel.DynamicGridViewModel.AddRow();
            var entryForm = dialog.QueryViewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(entryForm);
            entryForm.LoadFormSections();
            entryForm.GetFieldViewModel(Fields.account_.accountid).ValueObject = allAccounts.First().Id;

            try
            {
                entryForm.SaveButtonViewModel.Invoke();
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex is FakeUserMessageException);
            }
            //verify we are still on the child entry form
            entryForm = dialog.QueryViewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(entryForm);
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);
        }

        private static void EnterNewRecord(XrmCrudDialog crudDialog)
        {
            var entryForm = crudDialog.QueryViewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(entryForm);
            entryForm.LoadFormSections();
            entryForm.GetStringFieldFieldViewModel(Fields.account_.name).Value = "Test Script Record";
            entryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.QueryViewModel.ChildForms.Any());
        }

        private static void DoBulkUpdate(XrmCrudDialog crudDialog, string newValue, string field)
        {
            var bulkUpdateDialog = crudDialog.ChildForms.First() as BulkUpdateDialog;
            Assert.IsNotNull(bulkUpdateDialog);
            bulkUpdateDialog.LoadDialog();
            var bulkUpdateEntry = bulkUpdateDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(bulkUpdateEntry);
            bulkUpdateEntry.LoadFormSections();
            var fieldField = bulkUpdateEntry.GetRecordFieldFieldViewModel(nameof(BulkUpdateRequest.FieldToSet));
            fieldField.Value = fieldField.ItemsSource.First(kv => kv.Key == field);
            var valueField = bulkUpdateEntry.GetStringFieldFieldViewModel(nameof(BulkUpdateRequest.ValueToSet));
            valueField.Value = newValue;
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkUpdateDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkUpdateResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CloseButton.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }

        private static void DoBulkDelete(XrmCrudDialog crudDialog)
        {
            var bulkDeleteDialog = crudDialog.ChildForms.First() as BulkDeleteDialog;
            Assert.IsNotNull(bulkDeleteDialog);
            bulkDeleteDialog.LoadDialog();
            bulkDeleteDialog.LoadDialog();
            var bulkUpdateEntry = bulkDeleteDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(bulkDeleteDialog);
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkDeleteDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkDeleteResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CloseButton.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }
    }
}
