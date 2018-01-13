#region

using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.Options;
using JosephM.Application.Prism.Module;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


#endregion

namespace JosephM.Prism.Infrastructure.Console
{
    /// <summary>
    ///     Class For A Prism Application Instance To Load Modules The Run
    /// </summary>
    public class ConsoleApplication : ApplicationBase
    {
        private ConsoleApplicationController ConsoleApplicationController { get; set; }

        public ConsoleApplication(ConsoleApplicationController applicationController, IApplicationOptions applicationOptions, ISettingsManager settingsManager)
            : base(applicationController, applicationOptions, settingsManager)
        {
            ConsoleApplicationController = applicationController;
            Controller.RegisterType<IDialogController, ConsoleDialogController>();
        }

        public void Run(string[] args)
        {
            //need to run as per the arguments or display the console options
            if (args == null || !args.Any())
            {
                ConsoleApplicationController.WriteToConsole("The Valid Commands For This Application Are\n");
                ConsoleApplicationController.WriteToConsole(GetCommandLineSwitchString());
            }
            else
            {
                var command = args.First();
                if (command == "?" || command.ToLower() == "help")
                {
                    ConsoleApplicationController.WriteToConsole("The Valid Commands For This Application Are\n");
                    ConsoleApplicationController.WriteToConsole(GetCommandLineSwitchString());
                }
                else
                {
                    var matchingOptions = GetCommandOptions().Where(o => o.CommandName.ToLower() == command.ToLower());
                    if (!matchingOptions.Any())
                    {
                        ConsoleApplicationController.WriteToConsole(string.Format("No Matching Command Found For '{0}'\n", command));
                        ConsoleApplicationController.WriteToConsole(GetCommandLineSwitchString());
                    }
                    else
                    {
                        var matchingOption = matchingOptions.First();
                        var arguments = ParseCommandLineArguments(args);
                        if (!arguments.Any() || arguments.First().Key == "?" || arguments.First().Key == "help")
                        {
                            ConsoleApplicationController.WriteToConsole("The Commands Arguments Are");
                            ConsoleApplicationController.WriteToConsole(GetCommandLineArgumentsString(command));
                        }
                        else
                        {
                            ConsoleApplicationController.WriteToConsole(string.Format("Loading {0} Command", command));
                            var commandArgs = matchingOption.GetArgs();

                            foreach (var arg in arguments.Where(a => !StandardCommandArguments.Any(sca => sca.CommandName == a.Key)))
                            {
                                var matchingCommand = commandArgs.Where(c => c.CommandName == arg.Key);
                                if (!matchingCommand.Any())
                                {
                                    throw new ArgumentOutOfRangeException(arg.Key, string.Format("'{0}' is not a valid argument name for the command. The valid arguments are\n{1}", arg.Key, GetCommandLineArgumentsString(command)));
                                }
                                matchingCommand.First().LoadAction(arg.Value);
                            }
                            //this needs to run after others which load the log path
                            LoadActiveXrmConnection(arguments);

                            ConsoleApplicationController.WriteToConsole(string.Format("Starting {0} Process", command));
                            matchingOption.Command();
                        }
                    }
                }
            }
        }

        private void LoadActiveXrmConnection(Dictionary<string, string> arguments)
        {
            //if we have an active connection in the command line then we need to load it
            if (arguments.ContainsKey("ActiveXrmConnection"))
            {
                var savedConnection = arguments["ActiveXrmConnection"];
                if (string.IsNullOrWhiteSpace(savedConnection))
                    throw new NullReferenceException("Command Argument ActiveXrmConnection Is Empty");

                ConsoleApplicationController.WriteToConsole($"Loading ActiveXrmConnection: '{savedConnection}'");
                //var settingsManager = Controller.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
                //if (settingsManager == null)
                //    throw new NullReferenceException(nameof(settingsManager));

                //var savedConnections = settingsManager
                //    .Resolve<SavedSettings>(typeof(IXrmRecordConfiguration))
                //    .SavedRequests
                //    .Cast<XrmRecordConfiguration>()
                //    .ToArray();
                var savedConnections = Controller.ResolveType(typeof(ISavedXrmConnections)) as ISavedXrmConnections;
                if (savedConnections == null)
                    throw new NullReferenceException(nameof(savedConnections));

                var matchingConnections = savedConnections
                    .Connections
                    .Where(c => c.Name == savedConnection)
                    .ToArray();
                if (!matchingConnections.Any())
                    throw new NullReferenceException("No saved Connection Found With Name Of " + savedConnection);

                var setConnectionActive = matchingConnections.First();
                var validate = setConnectionActive.Validate();
                if (!validate.IsValid)
                    throw new Exception($"Error Loading ActiveXrmConnection '{savedConnection}': {validate.GetErrorString()}");

                XrmConnectionModule.RefreshXrmServices(matchingConnections.First(), Controller);
            }
        }

        public static Dictionary<string, string> ParseCommandLineArguments(string[] args)
        {
            var arguments = new Dictionary<string, string>();
            for (var i = 1; i < args.Length; i++)
            {
                if (i % 2 == 1)
                {
                    string key = RemoveKeyPrefix(args[i]);
                    if (arguments.ContainsKey(key))
                    {
                        throw new Exception(string.Format("Error Duplicate Command Found '{0}'", key));
                    }
                    arguments.Add(key, null);
                }
                else
                {
                    arguments[RemoveKeyPrefix(args[i - 1])] = args[i];
                }
            }

            return arguments;
        }

        public string GetCommandLineArgumentsString(string command)
        {
            var matchingOptions = GetCommandOptions().Where(o => o.CommandName.ToLower() == command.ToLower());
            if (!matchingOptions.Any())
            {
                throw new NullReferenceException(string.Format("No Matching Command Found For '{0}'", command));
            }
            else
            {
                var matchingOption = matchingOptions.First();

                var commandSwitchesString = new StringBuilder();
                foreach (var option in matchingOption.GetArgs().Union(StandardCommandArguments))
                {
                    commandSwitchesString.AppendLine(string.Format("\t{0} : {1}", option.CommandName, option.Description));
                }
                return commandSwitchesString.ToString();
            }
        }

        private static string RemoveKeyPrefix(string key)
        {
            foreach (var item in ValidArgPrefixes)
            {
                while (key.StartsWith(item))
                {
                    key = key.Substring(item.Length);
                }
            }

            return key;
        }

        private string GetCommandLineSwitchString()
        {
            var commandSwitchesString = new StringBuilder();
            foreach (var option in GetCommandOptions())
            {
                commandSwitchesString.AppendLine(string.Format("\t{0}{1} : {2}", ValidArgPrefixes.First(), option.CommandName, option.Description));
            }
            return commandSwitchesString.ToString();
        }

        private IEnumerable<ICommandLineExecutable> GetCommandOptions()
        {
            return Modules
                .Where(kv => kv.Value is ICommandLineExecutable)
                .Select(kv => kv.Value)
                .Cast<ICommandLineExecutable>()
                .ToArray();
        }

        private static IEnumerable<CommandLineArgument> StandardCommandArguments
        {
            get
            {
                return new CommandLineArgument[]
                {
                    new CommandLineArgument("SettingsFolderName", "The Folder Containing The Saved Settings For The Application"),
                    new CommandLineArgument("ActiveXrmConnection", "Saved Xrm Connection To Load If The Dialog Uses The Current Active Xrm Connection")
                };
            }
        }

        private static string[] ValidArgPrefixes
        {
            get
            {
                return new[] { "-", "/", @"\" };
            }
        }

        public void LoadModulesInExecutionFolder()
        {
            ConsoleApplicationController.WriteToConsole("Loading Modules");
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                var loadIt = Assembly.LoadFile(dll);
                foreach (var type in loadIt.GetTypes())
                {
                    if (type.IsTypeOf(typeof(ModuleBase)) && !type.IsGenericType && !type.IsAbstract)
                    {
                        AddModule(type);
                    }
                }
            }

            var commands = GetCommandOptions();
            if (!commands.Any())
                throw new NullReferenceException(string.Format("No {0} Implementing {1} Was Found In The Execution Folder '{2}'", nameof(ModuleBase), nameof(ICommandLineExecutable), codeBase));
        }
    }
}