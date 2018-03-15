using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$.Core
{
    public class ConsoleSettings
    {
        private static string[] ValidArgPrefixes
        {
            get
            {
                return new[] { "-", "/", @"\" };
            }
        }

        public IEnumerable<SettingFile> SettingsFiles { get; set; }
        public string RootFolder { get; set; }

        public ConsoleSettings(IEnumerable<SettingFile> settingsFiles, string rootFolder = null)
        {
            SettingsFiles = settingsFiles;
            RootFolder = rootFolder;
        }

        public T Resolve<T>(string name = null)
        {
            var matchingOnes = name == null
                ? SettingsFiles.Where(s => s.Type == typeof(T))
                : SettingsFiles.Where(s => s.Name == name);
            if (!matchingOnes.Any())
                throw new NullReferenceException(string.Format("None of the settings matched for {0}", name == null ? typeof(T).Name : name));
            return (T) matchingOnes.First().ResolveObject(RootFolder);
        }

        public bool ConsoleArgs(IEnumerable<string> args)
        {
            if (args == null || !args.Any())
                return false;

            if (args.Count() > 1)
            {
                throw new ArgumentOutOfRangeException("args", string.Format("Error there is more than 1 switch in execution string. Only 1 is allowed"));
            }
            var matched = false;
            var argument = args.First();
            foreach(var item in ValidArgPrefixes)
            {
                while(argument.StartsWith(item))
                {
                    argument = argument.Substring(item.Length);
                }
            }
            if(argument == "?" || argument.ToLower() == "help")
            {
                matched = true;
                Console.WriteLine();
                Console.WriteLine(GetCommandLineSwitchString());
            }
            foreach (var setting in SettingsFiles)
            {
                if (setting.SwitchName.ToLower() == argument.ToLower())
                {
                    var settingsObject = setting.EnterObject();
                    var serialise = JsonHelper.ObjectToJsonString(settingsObject);
                    if (File.Exists(setting.FileName))
                        File.Delete(setting.FileName);
                    File.WriteAllText(setting.FileName, serialise);
                    matched = true;
                }
            }
            if (!matched)
            {
                throw new ArgumentOutOfRangeException("args", string.Format("Unknown command in execution string. {0}. The valid commands are:\n{1}", args.First(), GetCommandLineSwitchString()));
            }
            return true;
        }

        private string GetCommandLineSwitchString()
        {
            var commandSwitchesString = new StringBuilder();
            foreach (var item in SettingsFiles)
            {
                commandSwitchesString.AppendLine(string.Format("\t{0}{1} : Generates A New File Containing the {1} Details", ValidArgPrefixes.First(), item.SwitchName));
            }
            return commandSwitchesString.ToString();
        }

        public class SettingFile
        {
            public Type Type { get; set; }

            public string Name { get; set; }

            public string SwitchName { get { return Name == null ? Type.Name : Name; } }

            public SettingFile(Type type, string name = null)
            {
                Type = type;
                Name = name;
            }

            public object EnterObject()
            {
                var newObject = Type.CreateFromParameterlessConstructor();
                var props = Type.GetWritableProperties();
                foreach (var prop in props)
                {
                    Console.Write("Please enter the " + prop.Name + ":");
                    var value = Console.ReadLine();
                    newObject.SetPropertyByString(prop.Name, value);
                }
                return newObject;
            }

            public object ResolveObject(string rootFolder = null)
            {
                var qualifiedFileName = string.IsNullOrWhiteSpace(rootFolder)
                    ? FileName
                    : Path.Combine(rootFolder, FileName);
                if (!File.Exists(qualifiedFileName))
                    throw new NullReferenceException("Could not find " + qualifiedFileName + ". To generate it run the command exe with the " + ValidArgPrefixes.First() + Type.Name + " switch");

                var smartSettingsJson = File.ReadAllText(qualifiedFileName);
                if (string.IsNullOrWhiteSpace(smartSettingsJson))
                    throw new NullReferenceException(qualifiedFileName + " is empty. To generate it run the command exe with the " + ValidArgPrefixes.First() + Type.Name + " switch");

                return JsonHelper.JsonStringToObject(smartSettingsJson, Type);
            }

            public string FileName
            {
                get { return Type.Name + (Name == null ? "" : "_" + Name) + ".txt"; }
            }
        }
    }
}

