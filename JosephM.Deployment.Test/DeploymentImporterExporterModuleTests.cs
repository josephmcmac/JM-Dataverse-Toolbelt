using System;
using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Application.ViewModel.Grid;
using JosephM.Xrm;
using JosephM.Application.ViewModel.Query;
using JosephM.Record.Query;
using JosephM.Record.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImporterExporterModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentImporterExporterModuleTest()
        {
            var account = CreateAccount();
            FileUtility.DeleteFiles(TestingFolder);

            var application = CreateAndLoadTestApplication<XrmImporterExporterModule>();

            var instance = new XrmImporterExporterRequest();
            instance.ImportExportTask = ImportExportTask.ExportXml;
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ImportExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    ExcludeTheseFieldsInExportedRecords = new [] { new FieldSetting() { RecordField = new RecordField(Fields.account_.accountcategorycode, Fields.account_.accountcategorycode)}}
                }
            };

            application.NavigateAndProcessDialog<XrmImporterExporterModule, XrmImporterExporterDialog>(instance);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }

        [TestMethod]
        public void DeploymentImporterExporterModuleTestExportSpecificRecordsWithQuery()
        {
            //okay this test is for the new bulk add
            //for lookups in specific record grid, record types and data to export, and fields in fields to exclude
            DeleteAll(Entities.account);

            var account = CreateRecordAllFieldsPopulated(Entities.account);
            FileUtility.DeleteFiles(TestingFolder);

            var accountRecord = XrmRecordService.Get(account.LogicalName, account.Id.ToString());

            var application = CreateAndLoadTestApplication<XrmImporterExporterModule>();

            var instance = new XrmImporterExporterRequest();
            instance.ImportExportTask = ImportExportTask.ExportXml;
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ImportExportRecordType()
                {
                    Type = ExportType.SpecificRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    SpecificRecordsToExport = new [] { new LookupSetting() {  Record = accountRecord.ToLookup()} }
                }
            };

            var entryForm = application.NavigateToDialogModuleEntryForm<XrmImporterExporterModule, XrmImporterExporterDialog>();
            application.EnterObject(instance, entryForm);
            var recordTypesGrid = entryForm.GetSubGridViewModel(nameof(XrmImporterExporterRequest.RecordTypesToExport));
            var row = recordTypesGrid.GridRecords.First();

            //edit the accounts export record
            row.EditRow();
            var specificRecordEntry = entryForm.ChildForms.First() as RecordEntryFormViewModel;
            var specificRecordsGrid = specificRecordEntry.GetSubGridViewModel(nameof(ImportExportRecordType.SpecificRecordsToExport));
            //delete the row we added
            specificRecordsGrid.GridRecords.First().DeleteRow();
            Assert.IsFalse(specificRecordsGrid.GridRecords.Any());
            //now add using the add multiple option
            var customFunction = specificRecordsGrid.DynamicGridViewModel.AddMultipleRowButton;
            customFunction.Invoke();
            var bulkAddForm = specificRecordEntry.ChildForms.First() as QueryViewModel;
            //verify a quickfind finds a record
            bulkAddForm.QuickFindText = account.GetStringField(Fields.account_.name);
            bulkAddForm.QuickFind();
            Assert.IsFalse(bulkAddForm.DynamicGridViewModel.GridLoadError, bulkAddForm.DynamicGridViewModel.ErrorMessage);
            Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());
            //now do an and query on every field in the entity and verify it works
            bulkAddForm.QueryTypeButton.Invoke();

            var lastCondition = bulkAddForm.FilterConditions.Conditions.Last();
            Assert.AreEqual(Entities.account, lastCondition.GetRecordTypeFieldViewModel(nameof(ConditionViewModel.QueryCondition.RecordType)).Value.Key);
            var fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
            var validSearchFields = fieldViewModel.ItemsSource.Select(i => i.Key).ToArray();

            foreach (var field in validSearchFields)
            {
                var fieldvalue = accountRecord.GetField(field);
                if (fieldvalue != null)
                {
                    lastCondition = bulkAddForm.FilterConditions.Conditions.Last();
                    Assert.AreEqual(Entities.account, lastCondition.GetRecordTypeFieldViewModel(nameof(ConditionViewModel.QueryCondition.RecordType)).Value.Key);
                    fieldViewModel = lastCondition.GetRecordFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.FieldName));
                    fieldViewModel.Value = fieldViewModel.ItemsSource.ToArray().First(i => i.Key == field);
                    var conditionTypeViewModel = lastCondition.GetPicklistFieldFieldViewModel(nameof(ConditionViewModel.QueryCondition.ConditionType));
                    conditionTypeViewModel.Value = conditionTypeViewModel.ItemsSource.First(i => i.Value == ConditionType.Equal.ToString());
                    var valueViewModel = lastCondition.GetFieldViewModel(nameof(ConditionViewModel.QueryCondition.Value));
                    valueViewModel.ValueObject = fieldvalue;
                }
            }
            bulkAddForm.QuickFind();
            Assert.IsFalse(bulkAddForm.DynamicGridViewModel.GridLoadError, bulkAddForm.DynamicGridViewModel.ErrorMessage);
            Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());
            //select and add
            bulkAddForm.DynamicGridViewModel.GridRecords.First().IsSelected = true;
            //this triggered by the grid event
            bulkAddForm.DynamicGridViewModel.OnSelectionsChanged();
            //this is supposed to be the add selected button
            bulkAddForm.DynamicGridViewModel.CustomFunctions.Last().Invoke();
            Assert.IsTrue(specificRecordsGrid.GridRecords.Any());

            var excludeFieldsGrid = specificRecordEntry.GetSubGridViewModel(nameof(ImportExportRecordType.ExcludeTheseFieldsInExportedRecords));
            //now add using the add multiple option
            excludeFieldsGrid.DynamicGridViewModel.AddMultipleRowButton.Invoke();
            bulkAddForm = specificRecordEntry.ChildForms.First() as QueryViewModel;
            Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());

            bulkAddForm.QuickFindText = Fields.account_.name;
            bulkAddForm.QuickFind();

            Assert.IsFalse(bulkAddForm.DynamicGridViewModel.GridLoadError, bulkAddForm.DynamicGridViewModel.ErrorMessage);
            Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());

            bulkAddForm.DynamicGridViewModel.GridRecords.First().IsSelected = true;
            //this triggered by the grid event
            bulkAddForm.DynamicGridViewModel.OnSelectionsChanged();
            //this is supposed to be the add selected button
            bulkAddForm.DynamicGridViewModel.CustomFunctions.Last().Invoke();
            Assert.IsFalse(specificRecordEntry.ChildForms.Any());

            specificRecordEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryForm.ChildForms.Any());

            var subGrid = entryForm.GetSubGridViewModel(nameof(XrmImporterExporterRequest.RecordTypesToExport));
            subGrid.DynamicGridViewModel.AddMultipleRowButton.Invoke();

            bulkAddForm = entryForm.ChildForms.First() as QueryViewModel;

            Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());

            bulkAddForm.QuickFindText = Entities.contact;
            bulkAddForm.QuickFind();

            Assert.IsFalse(bulkAddForm.DynamicGridViewModel.GridLoadError, bulkAddForm.DynamicGridViewModel.ErrorMessage);
            Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());

            bulkAddForm.DynamicGridViewModel.GridRecords.First().IsSelected = true;
            //this triggered by the grid event
            bulkAddForm.DynamicGridViewModel.OnSelectionsChanged();
            //this is supposed to be the add selected button
            bulkAddForm.DynamicGridViewModel.CustomFunctions.Last().Invoke();

            Assert.IsFalse(entryForm.ChildForms.Any());
        }
    }
}
