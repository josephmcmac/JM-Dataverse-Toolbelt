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
                ConsoleApplicationController.UserMessage(GetCommandLineSwitchString());
            }
            else
            {
                var command = args.First();
                if (command == "?" || command.ToLower() == "help")
                {
                    ConsoleApplicationController.WriteToConsole("The Valid Commands For This Application Are\n");
                    ConsoleApplicationController.UserMessage(GetCommandLineSwitchString());
                }
                else
                {
                    var matchingOptions = GetCommandOptions().Where(o => o.CommandName.ToLower() == command.ToLower());
                    if (!matchingOptions.Any())
                    {
                        ConsoleApplicationController.WriteToConsole(string.Format("No Matching Command Found For '{0}'\n", command));
                        ConsoleApplicationController.UserMessage(GetCommandLineSwitchString());
                    }
                    else
                    {
                        var matchingOption = matchingOptions.First();
                        Dictionary<string, string> arguments = ParseCommandLineArguments(args);

                        var commandArgs = matchingOption.GetArgs();
                        foreach (var arg in arguments.Where(a => !StandardCommandArguments.ContainsKey(a.Key)))
                        {
                            if (!commandArgs.ContainsKey(arg.Key))
                            {
                                throw new ArgumentOutOfRangeException(arg.Key, string.Format("'{0}' is not a valid argument name for the command. The valid arguments are {1}", arg.Key, commandArgs.Keys.JoinGrammarAnd()));
                            }
                            commandArgs[arg.Key](arg.Value);
                        }
                        matchingOption.Command();
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

        private static Dictionary<string,string> StandardCommandArguments
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "SettingsFolderName", "The Folder Containing The Saved Settings For The Application" }
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

        public void LoadModulesInExcecutionFolder()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
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
        }

        private class CommandOption
        {
            private ModuleBase value;

            public CommandOption(ModuleBase value)
            {
                this.value = value;
            }
        }
    }
}