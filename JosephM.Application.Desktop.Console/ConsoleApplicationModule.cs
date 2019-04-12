using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.CommandLine;
using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JosephM.Application.Desktop.Console
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
            var customGridFunction = new CustomGridFunction("GENERATEBAT", "Create Bat Exe", GenerateBat, (re) => { return true; });
            this.AddCustomGridFunction(customGridFunction, typeof(IAllowSaveAndLoad));
        }

        private void GenerateBat(DynamicGridViewModel grid)
        {
            try
            {
                var selectedRows = grid.SelectedRows;
                if (selectedRows.Count() != 1)
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

                    if (!matchingModules.Any())
                        throw new NullReferenceException(string.Format("Could Not Find {0} Implementing {1} With {2} {3}", typeof(ModuleBase).Name, typeof(ICommandLineExecutable).Name, nameof(ICommandLineExecutable.RequestType), grid.RecordType));
                    if (matchingModules.Count() > 1)
                        throw new NullReferenceException(string.Format("Error Multiple {0} Found Implementing {1} With {2} {3}", typeof(ModuleBase).Name, typeof(ICommandLineExecutable).Name, nameof(ICommandLineExecutable.RequestType), grid.RecordType));

                    var executableModule = matchingModules.First();

                    //okay so since some of the dialogs use the active xrm connection
                    //then we should use that specific connection in the generated bat
                    if (RequiresActiveConnection(executableModule.GetType()))
                    {
                        var savedConnection = Controller.Container.ResolveType(typeof(IXrmRecordConfiguration)) as XrmRecordConfiguration;
                        if (savedConnection == null)
                            throw new NullReferenceException("The Active Connection Must Be A Saved Connection");

                        arguments.Add("ActiveXrmConnection", savedConnection.Name);
                    }

                    ApplicationController.LogEvent("Generate Bat", new Dictionary<string, string> { { "Type", executableModule.RequestType.Name }, { "Is Completed Event", true.ToString() } });

                    //var blah = module.BaseType.GenericTypeArguments[1].GetConstructors().First().GetParameters().Any(p => p.ParameterType == typeof(XrmRecordService));


                    var commandLine = string.Format("\"{0}\" \"{1}\" {2}", executableConsoleApp, executableModule.CommandName, string.Join(" ", arguments.Select(kv => string.Format("-\"{0}\" \"{1}\"", kv.Key, kv.Value))));

                    var newFileName = ApplicationController.GetSaveFileName(string.Format("{0} {1}.bat", executableModule.CommandName, name), "bat");

                    if (newFileName != null)
                    {
                        var folder = Path.GetDirectoryName(newFileName);
                        var fileName = Path.GetFileName(newFileName);
                        FileUtility.WriteToFile(folder, fileName, commandLine);
                        ApplicationController.StartProcess("explorer.exe", "/select, \"" + Path.Combine(folder, fileName) + "\"");
                    }
                }
            }
            catch(Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }


        private static bool RequiresActiveConnection(Type moduleType)
        {
            //okay just a hack to determine if the module uses the active connection
            //basically get the service which the module loads
            //check if its constructor takes a XrmRecordService argument
            //if it does then we will add it to the command line and load it when the console application runs
            while (moduleType != null)
            {
                var genericTypeArguments = moduleType.GenericTypeArguments;
                foreach (var item in moduleType.GenericTypeArguments)
                {
                    if (IsSubclassOfRawGeneric(typeof(ServiceBase<,,>), item))
                    {
                        if (item.GetConstructors().First().GetParameters().Any(p => p.ParameterType == typeof(XrmRecordService)))
                        {
                            return true;
                        }
                    }
                }
                moduleType = moduleType.BaseType;
            }
            return false;
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}