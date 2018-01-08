using JosephM.Application.Application;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Console;
using JosephM.Record.Application.Fakes;
using Microsoft.Practices.Unity;
using System;

namespace JosephM.Application.Prism.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var arguments = ConsoleApplication.ParseCommandLineArguments(args);
                var applicationName = arguments.ContainsKey("SettingsFolderName") ? arguments["SettingsFolderName"] : "Unknown Console Context";

                //okay need to create app
                var dependencyResolver = new PrismDependencyContainer(new UnityContainer());
                var controller = new ConsoleApplicationController(applicationName, dependencyResolver);
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
                throw;
            }
        }
    }
}
