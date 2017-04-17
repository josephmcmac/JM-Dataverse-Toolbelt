using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;
using JosephM.Core.Service;
using JosephM.Core.Attributes;
using JosephM.Core.Log;
using System;

namespace JosephM.Prism.Infrastructure.Test
{
    [TestClass]
    public class SavedRequestDialogTest
    {
        [TestMethod]
        public void SavedRequestDialogOnlyValidateNameTest()
        {
            //okay this dialog is scripted for all the module tests themselves
            //but does not include verifying only validating that the saved request has a name
            //and the request fields do not need validating when saved

            //that is a saved request does not require validating apart from the fields in the base class (name)

            var testApplication = TestApplication.CreateTestApplication();
            testApplication.Controller.RegisterType<IDialogController, FakeDialogController>();
            testApplication.AddModule<TestSavedRequestModule>();

            //okay initially save a new request
            var settings = new SavedSettings()
            {
                SavedRequests = new[] { new TestSavedRequestDialogRequest() }
            };
            var settingsManager = testApplication.Controller.ResolveType<PrismSettingsManager>();
            settingsManager.SaveSettingsObject(settings, typeof(TestSavedRequestDialogRequest));

            //navigate to the saved requests
            var entryForm = testApplication.LoadSavedRequestsEntryForm(typeof(TestSavedRequestDialogRequest));
            var subGrid = entryForm.SubGrids.First(g => g.FieldName == "SavedRequests");
            var savedRequest = subGrid.GridRecords.First();

            //set the fields empty
            //and verify invalid
            var doNotValidateField = savedRequest.GetStringFieldFieldViewModel("DoNotValidateString");
            doNotValidateField.Value = null;
            var nameField = savedRequest.GetStringFieldFieldViewModel("Name");
            nameField.Value = null;
            Assert.IsFalse(entryForm.Validate());

            //populate only the name and verify now valid
            nameField.Value = "Name";
            Assert.IsTrue(entryForm.Validate());
        }

        public class TestSavedRequestDialogRequest : ServiceRequestBase
        {
            [RequiredProperty]
            public string DoNotValidateString { get; set; }
        }

        public class TestSavedRequestModule : ServiceRequestModule<TestSavedRequestDialog, TestSavedRequestDialogService, TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
        {
        }

        public class TestSavedRequestDialog : ServiceRequestDialog<TestSavedRequestDialogService, TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
        {
            public TestSavedRequestDialog(IDialogController controller)
                : base(new TestSavedRequestDialogService(), controller)
            {
                    
            }
        }

        public class TestSavedRequestDialogService : ServiceBase<TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
        {
            public override void ExecuteExtention(TestSavedRequestDialogRequest request, TestSavedRequestDialogResponse response, LogController controller)
            {
                return;
            }
        }

        public class TestSavedRequestDialogResponseItem : ServiceResponseItem
        {
        }

        public class TestSavedRequestDialogResponse : ServiceResponseBase<TestSavedRequestDialogResponseItem>
        {
        }
    }
}