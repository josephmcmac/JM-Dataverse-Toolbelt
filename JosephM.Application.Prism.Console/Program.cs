using JosephM.Application.Application;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Prism.Infrastructure.Console;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Plugins;
using Microsoft.Practices.Unity;
using System;
using System.Linq;

namespace JosephM.Application.Prism.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = ConsoleApplication.ParseCommandLineArguments(args);
            var applicationName = arguments.ContainsKey("SettingsFolderName") ? arguments["SettingsFolderName"] : "Unknown Console Context";

            //okay need to create app
            var dependencyResolver = new PrismDependencyContainer(new UnityContainer());
            var controller = new ConsoleApplicationController(applicationName, dependencyResolver);
            try
            {
                var settingsManager = new PrismSettingsManager(controller);
                var applicationOptions = new ApplicationOptionsViewModel(controller);
                var app = new ConsoleApplication(controller, applicationOptions, settingsManager);
                //load modules in folder path
                app.LoadModulesInExecutionFolder();
                //run app
                app.Run(args);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.DisplayString());
                System.Console.WriteLine("Fatal Error");
                var logFile = new LogFileUserInterface(controller.LogPath, controller.ApplicationName, true, 50);
                logFile.LogMessage($"Fatal Error During Console Execution: {ex.XrmDisplayString()}");
                throw;
            }
        }
    }
}
