using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
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
        [TestMethod]
        public void VsixManagePluginTriggersTest()
        {
            var packageSettings = GetTestPackageSettings();
            DeployAssembly(packageSettings);

            var assemblyRecord = GetTestPluginAssemblyRecords().First();

            DeletePluginTriggers(assemblyRecord);

            //add one trigger
            var dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

            var entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            var triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.AddRow();
            var newRow = triggersSubGrid.GridRecords.First();
            PopulateRowForMessage(newRow, "Update");
            var modeVm = newRow.GetFieldViewModel<PicklistFieldViewModel>(nameof(PluginTrigger.Mode));
            modeVm.ValueObject = PluginTrigger.PluginMode.Asynchronous;
            var stageVm = newRow.GetFieldViewModel<PicklistFieldViewModel>(nameof(PluginTrigger.Stage));
            stageVm.ValueObject = PluginTrigger.PluginStage.PostEvent;
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            var triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(1, triggers.Count());
            Assert.IsTrue(triggers.First().GetBoolField(Fields.sdkmessageprocessingstep_.asyncautodelete));
            Assert.IsNotNull(triggers.First().GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes));

            //verify preimage created for update
            var image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, triggers.First().Id);
            Assert.IsNotNull(image);

            //add second trigger
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.AddRow();
            newRow = triggersSubGrid.GridRecords.First(gr => gr.Record.GetField("Id") == null);
            PopulateRowForMessage(newRow, "Create");
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(2, triggers.Count());

            //delete a trigger
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.GridRecords.First().DeleteRow();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(1, triggers.Count());
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
                            typeFieldViewModel.Value = typeFieldViewModel.LookupService.ToLookup(typeFieldViewModel.ItemsSource.First().Record); ;
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
                }
            }
        }
    }
}
