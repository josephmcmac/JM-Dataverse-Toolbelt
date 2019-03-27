using JosephM.Application.Desktop.Module.Crud.BulkDelete;
using JosephM.Application.Desktop.Module.Crud.BulkUpdate;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.XrmModule.Crud;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using JosephM.Core.AppConfig;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Application.Application;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Core.Utility;
using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class XrmCrudModuleTest : XrmModuleTest
    {
        /// <summary>
        /// scripts through running a query with joins and conditions
        /// </summary>
        [TestMethod]
        public void XrmCrudQueryEditColumnsTestScript()
        {
            var count = XrmRecordService.GetFirstX(Entities.account, 3, null, null).Count();
            while (count < 3)
            {
                CreateAccount();
                count++;
            }

            //Create test app and load query
            var app = CreateAndLoadTestApplication<XrmCrudModule>();
            var crudDialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();
            var queryViewModel = crudDialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);

            //select account type then edit columns button
            queryViewModel.SelectedRecordType = queryViewModel.RecordTypeItemsSource.First(r => r.Key == Entities.account);

            var initialColumnCount = queryViewModel.DynamicGridViewModel.FieldMetadata.Count();

            queryViewModel.DynamicGridViewModel.GetButton("EDITCOLUMNS").Invoke();

            Assert.AreEqual(1, queryViewModel.ChildForms.Count);
            var editColumnsDialog = queryViewModel.ChildForms.First() as ColumnEditDialogViewModel;
            Assert.IsNotNull(editColumnsDialog);

            //okay lets do some moving of columns
            var currentCount = editColumnsDialog.CurrentColumns.Count;
            var selectableCount = editColumnsDialog.SelectableColumns.Count;

            Assert.IsTrue(currentCount > 0);
            Assert.IsTrue(selectableCount > 0);

            //add column button
            editColumnsDialog.SelectableColumns.Last().AddCommand.Execute();
            Assert.AreEqual(++currentCount, editColumnsDialog.CurrentColumns.Count);
            Assert.AreEqual(--selectableCount, editColumnsDialog.SelectableColumns.Count);
            //drag column
            editColumnsDialog.AddCurrentItem(editColumnsDialog.SelectableColumns.Last(), target: editColumnsDialog.SelectableColumns.First(), isAfter: false);
            Assert.AreEqual(++currentCount, editColumnsDialog.CurrentColumns.Count);
            Assert.AreEqual(--selectableCount, editColumnsDialog.SelectableColumns.Count);

            //remove column
            editColumnsDialog.CurrentColumns.Last().RemoveCommand.Execute();
            Assert.AreEqual(--currentCount, editColumnsDialog.CurrentColumns.Count);
            Assert.AreEqual(++selectableCount, editColumnsDialog.SelectableColumns.Count);

            //reorder colummn
            editColumnsDialog.AddCurrentItem(editColumnsDialog.CurrentColumns.First(), target: editColumnsDialog.CurrentColumns.Last(), isAfter: true);
            Assert.AreEqual(currentCount, editColumnsDialog.CurrentColumns.Count);
            Assert.AreEqual(selectableCount, editColumnsDialog.SelectableColumns.Count);

            editColumnsDialog.AddCurrentItem(editColumnsDialog.CurrentColumns.First(), target: editColumnsDialog.CurrentColumns.First(), isAfter: false);
            Assert.AreEqual(currentCount, editColumnsDialog.CurrentColumns.Count);
            Assert.AreEqual(selectableCount, editColumnsDialog.SelectableColumns.Count);

            Assert.AreNotEqual(initialColumnCount, currentCount);

            editColumnsDialog.ApplyChanges();

            Assert.AreEqual(0, queryViewModel.ChildForms.Count);

            Assert.AreEqual(currentCount, queryViewModel.DynamicGridViewModel.FieldMetadata.Count());
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.GridRecords.Any());
            Assert.AreEqual(currentCount, queryViewModel.DynamicGridViewModel.GridRecords.First().FieldViewModels.Count());
        }

        /// <summary>
        /// scripts through running a query with joins and conditions
        /// </summary>
        [TestMethod]
        public void XrmCrudQueryTestScript()
        {
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

            //change to query entry
            queryViewModel.QueryTypeButton.Invoke();

            //okay well lets do a query which has

            //grouped conditions
            var lastCondition = queryViewModel.FilterConditions.Conditions.Last();
            var fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First();
            var conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.NotNull).ToString());
            lastCondition.GetBooleanFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.IsSelected)).Value = true;

            lastCondition = queryViewModel.FilterConditions.Conditions.Last();
            fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First();
            conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.NotNull).ToString());
            lastCondition.GetBooleanFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.IsSelected)).Value = true;

            queryViewModel.GroupSelectedConditionsOr.Invoke();
            Assert.AreEqual(1, queryViewModel.FilterConditions.Conditions.Count());
            Assert.AreEqual(3, queryViewModel.FilterConditions.FilterConditions.First().Conditions.Count());

            queryViewModel.UngroupSelectedConditions.Invoke();
            Assert.AreEqual(3, queryViewModel.FilterConditions.Conditions.Count());
            Assert.IsFalse( queryViewModel.FilterConditions.FilterConditions.Any());

            //a join
            var lastjoin = queryViewModel.Joins.Joins.Last();
            lastjoin.SelectedItem = lastjoin.LinkSelections.First();
            Assert.AreEqual(2, queryViewModel.Joins.Joins.Count());

            // conditions in the join
            lastCondition = lastjoin.FilterConditions.Conditions.Last();
            fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First();
            conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.NotNull).ToString());
            lastCondition.GetBooleanFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.IsSelected)).Value = true;

            lastCondition = lastjoin.FilterConditions.Conditions.Last();
            fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First();
            conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.NotNull).ToString());
            lastCondition.GetBooleanFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.IsSelected)).Value = true;

            queryViewModel.GroupSelectedConditionsOr.Invoke();
            Assert.AreEqual(1, lastjoin.FilterConditions.Conditions.Count());
            Assert.AreEqual(3, lastjoin.FilterConditions.FilterConditions.First().Conditions.Count());

            queryViewModel.UngroupSelectedConditions.Invoke();
            Assert.AreEqual(3, lastjoin.FilterConditions.Conditions.Count());
            Assert.IsFalse(lastjoin.FilterConditions.FilterConditions.Any());

            //a child join

            var lastJoinLastjoin = lastjoin.Joins.Joins.Last();
            lastJoinLastjoin.SelectedItem = lastJoinLastjoin.LinkSelections.First();
            Assert.AreEqual(2, lastjoin.Joins.Joins.Count());

            // conditions in the child join
            lastCondition = lastJoinLastjoin.FilterConditions.Conditions.Last();
            fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First();
            conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.NotNull).ToString());
            lastCondition.GetBooleanFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.IsSelected)).Value = true;

            lastCondition = lastJoinLastjoin.FilterConditions.Conditions.Last();
            fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First();
            conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.NotNull).ToString());
            lastCondition.GetBooleanFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.IsSelected)).Value = true;

            queryViewModel.GroupSelectedConditionsOr.Invoke();
            Assert.AreEqual(1, lastJoinLastjoin.FilterConditions.Conditions.Count());
            Assert.AreEqual(3, lastJoinLastjoin.FilterConditions.FilterConditions.First().Conditions.Count());

            queryViewModel.UngroupSelectedConditions.Invoke();
            Assert.AreEqual(3, lastJoinLastjoin.FilterConditions.Conditions.Count());
            Assert.IsFalse(lastJoinLastjoin.FilterConditions.FilterConditions.Any());

            var query = queryViewModel.GenerateQuery();

            queryViewModel.QuickFind();
        }

        /// <summary>
        /// runs through several xrm crud scenarios - quickfind, edit, bulk update, bulk delete, create
        /// </summary>
        [TestMethod]
        public void XrmCrudModuleTestScript()
        {
            // this script runs through several scenarios in the crud module
            // opening and running quickfind
            // opening a record updating a field and saving
            // selecting 2 records and doing a bulk update on them
            // doing a bulk update on all records
            // selecting 2 records and doing a bulk delete on them
            // doing a bulk delete on all records
            // create a new record
            // create a new record with an error thrown
            DeleteAll(Entities.account);
            var count = XrmRecordService.GetFirstX(Entities.account, 3, null, null).Count();
            while (count < 3)
            {
                CreateAccount();
                count++;
            }

            //Create test app and load query
            var app = CreateAndLoadTestApplication<XrmCrudModule>();
            var settingsFolder = app.Controller.SettingsPath;
            if (settingsFolder != null)
                FileUtility.DeleteFiles(settingsFolder);

            //okay adding this here because I added a redirect to connection entry if none is entered
            var xrmRecordService = app.Controller.ResolveType<XrmRecordService>();
            //okay this is the service which will get resolve by the dialog - so lets clear out its connection details
            //then the dialog should redirect to entry
            var originalConnection = xrmRecordService.XrmRecordConfiguration;
            xrmRecordService.XrmRecordConfiguration = new XrmRecordConfiguration();

            var dialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();

            //okay we should have been dir4ected to a connection entry

            var connectionEntryViewModel = dialog.Controller.UiItems[0] as ObjectEntryViewModel;
            var newConnection = connectionEntryViewModel.GetObject() as SavedXrmRecordConfiguration;
            newConnection.AuthenticationProviderType = originalConnection.AuthenticationProviderType;
            newConnection.DiscoveryServiceAddress = originalConnection.DiscoveryServiceAddress;
            newConnection.OrganizationUniqueName = originalConnection.OrganizationUniqueName;
            newConnection.Domain = originalConnection.Domain;
            newConnection.Username = originalConnection.Username;
            newConnection.Password = originalConnection.Password;
            newConnection.Name = "RedirectScriptEntered";
            connectionEntryViewModel.SaveButtonViewModel.Invoke();

            //cool if has worked then now we will be at the query view model with the connection
            var queryViewModel = dialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);
            //lets just verify the connection was saved as well
            var savedConnections = app.Controller.ResolveType<ISavedXrmConnections>();
            Assert.IsTrue(savedConnections.Connections.Any(c => c.Name == "RedirectScriptEntered"));
            var appXrmRecordService = app.Controller.ResolveType<XrmRecordService>();
            Assert.IsTrue(appXrmRecordService.XrmRecordConfiguration.ToString() == "RedirectScriptEntered");
            var appXrmRecordConnection = app.Controller.ResolveType<IXrmRecordConfiguration>();
            Assert.IsTrue(appXrmRecordConnection.ToString() == "RedirectScriptEntered");
            var savedSetingsManager = app.Controller.ResolveType<ISettingsManager>();
            var savedXrmRecordService = savedSetingsManager.Resolve<SavedXrmConnections.SavedXrmConnections>();
            Assert.IsTrue(appXrmRecordService.XrmRecordConfiguration.ToString() == "RedirectScriptEntered");
            var savedXrmRecordConnection = savedSetingsManager.Resolve<XrmRecordConfiguration>();

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

            //now do bulk copy field value

            //select 2 record for bulk copy field value
            queryViewModel.GridRecords.First().IsSelected = true;
            queryViewModel.GridRecords.ElementAt(1).IsSelected = true;
            id = queryViewModel.GridRecords.First().GetRecord().Id;
            id2 = queryViewModel.GridRecords.ElementAt(1).GetRecord().Id;
            record = XrmRecordService.Get(Entities.account, id);
            record.SetField(Fields.account_.description, "WTF", XrmRecordService);
            XrmRecordService.Update(record, new[] { Fields.account_.description });
            record = XrmRecordService.Get(Entities.account, id2);
            Assert.IsNull(record.GetStringField(Fields.account_.description));
            //this triggered by ui event
            queryViewModel.DynamicGridViewModel.OnSelectionsChanged();
            //trigger and enter bulk update
            queryViewModel.DynamicGridViewModel.GetButton("BULKCOPYFIELDVALUESELECTED").Invoke();
            //todo verify description empty
            DoBulkCopyFieldValue(dialog, Fields.account_.name, Fields.account_.description);
            //verify records updated
            //this dude doesnt get copied because already populated
            record = XrmRecordService.Get(Entities.account, id);
            Assert.AreEqual("WTF", record.GetStringField(Fields.account_.description));
            record = XrmRecordService.Get(Entities.account, id2);
            Assert.AreEqual(record.GetStringField(Fields.account_.name), record.GetStringField(Fields.account_.description));
            Assert.IsFalse(queryViewModel.ChildForms.Any());

            //now do bulk copy field value all
            queryViewModel.DynamicGridViewModel.GetButton("BULKCOPYFIELDVALUEALL").Invoke();
            DoBulkCopyFieldValue(dialog, Fields.account_.name, Fields.account_.description);
            allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);

            //this dude doesnt get copied because already populated
            Assert.IsTrue(allAccounts.Any(a => a.GetStringField(Fields.account_.description) == "WTF"));
            foreach (var account in allAccounts)
            {
                if (account.Id == id)
                    Assert.AreEqual("WTF", account.GetStringField(Fields.account_.description));
                else
                    Assert.AreEqual(account.GetStringField(Fields.account_.name), account.GetStringField(Fields.account_.description));
            }
            //okay lets just verify options
            //do bulk copy field with overwrite
            queryViewModel.DynamicGridViewModel.GetButton("BULKCOPYFIELDVALUEALL").Invoke();
            DoBulkCopyFieldValue(dialog, Fields.account_.name, Fields.account_.description, overwriteIfPopulated: true);

            allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);
            foreach (var account in allAccounts)
            {
                //all got updated
                Assert.AreEqual(account.GetStringField(Fields.account_.name), account.GetStringField(Fields.account_.description));

                account.SetField(Fields.account_.description, "WTF", XrmRecordService);
                XrmRecordService.Update(account, new[] { Fields.account_.description });
                //set the name null to check doesnt copy next call
                if (account.Id == id)
                {
                    account.SetField(Fields.account_.name, null, XrmRecordService);
                    XrmRecordService.Update(account, new[] { Fields.account_.name });
                }
            }
            //do another and verify the desciption wasnt cleared
            queryViewModel.DynamicGridViewModel.GetButton("BULKCOPYFIELDVALUEALL").Invoke();
            DoBulkCopyFieldValue(dialog, Fields.account_.name, Fields.account_.description, overwriteIfPopulated: true);
            allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);
            Assert.IsTrue(allAccounts.Any(a => a.GetStringField(Fields.account_.name) == null));
            foreach (var account in allAccounts)
            {
                Assert.IsNotNull(account.GetStringField(Fields.account_.description));
            }
            //do one more with include nulls and verify we now have an empty description
            queryViewModel.DynamicGridViewModel.GetButton("BULKCOPYFIELDVALUEALL").Invoke();
            DoBulkCopyFieldValue(dialog, Fields.account_.name, Fields.account_.description, copyIfNull: true, overwriteIfPopulated: true);
            allAccounts = XrmRecordService.RetrieveAll(Entities.account, null);
            Assert.IsTrue(allAccounts.Any(a => a.GetStringField(Fields.account_.description) == null));


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
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }

        private void DoBulkCopyFieldValue(XrmCrudDialog crudDialog, string sourceField, string targetField, bool copyIfNull = false, bool overwriteIfPopulated = false)
        {
            var bulkUpdateDialog = crudDialog.ChildForms.First() as BulkCopyFieldValueDialog;
            Assert.IsNotNull(bulkUpdateDialog);
            bulkUpdateDialog.LoadDialog();
            var bulkUpdateEntry = bulkUpdateDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(bulkUpdateEntry);
            bulkUpdateEntry.LoadFormSections();
            var fieldField = bulkUpdateEntry.GetRecordFieldFieldViewModel(nameof(BulkCopyFieldValueRequest.SourceField));
            fieldField.Value = fieldField.ItemsSource.First(kv => kv.Key == sourceField);
            var valueField = bulkUpdateEntry.GetRecordFieldFieldViewModel(nameof(BulkCopyFieldValueRequest.TargetField));
            valueField.Value = valueField.ItemsSource.First(kv => kv.Key == targetField);
            bulkUpdateEntry.GetBooleanFieldFieldViewModel(nameof(BulkCopyFieldValueRequest.CopyIfNull)).Value = copyIfNull;
            bulkUpdateEntry.GetBooleanFieldFieldViewModel(nameof(BulkCopyFieldValueRequest.OverwriteIfPopulated)).Value = overwriteIfPopulated;
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkUpdateDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkCopyFieldValueResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
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
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }
    }
}
