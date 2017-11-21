using JosephM.Application.Application;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportXml;
using JosephM.Prism.Infrastructure.Module.SavedRequests;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Xrm;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentExportXmlModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentExportXmlModuleTest()
        {
            //script 2 exports

            //first one with only one specific field

            var account = CreateAccount();
            FileUtility.DeleteFiles(TestingFolder);

            var application = CreateAndLoadTestApplication<ExportXmlModule>();

            var instance = new ExportXmlRequest();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    IncludeAllFields = false,
                    IncludeOnlyTheseFieldsInExportedRecords = new [] { new FieldSetting() { RecordField = new RecordField(Fields.account_.createdby, Fields.account_.createdby) }}
                }
            };

            var response = application.NavigateAndProcessDialog<ExportXmlModule, ExportXmlDialog, ExportXmlResponse>(instance);
            Assert.IsFalse(response.HasError);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            var importXmlService = new ImportXmlService(XrmRecordService);
            var loaded = importXmlService.LoadEntitiesFromXmlFiles(TestingFolder);
            foreach(var item in loaded)
            {
                Assert.IsNull(item.GetField(Fields.account_.createdon));
                Assert.IsNotNull(item.GetField(Fields.account_.createdby));
                Assert.IsNotNull(item.GetField(Fields.account_.name));
            }

            FileUtility.DeleteFiles(TestingFolder);

            //first verify when all fields selected
            application = CreateAndLoadTestApplication<ExportXmlModule>();

            instance = new ExportXmlRequest();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    IncludeAllFields = true
                }
            };

            response = application.NavigateAndProcessDialog<ExportXmlModule, ExportXmlDialog, ExportXmlResponse>(instance);
            Assert.IsFalse(response.HasError);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            loaded = importXmlService.LoadEntitiesFromXmlFiles(TestingFolder);
            foreach (var item in loaded)
            {
                Assert.IsNotNull(item.GetField(Fields.account_.createdon));
                Assert.IsNotNull(item.GetField(Fields.account_.createdby));
                Assert.IsNotNull(item.GetField(Fields.account_.name));
            }
        }

        /// <summary>
        /// This test scripts through the bulk add function on a subgrid
        /// as well as running a query on all fields in the account type
        /// the bulk add was added to this function to bulk add record types, fields and lookup records
        /// rather than selecting them oine by one in the grid
        /// </summary>
        [TestMethod]
        public void DeploymentExportXmlModuleTestExportWithBulkAddToGridAndQuery()
        {
            DeleteAll(Entities.account);

            var account = CreateRecordAllFieldsPopulated(Entities.account);
            FileUtility.DeleteFiles(TestingFolder);

            var accountRecord = XrmRecordService.Get(account.LogicalName, account.Id.ToString());

            //okay create/navigate to a new entry form entering an ExportXmlRequest
            var application = CreateAndLoadTestApplication<ExportXmlModule>();
            var instance = new ExportXmlRequest();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.SpecificRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    SpecificRecordsToExport = new [] { new LookupSetting() {  Record = accountRecord.ToLookup()} }
                }
            };
            var entryForm = application.NavigateToDialogModuleEntryForm<ExportXmlModule, ExportXmlDialog>();
            application.EnterObject(instance, entryForm);

            //get the record types subgrid
            var recordTypesGrid = entryForm.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));
            var row = recordTypesGrid.GridRecords.First();

            //edit the accounts export record row
            row.EditRow();
            var specificRecordEntry = entryForm.ChildForms.First() as RecordEntryFormViewModel;
            specificRecordEntry.LoadFormSections();
            var specificRecordsGrid = specificRecordEntry.GetEnumerableFieldViewModel(nameof(ExportRecordType.SpecificRecordsToExport));
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
                    //bulkAddForm.QuickFind();
                    //Assert.IsFalse(bulkAddForm.DynamicGridViewModel.GridLoadError, bulkAddForm.DynamicGridViewModel.ErrorMessage);
                    //Assert.IsTrue(bulkAddForm.DynamicGridViewModel.GridRecords.Any());
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

            //and verify the row was added to the records for export
            Assert.IsTrue(specificRecordsGrid.GridRecords.Any());

            //okay now lets do the equivalent for a grid of fields

            //set this false so the selection of fields is in context
            specificRecordEntry.GetBooleanFieldFieldViewModel(nameof(ExportRecordType.IncludeAllFields)).Value = false;

            //get the fields grid and trigger bulk add function
            var excludeFieldsGrid = specificRecordEntry.GetEnumerableFieldViewModel(nameof(ExportRecordType.IncludeOnlyTheseFieldsInExportedRecords));
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
            //and verify the row was added to the records for export
            Assert.IsTrue(excludeFieldsGrid.GridRecords.Any());

            specificRecordEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryForm.ChildForms.Any());

            //okay now lets to bulk add on the record types grid
            var subGrid = entryForm.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));
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

        /// <summary>
        /// This test scripts through the bulk add function on a subgrid's enumerbale field
        /// the bulk add was added to the enumerable field in the grid so you don't have to open/edit the row
        /// to bulk add items to it
        /// </summary>
        [TestMethod]
        public void DeploymentExportXmlModuleTestExportWithBulkAddToGridField()
        {
            DeleteAll(Entities.account);

            var account = CreateRecordAllFieldsPopulated(Entities.account);
            FileUtility.DeleteFiles(TestingFolder);

            var accountRecord = XrmRecordService.Get(account.LogicalName, account.Id.ToString());

            var application = CreateAndLoadTestApplication<ExportXmlModule>();

            var instance = new ExportXmlRequest();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    SpecificRecordsToExport = new LookupSetting[0]
                }
            };

            var entryForm = application.NavigateToDialogModuleEntryForm<ExportXmlModule, ExportXmlDialog>();
            application.EnterObject(instance, entryForm);
            var recordTypesGrid = entryForm.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));

            //okay so we will be doing bulk adds on fields in this grid row
            var row = recordTypesGrid.GridRecords.First();
            row.GetPicklistFieldFieldViewModel(nameof(ExportRecordType.Type)).Value = PicklistOption.EnumToPicklistOption(ExportType.SpecificRecords);
            //first do it for an Enumerable lookup field (specific records for export)
            var specificRecordsGridField = row.GetEnumerableFieldViewModel(nameof(ExportRecordType.SpecificRecordsToExport));
            Assert.IsTrue(string.IsNullOrWhiteSpace(specificRecordsGridField.StringDisplay));
            Assert.IsNull(specificRecordsGridField.DynamicGridViewModel);
            Assert.IsNotNull(specificRecordsGridField.BulkAddButton);
           
            //trigger the add multiple option
            specificRecordsGridField.BulkAddButton.Invoke();
            var bulkAddForm = entryForm.ChildForms.First() as QueryViewModel;
            
            //verify a quickfind finds a record
            bulkAddForm.QuickFindText = account.GetStringField(Fields.account_.name);
            bulkAddForm.QuickFind();

            //select and add
            bulkAddForm.DynamicGridViewModel.GridRecords.First().IsSelected = true;
            //this triggered by the grid event
            bulkAddForm.DynamicGridViewModel.OnSelectionsChanged();
            //this is supposed to be the add selected button
            bulkAddForm.DynamicGridViewModel.CustomFunctions.Last().Invoke();
            //verify we now have a record selected and displayed for the field
            Assert.IsFalse(string.IsNullOrWhiteSpace(specificRecordsGridField.StringDisplay));

            //now do it for an Enumerable field (fields for inlcusion)
            //this sets it in context
            row.GetBooleanFieldFieldViewModel(nameof(ExportRecordType.IncludeAllFields)).Value = false;
            var excludeFieldsGrid = row.GetEnumerableFieldViewModel(nameof(ExportRecordType.IncludeOnlyTheseFieldsInExportedRecords));
            Assert.IsTrue(string.IsNullOrWhiteSpace(excludeFieldsGrid.StringDisplay));
           
            //trigger the add multiple option
            excludeFieldsGrid.BulkAddButton.Invoke();
            bulkAddForm = entryForm.ChildForms.First() as QueryViewModel;
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
            Assert.IsFalse(entryForm.ChildForms.Any());
            //verify we now have a record selected and displayed for the field
            Assert.IsFalse(string.IsNullOrWhiteSpace(excludeFieldsGrid.StringDisplay));
        }

        /// <summary>
        /// This test scripts through the setting explicit field values in the exported records
        /// as of the implementation need to add a row to the grid and enter into the entry form
        /// as cannot do dynamic field type in a grid when the field to set is selected
        /// </summary>
        [TestMethod]
        public void DeploymentExportXmlModuleTestExportWithSpecificValues()
        {
            DeleteAll(Entities.account);

            var account = CreateRecordAllFieldsPopulated(Entities.account);
            FileUtility.DeleteFiles(TestingFolder);

            var accountRecord = XrmRecordService.Get(account.LogicalName, account.Id.ToString());

            var application = CreateAndLoadTestApplication<ExportXmlModule>();
            application.AddModule<SavedRequestModule>();

            var instance = new ExportXmlRequest();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    SpecificRecordsToExport = new LookupSetting[0]
                }
            };

            var entryForm = application.NavigateToDialogModuleEntryForm<ExportXmlModule, ExportXmlDialog>();
            application.EnterObject(instance, entryForm);
            var recordTypesGrid = entryForm.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));

            var row = recordTypesGrid.GridRecords.First();
            row.EditRow();

            var exportTypeEntry = entryForm.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(exportTypeEntry);
            exportTypeEntry.LoadFormSections();

            //okay so at this point we aere in the export type form
            //need to add a row to the explicit value grid which will open the form
            var specificValuesGrid = exportTypeEntry.GetEnumerableFieldViewModel(nameof(ExportRecordType.ExplicitValuesToSet));
            specificValuesGrid.AddRow();
            var specificValueEntry = exportTypeEntry.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(specificValueEntry);
            specificValueEntry.LoadFormSections();

            var fieldSelectionViewModel = specificValueEntry.GetRecordFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.FieldToSet));
            var clearValueViewModel = specificValueEntry.GetBooleanFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ClearValue));

            //select several field types and verify the field control changes to the correct type for that field
            fieldSelectionViewModel.Value = fieldSelectionViewModel.ItemsSource.First(f => f.Key == Fields.account_.customertypecode);
            Assert.IsTrue(specificValueEntry.GetFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)) is PicklistFieldViewModel);
            Assert.IsTrue(specificValueEntry.GetPicklistFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)).ItemsSource.Any());

            fieldSelectionViewModel.Value = fieldSelectionViewModel.ItemsSource.First(f => f.Key == Fields.account_.primarycontactid);
            Assert.IsTrue(specificValueEntry.GetFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)) is LookupFieldViewModel);
            specificValueEntry.GetLookupFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)).Search();
            Assert.IsTrue(specificValueEntry.GetLookupFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)).LookupGridViewModel.DynamicGridViewModel.GridRecords.Any());

            //verify the field value hidden if we select to clear the value
            clearValueViewModel.Value = true;
            Assert.IsFalse(specificValueEntry.GetFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)).IsVisible);
            clearValueViewModel.Value = false;

            //okay so this is the specific field and value we will set
            var fakeExplicitExportValue = "fakeExplicitExportValue";
            fieldSelectionViewModel.Value = fieldSelectionViewModel.ItemsSource.First(f => f.Key == Fields.account_.address1_line1);
            Assert.IsTrue(specificValueEntry.GetFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet)) is StringFieldViewModel);
            var descriptionViewModel = specificValueEntry.GetStringFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet));
            descriptionViewModel.Value = fakeExplicitExportValue;

            Assert.IsTrue(specificValueEntry.Validate());
            specificValueEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(exportTypeEntry.ChildForms.Any());

            //okay lets add an explicit lookup value as well
            specificValuesGrid = exportTypeEntry.GetEnumerableFieldViewModel(nameof(ExportRecordType.ExplicitValuesToSet));
            specificValuesGrid.AddRow();
            specificValueEntry = exportTypeEntry.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(specificValueEntry);
            specificValueEntry.LoadFormSections();

            fieldSelectionViewModel = specificValueEntry.GetRecordFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.FieldToSet));
            fieldSelectionViewModel.Value = fieldSelectionViewModel.ItemsSource.First(f => f.Key == Fields.account_.primarycontactid);
            var lookupFieldViewModel = specificValueEntry.GetLookupFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet));
            lookupFieldViewModel.Search();
            Assert.IsTrue(lookupFieldViewModel.LookupGridViewModel.DynamicGridViewModel.GridRecords.Any());
            lookupFieldViewModel.LookupGridViewModel.DynamicGridViewModel.SelectedRow = lookupFieldViewModel.LookupGridViewModel.DynamicGridViewModel.GridRecords.First();
            lookupFieldViewModel.OnRecordSelected(lookupFieldViewModel.LookupGridViewModel.DynamicGridViewModel.GridRecords.First().Record);

            Assert.IsTrue(specificValueEntry.Validate());
            specificValueEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(exportTypeEntry.ChildForms.Any());

            //okay lets add an explicit picklist value as well
            specificValuesGrid = exportTypeEntry.GetEnumerableFieldViewModel(nameof(ExportRecordType.ExplicitValuesToSet));
            specificValuesGrid.AddRow();
            specificValueEntry = exportTypeEntry.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(specificValueEntry);
            specificValueEntry.LoadFormSections();

            fieldSelectionViewModel = specificValueEntry.GetRecordFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.FieldToSet));
            fieldSelectionViewModel.Value = fieldSelectionViewModel.ItemsSource.First(f => f.Key == Fields.account_.customertypecode);
            var picklistFieldViewModel = specificValueEntry.GetPicklistFieldFieldViewModel(nameof(ExportRecordType.ExplicitFieldValues.ValueToSet));
            Assert.IsTrue(picklistFieldViewModel.ItemsSource.Any());
            picklistFieldViewModel.Value = picklistFieldViewModel.ItemsSource.First();

            Assert.IsTrue(specificValueEntry.Validate());
            specificValueEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(exportTypeEntry.ChildForms.Any());

            Assert.IsTrue(exportTypeEntry.Validate());
            exportTypeEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryForm.ChildForms.Any());

            //okay lets verify save and load object as well
            //initially the dynamic object property for setting san explicit type
            //did not seralise due to known types in the serialiser

            //trigger save request
            var saveRequestButton = entryForm.GetButton("SAVEREQUEST");
            saveRequestButton.Invoke();

            //enter and save details
            var saveRequestForm = application.GetSubObjectEntryViewModel(entryForm);
            var detailsEntered = new SaveAndLoadFields()
            {
                Name = "TestName"
            };
            application.EnterAndSaveObject(detailsEntered, saveRequestForm);
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);

            //trigger load request
            var loadRequestButton = entryForm.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();
            var loadRequestForm = application.GetSubObjectEntryViewModel(entryForm);
            //select and load the saved request
            var subGrid = loadRequestForm.GetEnumerableFieldViewModel(nameof(SavedSettings.SavedRequests));
            Assert.IsTrue(subGrid.GridRecords.Count() == 1);
            subGrid.GridRecords.First().IsSelected = true;
            var loadButton = subGrid.DynamicGridViewModel.GetButton("LOADREQUEST");
            loadButton.Invoke();
            //verify loads
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);

            //this one will invoke the export
            Assert.IsTrue(entryForm.Validate());
            entryForm.SaveButtonViewModel.Invoke();

            //verify the exported records had the explicit value we set in them
            var importServoice = new ImportXmlService(XrmRecordService);
            var loadEntities = importServoice.LoadEntitiesFromXmlFiles(TestingFolder);

            foreach(var entity in loadEntities)
            {
                Assert.AreEqual(fakeExplicitExportValue, entity.GetStringField(Fields.account_.address1_line1));
            }
        }
    }
}
