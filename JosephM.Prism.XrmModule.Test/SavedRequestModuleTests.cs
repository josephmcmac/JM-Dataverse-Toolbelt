using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Module.SavedRequests;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JosephM.Prism.XrmModule.Test
{
    [TestClass]
    public class SavedRequestModuleTests : XrmModuleTest
    {
        /// <summary>
        /// Scripts through several scenarios for the saved requests module
        /// </summary>
        [TestMethod]
        public void SavedRequestModuleTest()
        {
            var testApplication = CreateAndLoadTestApplication<SavedRequestModule>();
            testApplication.AddModule<TestSavedRequestModule>();

            //set to no previously saved ones
            var savedRequests = new SavedSettings();
            var settingsManager = testApplication.Controller.ResolveType<ISettingsManager>();
            settingsManager.SaveSettingsObject(savedRequests, typeof(TestSavedRequestDialogRequest));

            //navigate to the request and populate the string field
            var entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();
            var request = new TestSavedRequestDialogRequest()
            {
                SomeArbitraryString = nameof(TestSavedRequestDialogRequest.SomeArbitraryString)
            };
            testApplication.EnterObject(request, entryForm);

            //trigger save request
            var saveRequestButton = entryForm.GetButton("SAVEREQUEST");
            saveRequestButton.Invoke();

            //enter and save details including autoload
            var saveRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);
            var detailsEntered = new SaveAndLoadFields()
            {
                Name = "TestName",
                Autoload = true
            };
            testApplication.EnterAndSaveObject(detailsEntered, saveRequestForm);
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);

            //reopen app/dialog and veirfy autoloads
            testApplication = CreateAndLoadTestApplication<SavedRequestModule>();
            testApplication.AddModule<TestSavedRequestModule>();
            entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();

            Assert.AreEqual(nameof(TestSavedRequestDialogRequest.SomeArbitraryString), entryForm.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.SomeArbitraryString)).Value);

            //clear the loaded string value
            entryForm.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.SomeArbitraryString)).Value = "Something Else";

            //invoke load request dialog
            var loadRequestButton = entryForm.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();
            var loadRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);
            //select and load the saved request
            var subGrid = loadRequestForm.GetSubGridViewModel(nameof(SavedSettings.SavedRequests));
            Assert.IsTrue(subGrid.GridRecords.Count() == 1);
            subGrid.GridRecords.First().IsSelected = true;
            var loadButton = subGrid.DynamicGridViewModel.GetButton("LOADREQUEST");
            loadButton.Invoke();
            //verify loads
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);
            Assert.AreEqual(nameof(TestSavedRequestDialogRequest.SomeArbitraryString), entryForm.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.SomeArbitraryString)).Value);

            //verify if delete on the load form

            //invoke load form
            loadRequestButton = entryForm.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();
            loadRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);
            //delete the saved request in the grid
            subGrid = loadRequestForm.GetSubGridViewModel(nameof(SavedSettings.SavedRequests));
            subGrid.DynamicGridViewModel.DeleteRow(subGrid.GridRecords.First());
            loadRequestForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);

            //verify no longer a saved request resolved by the settings manager
            settingsManager = testApplication.Controller.ResolveType<ISettingsManager>();
            savedRequests = settingsManager.Resolve<SavedSettings>(typeof(TestSavedRequestDialogRequest));
            Assert.IsFalse(savedRequests.SavedRequests.Any());

            //verify does not throw fatal error if lookup referenced is deleted in the saved requests

            //create a saved request for a deleted solution
            var solution = ReCreateTestSolution();
            request = new TestSavedRequestDialogRequest()
            {
                Name = "Foo",
                Autoload = true,
                SomeArbitraryString = nameof(TestSavedRequestDialogRequest.SomeArbitraryString),
                XrmLookupField = solution.ToLookup()
            };
            savedRequests = new SavedSettings()
            {
                SavedRequests = new[] { request }
            };
            settingsManager.SaveSettingsObject(savedRequests, typeof(TestSavedRequestDialogRequest));
            XrmRecordService.Delete(solution);

            //verify a user message is thrown when the autoload fires
            testApplication = CreateAndLoadTestApplication<SavedRequestModule>();
            testApplication.AddModule<TestSavedRequestModule>();

            try
            {
                entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex is FakeUserMessageException);
            }

            //verify saved requests load when a saved request has a deleted solution

            //create a saved request with a solution
            solution = ReCreateTestSolution();
            request = new TestSavedRequestDialogRequest()
            {
                Name = "Foo",
                SomeArbitraryString = nameof(TestSavedRequestDialogRequest.SomeArbitraryString),
                XrmLookupField = solution.ToLookup()
            };
            savedRequests = new SavedSettings()
            {
                SavedRequests = new[] { request }
            };
            settingsManager.SaveSettingsObject(savedRequests, typeof(TestSavedRequestDialogRequest));

            //load the dialog
            testApplication = CreateAndLoadTestApplication<SavedRequestModule>();
            testApplication.AddModule<TestSavedRequestModule>();
            entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();

            //delete the solution
            XrmRecordService.Delete(solution);

            //invoke the load request form
            loadRequestButton = entryForm.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();

            //veirfy a user message is thrown if try to load the one with the deleted solution
            loadRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);
            subGrid = loadRequestForm.GetSubGridViewModel(nameof(SavedSettings.SavedRequests));
            Assert.IsTrue(subGrid.GridRecords.Count() == 1);
            subGrid.GridRecords.First().IsSelected = true;
            loadButton = subGrid.DynamicGridViewModel.GetButton("LOADREQUEST");

            try
            {
                loadButton.Invoke();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is FakeUserMessageException);
            }
        }

        /// <summary>
        /// heck validations are not run on the saved requests
        /// </summary>
        [TestMethod]
        public void SavedRequestModuleTestFieldsNotValidatedInSavedRequests()
        {
            //only the name should be required on a saved request
            var testApplication = CreateAndLoadTestApplication<SavedRequestModule>();
            testApplication.AddModule<TestSavedRequestModule>();

            //set to no previously saved ones
            var savedRequests = new SavedSettings()
            {
                SavedRequests = new[]
                {
                    new TestSavedRequestDialogRequest()
                    {
                         DoNotValidateString = nameof(TestSavedRequestDialogRequest.DoNotValidateString),
                         Name = nameof(TestSavedRequestDialogRequest.DoNotValidateString)
                    }
                }
            };
            var settingsManager = testApplication.Controller.ResolveType<ISettingsManager>();
            settingsManager.SaveSettingsObject(savedRequests, typeof(TestSavedRequestDialogRequest));

            var entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();
            entryForm.GetButton("LOADREQUEST").Invoke();

            var savedRequestsForm = testApplication.GetSubObjectEntryViewModel(entryForm);

            var saveRequestsGrid = savedRequestsForm.GetSubGridViewModel(nameof(SavedSettings.SavedRequests));
            Assert.AreEqual(1, saveRequestsGrid.GridRecords.Count());

            var firstgridRecord = saveRequestsGrid.GridRecords.First();
            var nameField = firstgridRecord.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.Name));
            var doNotValidateField = firstgridRecord.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.DoNotValidateString));

            nameField.Value = null;
            doNotValidateField.Value = null;

            Assert.IsFalse(savedRequestsForm.Validate());

            nameField.Value = "Arbitrary Name";
            Assert.IsTrue(savedRequestsForm.Validate());
        }

        [AllowSaveAndLoad]
        public class TestSavedRequestDialogRequest : ServiceRequestBase
        {
            public string SomeArbitraryString { get; set; }

            [RequiredProperty]
            public string DoNotValidateString { get; set; }

            [ReferencedType(Entities.solution)]
            [LookupCondition(Fields.solution_.ismanaged, false)]
            [LookupCondition(Fields.solution_.isvisible, true)]
            [LookupFieldCascade(nameof(XrmLookupFieldCascaded), Fields.solution_.version)]
            [UsePicklist(Fields.solution_.uniquename)]
            public Lookup XrmLookupField { get; set; }

            public string XrmLookupFieldCascaded { get; set; }
        }

        [DependantModule(typeof(SavedXrmConnectionsModule))]
        public class TestSavedRequestModule : ServiceRequestModule<TestSavedRequestDialog, TestSavedRequestDialogService, TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
        {
        }

        public class TestSavedRequestDialog : ServiceRequestDialog<TestSavedRequestDialogService, TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
        {
            public TestSavedRequestDialog(IDialogController controller, XrmRecordService lookupService)
                : base(new TestSavedRequestDialogService(), controller, lookupService)
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