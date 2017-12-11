using JosephM.Application.Application;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Console;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Record.Application.Fakes;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace JosephM.Prism.XrmModule.Test
{
    [TestClass]
    public class ConsoleApplicationModuleTests : ModuleTest
    {
        /// <summary>
        /// Scripts through generation of a .bat for executing a dialog in the command line
        /// Then uses that .bat contents to run the console application
        /// </summary>
        [TestMethod]
        public void ConsoleApplicationModuleGenerateBatAndExecuteTest()
        {
            //this is done in the load.edit saved request form
            //a button is added the the saved request grid whiuch generates the .bat

            //so we need to save a request
            //then navigate to the save requests and trigger the generate bat button

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
                Name = "TestName"
            };
            testApplication.EnterAndSaveObject(detailsEntered, saveRequestForm);
            Assert.IsFalse(entryForm.ChildForms.Any());
            Assert.IsFalse(entryForm.LoadingViewModel.IsLoading);

            //reopen app/dialog
            testApplication = CreateThisApp();
            entryForm = testApplication.NavigateToDialogModuleEntryForm<TestSavedRequestModule, TestSavedRequestDialog>();

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

            var args = CommandLineToArgs(batContent)
                .Skip(1)
                .ToArray();

            var arguments = ConsoleApplication.ParseCommandLineArguments(args);
            var applicationName = arguments.ContainsKey("SettingsFolderName") ? arguments["SettingsFolderName"] : "Unknown Console Context";

            //okay need to create app
            var dependencyResolver = new PrismDependencyContainer(new UnityContainer());
            var controller = new ConsoleApplicationController(applicationName, dependencyResolver);
            settingsManager = new PrismSettingsManager(controller);
            var applicationOptions = new ApplicationOptionsViewModel(controller);
            var app = new ConsoleApplication(controller, applicationOptions, settingsManager);
            //load modules in folder path
            app.LoadModulesInExcecutionFolder();
            //run app
            app.Run(args);
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
    [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        private TestApplication CreateThisApp()
        {
            var testApplication = CreateAndLoadTestApplication<TestSavedRequestModule>();
            testApplication.AddModule<ConsoleApplicationModule>();
            return testApplication;
        }

        [AllowSaveAndLoad]
        public class TestSavedRequestDialogRequest : ServiceRequestBase
        {
            public string SomeArbitraryString { get; set; }
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