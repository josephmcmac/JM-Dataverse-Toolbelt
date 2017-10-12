using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.ObjectMapping;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Xrm.Sdk.Client;
using JosephM.Record.IService;
using JosephM.Prism.XrmModule.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Application.ViewModel.Grid;
using System.Linq;
using JosephM.Xrm.Schema;
using System;
using JosephM.Record.Extentions;

namespace JosephM.Prism.XrmModule.Test
{
    [TestClass]
    public class XrmCrudModuleTest : XrmModuleTest
    {
        [TestMethod]
        public void XrmCrudModuleTestScript()
        {
            //todo consider anything else necessary e.g. change other field types

            //this script is just to verify a simple scenario
            //1. opening the query view
            //2. runnning the query
            //3. opening a record
            //4. changing the name
            //5. saving
            //and verify the record is updated

            var account = XrmRecordService.GetFirst(Entities.account);
            if (account == null)
                CreateAccount();

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
            queryViewModel.GridRecords.First().IsSelected = true;
            //this triggered by ui event
            queryViewModel.DynamicGridViewModel.OnSelectionsChanged();

            queryViewModel.DynamicGridViewModel.GetButton("OPEN").Invoke();
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
        }
    }
}
