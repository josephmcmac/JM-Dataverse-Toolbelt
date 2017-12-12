using System;
using System.Collections.Generic;

namespace JosephM.Application.Prism.Module
{
    public class CommandLineArgument
    {
        public CommandLineArgument(string commandName, string description)
            : this(commandName, description, null)
        {
        }

        public CommandLineArgument(string commandName, string description, Action<string> loadAction)
        {
            CommandName = commandName;
            Description = description;
            LoadAction = (s) => { };
            if (loadAction != null)
                LoadAction = loadAction;
        }

        public string CommandName { get; set; }
        public string Description { get; set; }
        public Action<string> LoadAction { get; set; }
    }
}
