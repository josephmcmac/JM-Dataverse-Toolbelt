using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Test;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Core.AppConfig;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace JosephM.Application.Desktop.Console.Test
{
    [TestClass]
    public class ConsoleApplicationModuleWithXrmConnectionTests : XrmModuleTest
    {
        /// <summary>
        /// Variation of ConsoleApplicationModuleTests where the module's dialog uses the active xrm connection in it's service constructor
        /// and therefore adds it to the command line arguments for loading as the active connection in the console
        /// </summary>
        [TestMethod]
        public void ConsoleApplicationModuleWithXrmConnectionGenerateBatAndExecuteTest()
        {
            //this is done in the load.edit saved request form
            //a button is added the the saved request grid whiuch generates the .bat

            //so we need to save a request
            //then navigate to the save requests and trigger the generate bat button

            var testApplication = CreateThisApp();

            //set to no previously saved ones
            var savedRequests = new SavedSettings();
            var settingsManager = testApplication.Controller.ResolveType<ISettingsManager>();
            settingsManager.SaveSettingsObject(savedRequests, typeof(TestSavedRequestWithXrmConnectionDialogRequest));

            //navigate to the request and populate the string field
            var entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestWithXrmConnectionModule, TestSavedRequestWithXrmConnectionDialog>();
            var request = new TestSavedRequestWithXrmConnectionDialogRequest()
            {
                SomeArbitraryString = nameof(TestSavedRequestWithXrmConnectionDialogRequest.SomeArbitraryString)
            };
            testApplication.EnterObject(request, entryForm);

            //trigger save request
            var saveRequestButton = entryForm.GetButton("SAVEREQUEST");
            saveRequestButton.Invoke();

            //enter and save details including autoload
            var saveRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);
            var detailsEntered = new SaveAndLoadFields()
            {
                Name = "TestName"
            };
            testApplication.EnterAndSaveObject(detailsEntered, saveRequestForm);
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);

            //reopen app/dialog
            testApplication = CreateThisApp();
            entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestWithXrmConnectionModule, TestSavedRequestWithXrmConnectionDialog>();

            //invoke load request dialog
            var loadRequestButton = entryForm.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();
            var loadRequestForm = testApplication.GetSubObjectEntryViewModel(entryForm);

            //verify there is a saved request and trigger the generate bat button
            var subGrid = loadRequestForm.GetEnumerableFieldViewModel(nameof(SavedSettings.SavedRequests));
            Assert.IsTrue(subGrid.GridRecords.Count() == 1);
            subGrid.GridRecords.First().IsSelected = true;

            var generateBatButton = subGrid.DynamicGridViewModel.GetButton("GENERATEBAT");
            generateBatButton.Invoke();

            var testFiles = FileUtility.GetFiles(TestingFolder);
            Assert.AreEqual(1, testFiles.Count());
            Assert.IsTrue(testFiles.First().EndsWith(".bat"));

            var batContent = File.ReadAllText(testFiles.First());

            var args = ConsoleTestUtility.CommandLineToArgs(batContent)
                .Skip(1)
                .ToArray();

            var arguments = ConsoleApplication.ParseCommandLineArguments(args);
            var applicationName = arguments.ContainsKey("SettingsFolderName") ? arguments["SettingsFolderName"] : "Unknown Console Context";

            //okay need to create app
            var dependencyResolver = new DependencyContainer();
            var controller = new ConsoleApplicationController(applicationName, dependencyResolver);
            settingsManager = new DesktopSettingsManager(controller);
            var applicationOptions = new ApplicationOptionsViewModel(controller);
            var app = new ConsoleApplication(controller, applicationOptions, settingsManager);
            //load modules in folder path
            app.LoadModulesInExecutionFolder();

            //for this we will register saved connections in the console
            //in reality they would have been created on disk by the app and loaded by the module\

            //this was just debugging an invalid connection
            //var c1 = GetXrmRecordConfiguration();
            //var c2 = GetSavedXrmRecordConfiguration();
            //c1.Password = new Password("Nope", false, true);
            //c2.Password = new Password("Nope", false, true);
            //XrmConnectionModule.RefreshXrmServices(c1, app.Controller);
            //app.Controller.RegisterInstance<ISavedXrmConnections>(new SavedXrmConnections
            //{
            //    Connections = new[] { c2 }
            //});

            SavedXrmConnectionsModule.RefreshXrmServices(GetXrmRecordConfiguration(), app.Controller);
            app.Controller.RegisterInstance<ISavedXrmConnections>(new SavedXrmConnections
            {
                Connections = new[] { GetSavedXrmRecordConfiguration() }
            });

            //run app
            app.Run(args);
        }


        private TestApplication CreateThisApp()
        {
            var testApplication = CreateAndLoadTestApplication<TestSavedRequestWithXrmConnectionModule>();
            testApplication.AddModule<ConsoleApplicationModule>();
            return testApplication;
        }
    }
}