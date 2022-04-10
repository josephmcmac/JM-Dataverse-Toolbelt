using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using JosephM.Application.Desktop.Module.Crud.BulkDelete;
using JosephM.Application.Desktop.Module.Crud.BulkReplace;
using JosephM.Application.Desktop.Module.Crud.BulkUpdate;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.Crud.BulkWorkflow;
using JosephM.XrmModule.SavedXrmConnections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class XrmCrudModuleTest : XrmModuleTest
    {
        /// <summary>
        /// runs through several xrm crud scenarios - quickfind, edit, bulk update, bulk delete, create
        /// </summary>
        [TestMethod]
        public void XrmCrudModuleBulkOperationsTestScript()
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
            while (count < 10)
            {
                CreateAccount();
                count++;
            }

            //Create test app and load query
            var app = CreateAndLoadTestApplication<XrmCrudModule>();
            var crudDialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();
            var queryViewModel = crudDialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);
            queryViewModel.SelectedRecordType = queryViewModel.RecordTypeItemsSource.First(r => r.Key == Entities.account);

            queryViewModel.DynamicGridViewModel.GetButton("QUERY").Invoke();
            Assert.IsTrue(queryViewModel.GridRecords.Any());

            //okay this just updates all
            queryViewModel.DynamicGridViewModel.GetButton("BULKUPDATEALL").Invoke();
            DoBulkUpdate(crudDialog, "I Update", Fields.account_.address1_line1, doExecuteMultiples: true);

            var accounts = XrmRecordService.RetrieveAll(Entities.account, null);
            foreach(var account in accounts)
            {
                Assert.AreEqual("I Update", account.GetStringField(Fields.account_.address1_line1));
            }

            //bulk replace
            queryViewModel.DynamicGridViewModel.GetButton("BULKREPLACEALL").Invoke();
            DoBulkReplace(crudDialog, Fields.account_.address1_line1, "I Update", "I Updated", doExecuteMultiples: true);

            accounts = XrmRecordService.RetrieveAll(Entities.account, null);
            foreach (var account in accounts)
            {
                Assert.AreEqual("I Updated", account.GetStringField(Fields.account_.address1_line1));
            }

            //bulk delete
            queryViewModel.DynamicGridViewModel.GetButton("BULKDELETEALL").Invoke();
            DoBulkDelete(crudDialog, doExecuteMultiples: true);

            accounts = XrmRecordService.RetrieveAll(Entities.account, null);
            Assert.IsFalse(accounts.Any());
        }

        /// <summary>
        /// scripts through running a query whcih includes not in
        /// </summary>
        [TestMethod]
        public void XrmCrudQueryWithNotInTestScript()
        {
            DeleteAll(Entities.account);

            var conditionFieldIn = Fields.account_.address1_line1;
            var conditionFieldOut = Fields.account_.address1_line2;
            var conditionValueIn = "In";
            var conditionValueOut = "Out";

            var recordsInToCreate = 54;
            for (var i = 0; i < recordsInToCreate; i++)
            {
                CreateTestRecord(Entities.account, new Dictionary<string, object>
                {
                    { conditionFieldIn, conditionValueIn }
                });
            }

            var recordsOutToCreate = 2;
            for (var i = 0; i < recordsOutToCreate; i++)
            {
                CreateTestRecord(Entities.account, new Dictionary<string, object>
                {
                    { conditionFieldIn, conditionValueIn },
                    { conditionFieldOut, conditionValueOut }
                });
            }

            //Create test app and load query
            var app = CreateAndLoadTestApplication<XrmCrudModule>();
            var crudDialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();
            var queryViewModel = crudDialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);
            queryViewModel.SelectedRecordType = queryViewModel.RecordTypeItemsSource.First(r => r.Key == Entities.account);

            //change to query entry
            queryViewModel.QueryTypeButton.Invoke();
            queryViewModel.IncludeNotInButton.Invoke();

            //in condition
            var lastCondition = queryViewModel.FilterConditions.Conditions.Last();
            var fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First(s => s.Key == conditionFieldIn);
            var conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.Equal).ToString());
            lastCondition.GetStringFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.Value)).Value = conditionValueIn;
            
            //out condition
            lastCondition = queryViewModel.NotInFilterConditions.Conditions.Last();
            fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First(s => s.Key == conditionFieldOut);
            conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
            conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Key == ((int)ConditionType.Equal).ToString());
            lastCondition.GetStringFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.Value)).Value = conditionValueOut;

            //run query
            queryViewModel.DynamicGridViewModel.GetButton("QUERY").Invoke();
            Assert.IsTrue(queryViewModel.GridRecords.Any());

            //set to display totals
            queryViewModel.DynamicGridViewModel.GetButton("DISPLAYTOTALS").Invoke();

            //verify totals for first page
            Assert.AreEqual(50, queryViewModel.DynamicGridViewModel.PageSize);
            Assert.AreEqual(50, queryViewModel.DynamicGridViewModel.GridRecords.Count);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);
            Assert.AreEqual(recordsInToCreate, queryViewModel.DynamicGridViewModel.TotalCount.Value);

            //navigate to next page and verify totals
            queryViewModel.DynamicGridViewModel.NextPageButton.Invoke();
            Assert.AreEqual(recordsInToCreate -50, queryViewModel.DynamicGridViewModel.GridRecords.Count);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);
            Assert.AreEqual(recordsInToCreate, queryViewModel.DynamicGridViewModel.TotalCount.Value);

            //okay lets create an additonal not in record and verify does not get included
            CreateTestRecord(Entities.account, new Dictionary<string, object>
                {
                    { conditionFieldIn, conditionValueIn },
                    { conditionFieldOut, conditionValueOut }
                });

            queryViewModel.DynamicGridViewModel.PreviousPageButton.Invoke();
            Assert.AreEqual(50, queryViewModel.DynamicGridViewModel.PageSize);
            Assert.AreEqual(50, queryViewModel.DynamicGridViewModel.GridRecords.Count);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);
            Assert.AreEqual(recordsInToCreate, queryViewModel.DynamicGridViewModel.TotalCount.Value);

            queryViewModel.DynamicGridViewModel.NextPageButton.Invoke();
            Assert.AreEqual(recordsInToCreate - 50, queryViewModel.DynamicGridViewModel.GridRecords.Count);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);
            Assert.AreEqual(recordsInToCreate, queryViewModel.DynamicGridViewModel.TotalCount.Value);

            //remove not in
            queryViewModel.ResetToQueryEntry();
            queryViewModel.IncludeNotInButton.Invoke();

            queryViewModel.DynamicGridViewModel.GetButton("QUERY").Invoke();
            Assert.IsTrue(queryViewModel.GridRecords.Any());

            //set to display totals
            queryViewModel.DynamicGridViewModel.GetButton("DISPLAYTOTALS").Invoke();

            //verify totals for first page
            Assert.AreEqual(50, queryViewModel.DynamicGridViewModel.PageSize);
            Assert.AreEqual(50, queryViewModel.DynamicGridViewModel.GridRecords.Count);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);
            Assert.AreEqual(recordsInToCreate + recordsOutToCreate + 1, queryViewModel.DynamicGridViewModel.TotalCount.Value);

            //navigate to next page and verify totals
            queryViewModel.DynamicGridViewModel.NextPageButton.Invoke();
            Assert.AreEqual((recordsInToCreate + recordsOutToCreate + 1) - 50, queryViewModel.DynamicGridViewModel.GridRecords.Count);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);
            Assert.AreEqual(recordsInToCreate + recordsOutToCreate + 1, queryViewModel.DynamicGridViewModel.TotalCount.Value);
        }

        /// <summary>
        /// scripts through running a query where fields in a referenced record are added to the grid view
        /// </summary>
        [TestMethod]
        public void XrmCrudQueryTestScriptWithRelatedColumns()
        {
            DeleteAll(Entities.jmcg_testentity);

            //create a record with a parent to add fields from

            var testEntity = CreateRecordAllFieldsPopulated(Entities.jmcg_testentity);
            var testEntityParent = CreateRecordAllFieldsPopulated(Entities.jmcg_testentity);
            testEntity.SetLookupField(Fields.jmcg_testentity_.jmcg_parentid, testEntityParent);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_parentid);

            //Create test app and load query
            var app = CreateAndLoadTestApplication<XrmCrudModule>();
            var crudDialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();
            var queryViewModel = crudDialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);
            queryViewModel.SelectedRecordType = queryViewModel.RecordTypeItemsSource.First(r => r.Key == Entities.jmcg_testentity);
            queryViewModel.RunQueryButton.Invoke();

            //spawn edit columns
            queryViewModel.DynamicGridViewModel.GetButton("EDITCOLUMNS").Invoke();
            Assert.AreEqual(1, queryViewModel.ChildForms.Count);
            var editColumnsDialog = queryViewModel.ChildForms.First() as ColumnEditDialogViewModel;
            Assert.IsNotNull(editColumnsDialog);
            Assert.IsTrue(editColumnsDialog.SelectableColumns.Any());
            
            //select to add fields in the parent record
            editColumnsDialog.SelectedLink = editColumnsDialog.LinkOptions.First(lo => lo.Key.StartsWith(Fields.jmcg_testentity_.jmcg_parentid + "|"));
            Assert.IsTrue(editColumnsDialog.SelectableColumns.Any());
            Assert.IsTrue(editColumnsDialog.SelectableColumns.All(sc => sc.FieldName.StartsWith(editColumnsDialog.SelectedLink.Key)));

            //add all fields in the parent record
            var allParentColumns = editColumnsDialog.SelectableColumns.ToArray();
            foreach (var field in allParentColumns)
            {
                editColumnsDialog.AddCurrentItem(field);
            }

            //lets also add a created by field
            editColumnsDialog.SelectedLink = editColumnsDialog.LinkOptions.First(lo => lo.Key.StartsWith(Fields.jmcg_testentity_.createdby + "|" + Entities.systemuser));
            Assert.IsTrue(editColumnsDialog.SelectableColumns.Any());
            Assert.IsTrue(editColumnsDialog.SelectableColumns.All(sc => sc.FieldName.StartsWith(editColumnsDialog.SelectedLink.Key)));
            editColumnsDialog.AddCurrentItem(editColumnsDialog.SelectableColumns.First(sc => sc.FieldName.EndsWith(Fields.systemuser_.firstname)));

            //apply changes
            editColumnsDialog.ApplyChanges();
            Assert.AreEqual(0, queryViewModel.ChildForms.Count);
            Assert.IsFalse(queryViewModel.DynamicGridViewModel.GridLoadError);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.GridRecords.Any());

            //verify a field in the parent record is populated in the grid view
            var childRow = queryViewModel.DynamicGridViewModel.GridRecords.First(gr => gr.GetRecord().Id == testEntity.Id.ToString());
            Assert.IsTrue(childRow.FieldViewModels.Any(pc => pc.AliasedFieldName != null && pc.AliasedFieldName.Contains(".") && !(pc.ValueObject is bool) && pc.ValueObject != null));

            //check sort works
            queryViewModel.DynamicGridViewModel.SortIt(Fields.jmcg_testentity_.jmcg_parentid + "_" + Entities.jmcg_testentity + "." + Fields.jmcg_testentity_.jmcg_name);
            Assert.IsFalse(queryViewModel.DynamicGridViewModel.GridLoadError);

            //hit edit columns again
            queryViewModel.DynamicGridViewModel.GetButton("EDITCOLUMNS").Invoke();
            Assert.AreEqual(1, queryViewModel.ChildForms.Count);
            editColumnsDialog = queryViewModel.ChildForms.First() as ColumnEditDialogViewModel;
            Assert.IsNotNull(editColumnsDialog);
            Assert.IsTrue(editColumnsDialog.SelectableColumns.Any());

            //apply changes
            editColumnsDialog.ApplyChanges();
            Assert.AreEqual(0, queryViewModel.ChildForms.Count);
            Assert.IsFalse(queryViewModel.DynamicGridViewModel.GridLoadError);
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.GridRecords.Any());
        }

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
            queryViewModel.DynamicGridViewModel.GetButton("DISPLAYTOTALS").Invoke();
            Assert.IsTrue(queryViewModel.DynamicGridViewModel.TotalCount > 0);
            queryViewModel.DynamicGridViewModel.GetButton("DISPLAYTOTALS").Invoke();
            Assert.IsFalse(queryViewModel.DynamicGridViewModel.TotalCount.HasValue);

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

            //okay we should have been directed to a connection entry

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

        private static void DoBulkUpdate(XrmCrudDialog crudDialog, string newValue, string field, bool doExecuteMultiples = false)
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
            var setSizeField = bulkUpdateEntry.GetIntegerFieldFieldViewModel(nameof(BulkUpdateRequest.ExecuteMultipleSetSize));
            setSizeField.Value = doExecuteMultiples ? 3 : 1;
            if (!bulkUpdateEntry.Validate())
                Assert.Fail(bulkUpdateEntry.GetValidationSummary());
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkUpdateDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkUpdateResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }

        private void DoBulkCopyFieldValue(XrmCrudDialog crudDialog, string sourceField, string targetField, bool copyIfNull = false, bool overwriteIfPopulated = false, bool doExecuteMultiples = false)
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
            var setSizeField = bulkUpdateEntry.GetIntegerFieldFieldViewModel(nameof(BulkUpdateRequest.ExecuteMultipleSetSize));
            setSizeField.Value = doExecuteMultiples ? 3 : 1;
            bulkUpdateEntry.GetBooleanFieldFieldViewModel(nameof(BulkCopyFieldValueRequest.CopyIfNull)).Value = copyIfNull;
            bulkUpdateEntry.GetBooleanFieldFieldViewModel(nameof(BulkCopyFieldValueRequest.OverwriteIfPopulated)).Value = overwriteIfPopulated;
            if (!bulkUpdateEntry.Validate())
                Assert.Fail(bulkUpdateEntry.GetValidationSummary());
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkUpdateDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkCopyFieldValueResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }

        private static void DoBulkWorkflow(XrmCrudDialog crudDialog, bool doExecuteMultiples = false)
        {
            var bulkDialog = crudDialog.ChildForms.First() as BulkWorkflowDialog;
            Assert.IsNotNull(bulkDialog);
            bulkDialog.LoadDialog();
            var bulkEntry = bulkDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(bulkEntry);
            var setSizeField = bulkEntry.GetIntegerFieldFieldViewModel(nameof(BulkWorkflowRequest.ExecuteMultipleSetSize));
            setSizeField.Value = doExecuteMultiples ? 2 : 1;
            var waitField = bulkEntry.GetIntegerFieldFieldViewModel(nameof(BulkWorkflowRequest.WaitPerMessage));
            waitField.Value = 2;
            var workflowField = bulkEntry.GetLookupFieldFieldViewModel(nameof(BulkWorkflowRequest.Workflow));
            if(!workflowField.ItemsSource.Any(p => p.Name == "Test Account Workflow On Demand"))
            {
                throw new NullReferenceException("Couldn't select workflow 'Test Account Workflow On Demand'");
            }
            workflowField.Value = workflowField.ItemsSource.First(p => p.Name == "Test Account Workflow On Demand").Record.ToLookup();
            if (!bulkEntry.Validate())
                Assert.Fail(bulkEntry.GetValidationSummary());
            bulkEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkDeleteResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }

        private static void DoBulkDelete(XrmCrudDialog crudDialog, bool doExecuteMultiples = false)
        {
            var bulkDeleteDialog = crudDialog.ChildForms.First() as BulkDeleteDialog;
            Assert.IsNotNull(bulkDeleteDialog);
            bulkDeleteDialog.LoadDialog();
            bulkDeleteDialog.LoadDialog();
            var bulkUpdateEntry = bulkDeleteDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(bulkUpdateEntry);
            var setSizeField = bulkUpdateEntry.GetIntegerFieldFieldViewModel(nameof(BulkUpdateRequest.ExecuteMultipleSetSize));
            setSizeField.Value = doExecuteMultiples ? 3 : 1;
            if (!bulkUpdateEntry.Validate())
                Assert.Fail(bulkUpdateEntry.GetValidationSummary());
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkDeleteDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkDeleteResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }

        private static void DoBulkReplace(XrmCrudDialog crudDialog, string field, string oldValue, string newValue, bool doExecuteMultiples = false)
        {
            var bulkUpdateDialog = crudDialog.ChildForms.First() as BulkReplaceDialog;
            Assert.IsNotNull(bulkUpdateDialog);
            bulkUpdateDialog.LoadDialog();
            var bulkUpdateEntry = bulkUpdateDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(bulkUpdateEntry);
            bulkUpdateEntry.LoadFormSections();

            var fieldGrid = bulkUpdateEntry.GetEnumerableFieldViewModel(nameof(BulkReplaceRequest.FieldsToReplace));
            fieldGrid.AddRow();
            var fieldSelection = fieldGrid.GridRecords.First().GetRecordFieldFieldViewModel(nameof(BulkReplaceRequest.FieldToReplace.RecordField));
            fieldSelection.Value = fieldSelection.ItemsSource.First(kv => kv.Key == field);

            var replacementsGrid = bulkUpdateEntry.GetEnumerableFieldViewModel(nameof(BulkReplaceRequest.ReplacementTexts));
            replacementsGrid.AddRow();
            replacementsGrid.GridRecords.First().GetStringFieldFieldViewModel(nameof(BulkReplaceRequest.ReplacementText.OldText)).Value = oldValue;
            replacementsGrid.GridRecords.First().GetStringFieldFieldViewModel(nameof(BulkReplaceRequest.ReplacementText.NewText)).Value = newValue;

            var setSizeField = bulkUpdateEntry.GetIntegerFieldFieldViewModel(nameof(BulkReplaceRequest.ExecuteMultipleSetSize));
            setSizeField.Value = doExecuteMultiples ? 3 : 1;
            if (!bulkUpdateEntry.Validate())
                Assert.Fail(bulkUpdateEntry.GetValidationSummary());
            bulkUpdateEntry.SaveButtonViewModel.Invoke();
            var completionScreen = bulkUpdateDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkReplaceResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(crudDialog.ChildForms.Any());
        }
    }
}
