using JosephM.Application.Application;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Console;
using JosephM.Record.Application.Fakes;
using Microsoft.Practices.Unity;
using System;
using System.Threading;

namespace JosephM.Application.Prism.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var arguments = ConsoleApplication.ParseCommandLineArguments(args);
                if (!arguments.ContainsKey("SettingsFolderName"))
                    throw new NullReferenceException(string.Format("The {0} Argument Is Missing. This Is Required For Loading The Settings", "SettingsFolderName"));

                //okay need to create app
                var dependencyResolver = new PrismDependencyContainer(new UnityContainer());
                var controller = new ConsoleApplicationController(arguments["SettingsFolderName"], dependencyResolver);
                var settingsManager = new PrismSettingsManager(controller);
                var applicationOptions = new ApplicationOptionsViewModel(controller);
                var app = new ConsoleApplication(controller, applicationOptions, settingsManager);
                //load modules in folder path
                app.LoadModulesInExcecutionFolder();
                //run app
                app.Run(args);
                System.Console.WriteLine("Process Completed. Finishing In 10 Seconds");
                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.DisplayString());
                System.Console.WriteLine("Press Any Key To Close");
                System.Console.ReadKey();
            }
        }
    }
}
