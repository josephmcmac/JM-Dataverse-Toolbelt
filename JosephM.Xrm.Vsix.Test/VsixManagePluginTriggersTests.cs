using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Entities = JosephM.Xrm.Schema.Entities;
using Fields = JosephM.Xrm.Schema.Fields;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixManagePluginTriggersTests : JosephMVsixTests
    {
        /// <summary>
        /// Scripts through several scenarios for changing the plugin triggers
        /// create, delete, configuring filtering attributes, pre images etc
        /// </summary>
        [TestMethod]
        public void VsixManagePluginTriggersTest()
        {
            var packageSettings = GetTestPackageSettings();
            DeployAssembly(packageSettings);

            var assemblyRecord = GetTestPluginAssemblyRecords().First();

            DeletePluginTriggers(assemblyRecord);

            //add one update trigger
            RunDialogAndAddMessage("Update");

            //verify trigger created
            var triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(1, triggers.Count());
            Assert.IsTrue(triggers.First().GetBoolField(Fields.sdkmessageprocessingstep_.asyncautodelete));
            Assert.IsNull(triggers.First().GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes));
            Assert.IsNull(triggers.First().GetStringField(Fields.sdkmessageprocessingstep_.impersonatinguserid));

            //verify preimage created for update with all fields
            var image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, triggers.First().Id);
            Assert.IsNotNull(image);
            Assert.IsNull(image.GetStringField(Fields.sdkmessageprocessingstepimage_.attributes));
            Assert.AreEqual("PreImage", image.GetStringField(Fields.sdkmessageprocessingstepimage_.entityalias));

            //add one create trigger
            RunDialogAndAddMessage("Create");
            
            //verify created
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(2, triggers.Count());

            //delete a trigger
            var dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

            var entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            var triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.GridRecords.First().DeleteRow();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify deleted
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(1, triggers.Count());


            //add 2 update triggers
            RunDialogAndAddMessage("Update");
            RunDialogAndAddMessage("Update");
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(3, triggers.Count());

            //okay now lets inspect and adjust the filtering attributes and preimages and impersonating user in one of the update messages
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            triggersSubGrid = entryViewModel.SubGrids.First();

            var updateRows = triggersSubGrid.GridRecords.Where(r => r.GetLookupFieldFieldViewModel(nameof(PluginTrigger.Message)).Value.Name == "Update");
            var letsAdjustThisOne = updateRows.First();
            //set no not all preimage fields
            letsAdjustThisOne.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = false;
            //set some arbitrary other image name
            letsAdjustThisOne.GetStringFieldFieldViewModel(nameof(PluginTrigger.PreImageName)).Value = "FooOthername";
            //set some specific fields in the preimage
            var preImageFieldsField = letsAdjustThisOne.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.PreImageFields));
            preImageFieldsField.MultiSelectsVisible = true;
            preImageFieldsField.DynamicGridViewModel.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            preImageFieldsField.DynamicGridViewModel.GridRecords.ElementAt(3).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            //set some specific filtering attributes
            var filteringAttributesField = letsAdjustThisOne.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.FilteringFields));
            filteringAttributesField.MultiSelectsVisible = true;
            filteringAttributesField.DynamicGridViewModel.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            filteringAttributesField.DynamicGridViewModel.GridRecords.ElementAt(3).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            //set impersonating user
            var impersonatingUserField = letsAdjustThisOne.GetLookupFieldFieldViewModel(nameof(PluginTrigger.SpecificUserContext));
            impersonatingUserField.SelectedItem = impersonatingUserField.ItemsSource.First(p => p.Record?.Id == CurrentUserId.ToString());


            //save
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify still 3 triggers
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(3, triggers.Count());

            //get the record we updated
            var updatedTriggerMatches = triggers.Where(t => t.GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes) != null);
            Assert.AreEqual(1, updatedTriggerMatches.Count());
            var updatedTrigger = updatedTriggerMatches.First();
            //verify the filtering and image fields we set got saved correctly
            Assert.IsNotNull(updatedTrigger.GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes));
            Assert.AreEqual(CurrentUserId.ToString(), updatedTrigger.GetLookupId(Fields.sdkmessageprocessingstep_.impersonatinguserid));
            image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, updatedTrigger.Id);
            Assert.IsNotNull(image);
            Assert.IsNotNull(image.GetStringField(Fields.sdkmessageprocessingstepimage_.attributes));
            Assert.AreEqual("FooOthername", image.GetStringField(Fields.sdkmessageprocessingstepimage_.entityalias));

            //lets just verify if we go through te dialog without touching the record we adjusted that it is still the same after the save
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            updatedTrigger = XrmRecordService.Get(updatedTrigger.Type, updatedTrigger.Id);
            Assert.IsNotNull(updatedTrigger.GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes));
            Assert.AreEqual(CurrentUserId.ToString(), updatedTrigger.GetLookupId(Fields.sdkmessageprocessingstep_.impersonatinguserid));
            XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage, Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, triggers.First().Id);
            Assert.IsNotNull(image);
            Assert.IsNotNull(image.GetStringField(Fields.sdkmessageprocessingstepimage_.attributes));
            Assert.AreEqual("FooOthername", image.GetStringField(Fields.sdkmessageprocessingstepimage_.entityalias));

            //now lets verify deletion of an image if changed to not have one (image deleted) as well as clear impersonating user
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            triggersSubGrid = entryViewModel.SubGrids.First();

            letsAdjustThisOne = triggersSubGrid.GridRecords.First(r => r.GetRecord().GetStringField(nameof(PluginTrigger.Id)) == updatedTrigger.Id);
            impersonatingUserField = letsAdjustThisOne.GetLookupFieldFieldViewModel(nameof(PluginTrigger.SpecificUserContext));
            impersonatingUserField.SelectedItem = impersonatingUserField.ItemsSource.First(p => p.Record == null);
            //set no not all preimage fields
            letsAdjustThisOne.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = false;
            //set no fields on the preimage
            preImageFieldsField = letsAdjustThisOne.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.PreImageFields));
            preImageFieldsField.MultiSelectsVisible = true;
            foreach (var field in preImageFieldsField.DynamicGridViewModel.GridRecords)
            {
                field.GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = false;
            }
            //save
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify now no impersonation
            updatedTrigger = XrmRecordService.Get(updatedTrigger.Type, updatedTrigger.Id);
            Assert.IsNull(updatedTrigger.GetLookupId(Fields.sdkmessageprocessingstep_.impersonatinguserid));

            //verify no image
            image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage, Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, updatedTrigger.Id);
            Assert.IsNull(image);

            //lets just verify if we go through te dialog without touching the record still doesn't have one
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify still no impersonation
            updatedTrigger = XrmRecordService.Get(updatedTrigger.Type, updatedTrigger.Id);
            Assert.IsNull(updatedTrigger.GetLookupId(Fields.sdkmessageprocessingstep_.impersonatinguserid));
            //verify still no image
            image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage, Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, updatedTrigger.Id);
            Assert.IsNull(image);

            //add the image again
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            triggersSubGrid = entryViewModel.SubGrids.First();

            letsAdjustThisOne = triggersSubGrid.GridRecords.First(r => r.GetRecord().GetStringField(nameof(PluginTrigger.Id)) == updatedTrigger.Id);
            //set no not all preimage fields
            letsAdjustThisOne.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = true;
            //save
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify image created
            image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage, Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, updatedTrigger.Id);
            Assert.IsNotNull(image);
            Assert.IsNull(image.GetStringField(Fields.sdkmessageprocessingstepimage_.attributes));

            var solutionComponents = XrmRecordService.GetSolutionComponents(packageSettings.Solution.Id, OptionSets.SolutionComponent.ObjectTypeCode.SDKMessageProcessingStep);
            Assert.IsTrue(triggers.All(t => solutionComponents.Contains(t.Id)));
        }

        private void RunDialogAndAddMessage(string message)
        {
            var packageSettings = GetTestPackageSettings(); ;
            var dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
            var entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            var triggersSubGrid = entryViewModel.SubGrids.First();
            var newRow = AddtriggerForMessage(triggersSubGrid, message);
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();
        }

        private static GridRowViewModel AddtriggerForMessage(EnumerableFieldViewModel triggersSubGrid, string message)
        {
            triggersSubGrid.AddRow();
            var newRow = triggersSubGrid.GridRecords.First();
            PopulateRowForMessage(newRow, message);
            return newRow;
        }

        private static void PopulateRowForMessage(GridRowViewModel newRow, string message)
        {
            foreach (var field in newRow.FieldViewModels)
            {
                if (field.ValueObject == null)
                {
                    if (field is LookupFieldViewModel)
                    {
                        var typeFieldViewModel = (LookupFieldViewModel) field;
                        if (field.FieldName == "Message")
                        {
                            typeFieldViewModel.Value = typeFieldViewModel.LookupService.ToLookup(typeFieldViewModel.ItemsSource.First(m => m.Name == message).Record);
                        }
                        else if (typeFieldViewModel.UsePicklist)
                            typeFieldViewModel.Value = typeFieldViewModel.LookupService.ToLookup(typeFieldViewModel.ItemsSource.First(p => p.Record != null).Record); ;
                    }
                    if (field is PicklistFieldViewModel)
                    {
                        var typeFieldViewModel = (PicklistFieldViewModel) field;
                        typeFieldViewModel.Value = typeFieldViewModel.ItemsSource.First();
                    }
                    if (field is RecordTypeFieldViewModel)
                    {
                        var typeFieldViewModel = (RecordTypeFieldViewModel) field;
                        typeFieldViewModel.Value = typeFieldViewModel.ItemsSource.First();
                    }
                    if (field.FieldName == nameof(PluginTrigger.FilteringFields) && message == "Update")
                    {
                        var multiSelectField = newRow.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.FilteringFields));
                        multiSelectField.MultiSelectsVisible = true;
                        multiSelectField.DynamicGridViewModel.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
                        multiSelectField.DynamicGridViewModel.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
                    }
                    if (field.FieldName == nameof(PluginTrigger.PreImageAllFields) && message == "Update")
                    {
                        newRow.GetFieldViewModel<BooleanFieldViewModel>(nameof(PluginTrigger.PreImageAllFields)).Value = false;
                    }
                    if (field.FieldName == nameof(PluginTrigger.PreImageFields) && message == "Update")
                    {
                        var multiSelectField = newRow.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.PreImageFields));
                        multiSelectField.MultiSelectsVisible = true;
                        multiSelectField.DynamicGridViewModel.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
                        multiSelectField.DynamicGridViewModel.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
                    }
                }
            }
            newRow.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Mode)).ValueObject = PluginTrigger.PluginMode.Asynchronous;
            newRow.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Stage)).ValueObject = PluginTrigger.PluginStage.PostEvent;
            newRow.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = true;
            newRow.GetLookupFieldFieldViewModel(nameof(PluginTrigger.SpecificUserContext)).Value = null;
            var filteringAttributesField = newRow.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.FilteringFields));
            filteringAttributesField.MultiSelectsVisible = true;
            foreach (var field in filteringAttributesField.DynamicGridViewModel.GridRecords)
            {
                field.GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = false;
            }
        }
    }
}
