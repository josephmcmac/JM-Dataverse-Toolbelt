using System;
using System.Collections.Generic;

namespace JosephM.Application.Prism.Module
{
    public interface ICommandLineExecutable
    {
        string CommandName { get; }
        string Description { get; }
        IDictionary<string, Action<string>> GetArgs();
        void Command();
    }
}
