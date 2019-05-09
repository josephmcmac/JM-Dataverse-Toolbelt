using JosephM.Application.Desktop.Test;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
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

            var testApplication = CreateAndLoadTestApplication<ManagePluginTriggersModule>();
            //add one update trigger
            RunDialogAndAddMessage(testApplication, "Update");

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
            RunDialogAndAddMessage(testApplication, "Create");
            
            //verify created
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(2, triggers.Count());

            //delete a trigger
            var dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            var entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            var triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.GridRecords.First().DeleteRowViewModel.Invoke();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify deleted
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(1, triggers.Count());

            //add 2 update triggers
            RunDialogAndAddMessage(testApplication, "Update");
            RunDialogAndAddMessage(testApplication, "Update");
            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(3, triggers.Count());

            //okay now lets inspect and adjust the filtering attributes and preimages and impersonating user in one of the update messages
            dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            triggersSubGrid = entryViewModel.SubGrids.First();

            var updateRows = triggersSubGrid.GridRecords.Where(r => r.GetLookupFieldFieldViewModel(nameof(PluginTrigger.Message)).Value.Name == "Update");
            updateRows.First().EditRowViewModel.Invoke();
            var letsAdjustThisOne = testApplication.GetSubObjectEntryViewModel(entryViewModel);
            //set no not all preimage fields
            letsAdjustThisOne.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = false;
            //set some arbitrary other image name
            letsAdjustThisOne.GetStringFieldFieldViewModel(nameof(PluginTrigger.PreImageName)).Value = "FooOthername";
            //set some specific fields in the preimage
            var preImageFieldsField = letsAdjustThisOne.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.PreImageFields));
            SelectItems(preImageFieldsField, 3, 4);
            //set some specific filtering attributes
            var filteringAttributesField = letsAdjustThisOne.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.FilteringFields));
            SelectItems(filteringAttributesField, 3, 4);
            //set impersonating user
            var impersonatingUserField = letsAdjustThisOne.GetLookupFieldFieldViewModel(nameof(PluginTrigger.SpecificUserContext));
            impersonatingUserField.SelectedItem = impersonatingUserField.ItemsSource.First(p => p.Record?.Id == CurrentUserId.ToString());

            Assert.IsTrue(letsAdjustThisOne.Validate());
            letsAdjustThisOne.SaveButtonViewModel.Invoke();
            //save
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.SaveButtonViewModel.Invoke();
            var response = (ManagePluginTriggersResponse)dialog.CompletionItem;
            if (response.HasResponseItemError)
                Assert.Fail(response.GetResponseItemsWithError().First().ErrorDetails);

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
            dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.SaveButtonViewModel.Invoke();

            updatedTrigger = XrmRecordService.Get(updatedTrigger.Type, updatedTrigger.Id);
            Assert.IsNotNull(updatedTrigger.GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes));
            Assert.AreEqual(CurrentUserId.ToString(), updatedTrigger.GetLookupId(Fields.sdkmessageprocessingstep_.impersonatinguserid));
            XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage, Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, triggers.First().Id);
            Assert.IsNotNull(image);
            Assert.IsNotNull(image.GetStringField(Fields.sdkmessageprocessingstepimage_.attributes));
            Assert.AreEqual("FooOthername", image.GetStringField(Fields.sdkmessageprocessingstepimage_.entityalias));

            //now lets verify deletion of an image if changed to not have one (image deleted) as well as clear impersonating user
            dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.GridRecords.First(r => r.GetRecord().GetStringField(nameof(PluginTrigger.Id)) == updatedTrigger.Id).EditRowViewModel.Invoke();
            letsAdjustThisOne = testApplication.GetSubObjectEntryViewModel(entryViewModel);

            impersonatingUserField = letsAdjustThisOne.GetLookupFieldFieldViewModel(nameof(PluginTrigger.SpecificUserContext));
            impersonatingUserField.SelectedItem = impersonatingUserField.ItemsSource.First(p => p.Record == null);
            //set no not all preimage fields
            letsAdjustThisOne.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = false;
            //set no fields on the preimage
            preImageFieldsField = letsAdjustThisOne.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.PreImageFields));
            DeselectAll(preImageFieldsField);

            Assert.IsTrue(letsAdjustThisOne.Validate());
            letsAdjustThisOne.OnSave();

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
            dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            //verify still no impersonation
            updatedTrigger = XrmRecordService.Get(updatedTrigger.Type, updatedTrigger.Id);
            Assert.IsNull(updatedTrigger.GetLookupId(Fields.sdkmessageprocessingstep_.impersonatinguserid));
            //verify still no image
            image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage, Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, updatedTrigger.Id);
            Assert.IsNull(image);

            //add the image again
            dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.GridRecords.First(r => r.GetRecord().GetStringField(nameof(PluginTrigger.Id)) == updatedTrigger.Id).EditRowViewModel.Invoke();
            letsAdjustThisOne = testApplication.GetSubObjectEntryViewModel(entryViewModel);
            //set no not all preimage fields
            letsAdjustThisOne.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = true;
            Assert.IsTrue(letsAdjustThisOne.Validate());
            letsAdjustThisOne.OnSave();
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

        private void RunDialogAndAddMessage(TestApplication testApplication, string message)
        {
            var dialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            var entryViewModel = testApplication.GetSubObjectEntryViewModel(dialog);
            var triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.DynamicGridViewModel.AddRowButton.Invoke();
            var triggerEntryForm = testApplication.GetSubObjectEntryViewModel(entryViewModel);

            foreach (var field in triggerEntryForm.FieldViewModels)
            {
                if (field.ValueObject == null)
                {
                    if (field is LookupFieldViewModel)
                    {
                        var typeFieldViewModel = (LookupFieldViewModel)field;
                        if (field.FieldName == "Message")
                        {
                            typeFieldViewModel.Value = typeFieldViewModel.LookupService.ToLookup(typeFieldViewModel.ItemsSource.First(m => m.Name == message).Record);
                        }
                        else if (typeFieldViewModel.UsePicklist)
                            typeFieldViewModel.Value = typeFieldViewModel.LookupService.ToLookup(typeFieldViewModel.ItemsSource.First(p => p.Record != null).Record); ;
                    }
                    if (field is PicklistFieldViewModel)
                    {
                        var typeFieldViewModel = (PicklistFieldViewModel)field;
                        typeFieldViewModel.Value = typeFieldViewModel.ItemsSource.First();
                    }
                    if (field is RecordTypeFieldViewModel)
                    {
                        var typeFieldViewModel = (RecordTypeFieldViewModel)field;
                        typeFieldViewModel.Value = typeFieldViewModel.ItemsSource.First();
                    }
                    if (field.FieldName == nameof(PluginTrigger.FilteringFields) && message == "Update")
                    {
                        var multiSelectField = triggerEntryForm.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.FilteringFields));
                        SelectItems(multiSelectField, 1, 2);
                    }
                    if (field.FieldName == nameof(PluginTrigger.PreImageAllFields) && message == "Update")
                    {
                        triggerEntryForm.GetFieldViewModel<BooleanFieldViewModel>(nameof(PluginTrigger.PreImageAllFields)).Value = false;
                    }
                    if (field.FieldName == nameof(PluginTrigger.PreImageFields) && message == "Update")
                    {
                        var multiSelectField = triggerEntryForm.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.PreImageFields));
                        SelectItems(multiSelectField, 1, 2);
                    }
                }
            }
            triggerEntryForm.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Mode)).ValueObject = PluginTrigger.PluginMode.Asynch;
            triggerEntryForm.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Stage)).ValueObject = PluginTrigger.PluginStage.PostEvent;
            triggerEntryForm.GetBooleanFieldFieldViewModel(nameof(PluginTrigger.PreImageAllFields)).Value = true;
            triggerEntryForm.GetLookupFieldFieldViewModel(nameof(PluginTrigger.SpecificUserContext)).Value = null;
            var filteringAttributesField = triggerEntryForm.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(PluginTrigger.FilteringFields));
            DeselectAll(filteringAttributesField);

            Assert.IsTrue(triggerEntryForm.Validate());
            triggerEntryForm.SaveButtonViewModel.Invoke();

            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.SaveButtonViewModel.Invoke();

            var response = dialog.CompletionItem as ManagePluginTriggersResponse;
            Assert.IsFalse(response.HasError);
        }

        private static void DeselectAll(RecordFieldMultiSelectFieldViewModel filteringAttributesField)
        {
            var mainForminContent = filteringAttributesField.GetRecordForm();
            if (mainForminContent is GridRowViewModel)
                mainForminContent = mainForminContent.ParentForm;

            filteringAttributesField.EditAction();
            //multiselection is done in a child form so select several and invoke save
            Assert.IsTrue(mainForminContent.ChildForms.Any());
            var filteringAttributesEntry = mainForminContent.ChildForms.First() as MultiSelectDialogViewModel<RecordField>;
            foreach (var item in filteringAttributesEntry.ItemsSource)
            {
                item.Select = false;
            }
            filteringAttributesEntry.ApplyButtonViewModel.Invoke();
            Assert.IsFalse(mainForminContent.ChildForms.Any());
        }

        private static void SelectItems(RecordFieldMultiSelectFieldViewModel multiSelectField, params int[] indexes)
        {
            var mainForminContent = multiSelectField.GetRecordForm();
            if (mainForminContent is GridRowViewModel)
                mainForminContent = mainForminContent.ParentForm;

            multiSelectField.EditAction();
            //multiselection is done in a child form so select several and invoke save
            Assert.IsTrue(mainForminContent.ChildForms.Any());
            var multiSelectEntry = mainForminContent.ChildForms.First() as MultiSelectDialogViewModel<RecordField>;

            foreach (var index in indexes)
                multiSelectEntry.ItemsSource.ElementAt(index).Select = true;
            multiSelectEntry.ApplyButtonViewModel.Invoke();

            Assert.IsFalse(mainForminContent.ChildForms.Any());
        }
    }
}
