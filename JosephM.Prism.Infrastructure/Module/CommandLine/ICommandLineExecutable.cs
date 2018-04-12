using System;
using System.Collections.Generic;

namespace JosephM.Application.Desktop.Module.CommandLine
{
    public interface ICommandLineExecutable
    {
        string CommandName { get; }
        string Description { get; }
        IEnumerable<CommandLineArgument> GetArgs();
        void Command();
        Type RequestType { get; }
    }
}
