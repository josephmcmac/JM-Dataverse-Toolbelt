using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Console;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Module.SavedRequests;
using JosephM.Prism.Infrastructure.Test;
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
    public class ConsoleApplicationModuleTests : SavedRequestModuleTests
    {
        /// <summary>
        /// Scripts through generation of a .bat for executing a dialog in the command line
        /// </summary>
        [TestMethod]
        public void ConsoleApplicationModuleTest()
        {
            var testApplication = CreateThisApp();

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
            testApplication = CreateThisApp();
            entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();

            Assert.AreEqual(nameof(TestSavedRequestDialogRequest.SomeArbitraryString), entryForm.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.SomeArbitraryString)).Value);

            //clear the loaded string value
            entryForm.GetStringFieldFieldViewModel(nameof(TestSavedRequestDialogRequest.SomeArbitraryString)).Value = "Something Else";

            //invoke load request dialog
            var loadRequestButton = entryForm.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();
            var loadRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);
            //select and load the saved request
            var subGrid = loadRequestForm.GetEnumerableFieldViewModel(nameof(SavedSettings.SavedRequests));
            Assert.IsTrue(subGrid.GridRecords.Count() == 1);
            subGrid.GridRecords.First().IsSelected = true;

            var generateBatButton = subGrid.DynamicGridViewModel.GetButton("GENERATEBAT");
            generateBatButton.Invoke();
        }

        private TestApplication CreateThisApp()
        {
            var testApplication = CreateAndLoadTestApplication<SavedRequestModule>();
            testApplication.AddModule<TestSavedRequestModule>();
            testApplication.AddModule<ConsoleApplicationModule>();
            return testApplication;
        }
    }
}