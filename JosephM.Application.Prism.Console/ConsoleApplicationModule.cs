using JosephM.Application.Modules;
using JosephM.Application.Prism.Module;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Module.SavedRequests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JosephM.Prism.Infrastructure.Console
{
    [DependantModule(typeof(SavedRequestModule))]
    /// <summary>
    /// This module adds a function to save (and subsequently load) details entered into a service request object
    /// </summary>
    public class ConsoleApplicationModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            AddGenerateBatFunction();
        }

        /// <summary>
        /// This function adds a button the the save setting grid to generate a .bat executable
        /// </summary>
        private void AddGenerateBatFunction()
        {
            var customGridFunction = new CustomGridFunction("GENERATEBAT", "Generate Bat Executable", GenerateBat, (re) => { return true; });
            this.AddCustomGridFunction(customGridFunction, typeof(IAllowSaveAndLoad));
        }

        private void GenerateBat(DynamicGridViewModel grid)
        {
            var selectedRows = grid.SelectedRows;
            if(selectedRows.Count() != 1)
            {
                ApplicationController.UserMessage("Only 1 Row May Be Selected For The Generate Bat Option");
            }
            else
            {
                var selectedRow = selectedRows.First();
                var name = selectedRow.GetStringFieldFieldViewModel(nameof(IAllowSaveAndLoad.Name)).Value;
                var codeBase = Assembly.GetExecutingAssembly().Location;
                var uri = new UriBuilder(codeBase);
                string executableConsoleApp = Uri.UnescapeDataString(uri.Path);

                var arguments = new Dictionary<string, string>
                {
                    { "SettingsFolderName", ApplicationController.ApplicationName },
                    { "Request", name },
                    { "LogPath", "Log" },
                };

                //get the module implementing the request type
                var moduleController = ApplicationController.ResolveType(typeof(ModuleController)) as ModuleController;
                if (moduleController == null)
                    throw new NullReferenceException(typeof(ModuleController).Name);

                var matchingModules = moduleController
                    .GetLoadedModules()
                    .Where(module => module is ICommandLineExecutable && ((ICommandLineExecutable)module).RequestType.AssemblyQualifiedName == grid.RecordType)
                    .Cast<ICommandLineExecutable>()
                    .ToArray();

                if(!matchingModules.Any())
                    throw new NullReferenceException(string.Format("Could Not Find {0} Implementing {1} With {2} {3}", typeof(ModuleBase).Name, typeof(ICommandLineExecutable).Name, nameof(ICommandLineExecutable.RequestType), grid.RecordType));
                if (matchingModules.Count() > 1)
                    throw new NullReferenceException(string.Format("Error Multiple {0} Found Implementing {1} With {2} {3}", typeof(ModuleBase).Name, typeof(ICommandLineExecutable).Name, nameof(ICommandLineExecutable.RequestType), grid.RecordType));

                var executableModule = matchingModules.First();

                var commandLine = string.Format("\"{0}\" \"{1}\" {2}", executableConsoleApp, executableModule.CommandName, string.Join(" ", arguments.Select(kv => string.Format("-\"{0}\" \"{1}\"", kv.Key, kv.Value))));

                var newFileName = ApplicationController.GetSaveFileName(string.Format("{0} {1}.bat", executableModule.CommandName, name), "bat");

                if (newFileName != null)
                {
                    var folder = Path.GetDirectoryName(newFileName);
                    var fileName = Path.GetFileName(newFileName);
                    FileUtility.WriteToFile(folder, fileName, commandLine);
                }
            }
        }
    }
}