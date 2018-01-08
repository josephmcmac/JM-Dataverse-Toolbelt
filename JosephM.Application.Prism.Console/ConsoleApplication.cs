#region

using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.Options;
using JosephM.Application.Prism.Module;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
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
                            ConsoleApplicationController.WriteToConsole(string.Format("Starting {0} Process", command));
                            matchingOption.Command();
                        }
                    }
                }
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
                    new CommandLineArgument("SettingsFolderName", "The Folder Containing The Saved Settings For The Application")
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