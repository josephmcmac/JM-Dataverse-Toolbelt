using JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using JosephM.Xrm;
using JosephM.Xrm.Autonumber;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;


namespace JosephM.CustomisationImporter.Test
{
    [TestClass]
    public class AutonumberTests : XrmModuleTest
    {
        [TestMethod]
        public void AutonumberTest()
        {
            if(int.Parse(XrmService.OrganisationVersion.Split('.')[0]) < 9)
            {
                Assert.Inconclusive("Organisation must be at least V9");
            }

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<AutonumberModule>();

            var dialog = testApplication.NavigateToDialog<AutonumberModule, AutonumberDialog>();

            var autonumbersViewModel = dialog.Controller.UiItems.First() as ObjectEntryViewModel;

            var recordTypeFieldViewModel = autonumbersViewModel.GetRecordTypeFieldViewModel(nameof(AutonumberNavigator.RecordType));
            recordTypeFieldViewModel.Value = recordTypeFieldViewModel.ItemsSource.First(r => r.Key == Entities.account);

            var fieldsViewModel = autonumbersViewModel.GetEnumerableFieldViewModel(nameof(AutonumberNavigator.AutonumberFields));
            while (fieldsViewModel.DynamicGridViewModel.GridRecords.Count > 0)
            {
                //lets clear all the auotnumbers
                var gridRowT = fieldsViewModel.DynamicGridViewModel.GridRecords.First();
                gridRowT.IsSelected = true;
                var buttonT = fieldsViewModel.DynamicGridViewModel.GetButton("RECONFIGUREAUTONUMBER");
                buttonT.Invoke();
                var configureDialogT = autonumbersViewModel.ChildForms.First() as ConfigureAutonumberDialog;
                var configureEntryT = testApplication.GetSubObjectEntryViewModel(configureDialogT);
                Assert.IsNotNull(configureEntryT.GetRecordFieldFieldViewModel(nameof(ConfigureAutonumberRequest.Field)).Value);
                configureEntryT.GetStringFieldFieldViewModel(nameof(ConfigureAutonumberRequest.AutonumberFormat)).Value = null;
                configureEntryT.SaveButtonViewModel.Invoke();
                Assert.IsFalse(autonumbersViewModel.ChildForms.Any());
            }

            var addButton = fieldsViewModel.DynamicGridViewModel.GetButton("ADDNEWAUTONUMBER");
            addButton.Invoke();
            var addDialog = autonumbersViewModel.ChildForms.First() as ConfigureAutonumberDialog;
            var addEntry = testApplication.GetSubObjectEntryViewModel(addDialog);
            var fieldViewModel = addEntry.GetRecordFieldFieldViewModel(nameof(ConfigureAutonumberRequest.Field));
            fieldViewModel.Value = fieldViewModel.ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            addEntry.GetStringFieldFieldViewModel(nameof(ConfigureAutonumberRequest.AutonumberFormat)).Value = "ACC-{SEQNUM:6}";
            addEntry.GetBigIntFieldViewModel(nameof(ConfigureAutonumberRequest.SetSeed)).Value = 1234;
            addEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(autonumbersViewModel.ChildForms.Any());

            Assert.AreEqual(1, fieldsViewModel.DynamicGridViewModel.GridRecords.Count);

            WaitTillTrue(() => CreateAccount().GetStringField(Fields.account_.accountnumber) == "ACC-001234");
            WaitTillTrue(() => CreateAccount().GetStringField(Fields.account_.accountnumber) == "ACC-001235");

            var gridRow = fieldsViewModel.DynamicGridViewModel.GridRecords.First();
            gridRow.IsSelected = true;
            var button = fieldsViewModel.DynamicGridViewModel.GetButton("RECONFIGUREAUTONUMBER");
            button.Invoke();
            var configureDialog = autonumbersViewModel.ChildForms.First() as ConfigureAutonumberDialog;
            var configureEntry = testApplication.GetSubObjectEntryViewModel(configureDialog);
            Assert.IsNotNull(configureEntry.GetRecordFieldFieldViewModel(nameof(ConfigureAutonumberRequest.Field)).Value);
            Assert.AreEqual("ACC-{SEQNUM:6}", configureEntry.GetStringFieldFieldViewModel(nameof(ConfigureAutonumberRequest.AutonumberFormat)).Value);
            configureEntry.GetBigIntFieldViewModel(nameof(ConfigureAutonumberRequest.SetSeed)).Value = 2345;
            configureEntry.SaveButtonViewModel.Invoke();
            Assert.AreEqual(1, fieldsViewModel.DynamicGridViewModel.GridRecords.Count);

            WaitTillTrue(() => CreateAccount().GetStringField(Fields.account_.accountnumber) == "ACC-002345");

            gridRow = fieldsViewModel.DynamicGridViewModel.GridRecords.First();
            gridRow.IsSelected = true;
            button = fieldsViewModel.DynamicGridViewModel.GetButton("RECONFIGUREAUTONUMBER");
            button.Invoke();
            configureDialog = autonumbersViewModel.ChildForms.First() as ConfigureAutonumberDialog;
            configureEntry = testApplication.GetSubObjectEntryViewModel(configureDialog);
            Assert.IsNotNull(configureEntry.GetRecordFieldFieldViewModel(nameof(ConfigureAutonumberRequest.Field)).Value);
            configureEntry.GetStringFieldFieldViewModel(nameof(ConfigureAutonumberRequest.AutonumberFormat)).Value = "ACC-{SEQNUM:5}";
            configureEntry.SaveButtonViewModel.Invoke();
            Assert.AreEqual(1, fieldsViewModel.DynamicGridViewModel.GridRecords.Count);

            WaitTillTrue(() => CreateAccount().GetStringField(Fields.account_.accountnumber) == "ACC-02346");

            gridRow = fieldsViewModel.DynamicGridViewModel.GridRecords.First();
            gridRow.IsSelected = true;
            button = fieldsViewModel.DynamicGridViewModel.GetButton("RECONFIGUREAUTONUMBER");
            button.Invoke();
            configureDialog = autonumbersViewModel.ChildForms.First() as ConfigureAutonumberDialog;
            configureEntry = testApplication.GetSubObjectEntryViewModel(configureDialog);
            Assert.IsNotNull(configureEntry.GetRecordFieldFieldViewModel(nameof(ConfigureAutonumberRequest.Field)).Value);
            Assert.AreEqual("ACC-{SEQNUM:5}", configureEntry.GetStringFieldFieldViewModel(nameof(ConfigureAutonumberRequest.AutonumberFormat)).Value);
            configureEntry.GetStringFieldFieldViewModel(nameof(ConfigureAutonumberRequest.AutonumberFormat)).Value = null;
            configureEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(autonumbersViewModel.ChildForms.Any());

            DeleteMyToday();
        }
    }
}