using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.XRM.VSIX.Commands.DeployAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Test;
using JosephM.XRM.VSIX.Commands.DeployWebResource;
using JosephM.XRM.VSIX.Commands.ManagePluginTriggers;
using JosephM.XRM.VSIX.Dialogs;
using Fields = JosephM.Xrm.Schema.Fields;
using Entities = JosephM.Xrm.Schema.Entities;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class ManagePluginTriggersTests : JosephMVsixTests
    {
        [TestMethod]
        public void ManagePluginTriggersTest()
        {
            DeployAssembly();

            var assemblyRecord = GetTestPluginAssemblyRecords().First();

            DeletePluginTriggers(assemblyRecord);

            //add one trigger
            var dialog = new ManagePluginTriggersDialog(CreateDialogController(), GetTestPluginAssemblyName(), XrmRecordService);
            dialog.Controller.BeginDialog();

            var entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            var triggersSubGrid = entryViewModel.SubGrids.First();

            triggersSubGrid.AddRow();
            var newRow = triggersSubGrid.GridRecords.First();
            PopulateRowForMessage(newRow, "Update");
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            var triggers = GetPluginTriggers(assemblyRecord);
            Assert.AreEqual(1, triggers.Count());
            //verify preimage created for update
            var image = XrmRecordService.GetFirst(Entities.sdkmessageprocessingstepimage,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, triggers.First().Id);
            Assert.IsNotNull(image);

            //add second trigger
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), GetTestPluginAssemblyName(), XrmRecordService);
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
            dialog = new ManagePluginTriggersDialog(CreateDialogController(), GetTestPluginAssemblyName(), XrmRecordService);
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
                }
            }
        }
    }
}
