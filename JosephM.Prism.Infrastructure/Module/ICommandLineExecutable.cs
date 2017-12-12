using System;
using System.Collections.Generic;

namespace JosephM.Application.Prism.Module
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
