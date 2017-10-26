using System;

namespace JosephM.Application.Options
{
    public interface IApplicationOptions
    {
        void AddOption(string group, string optionLabel, Action action);
    }
}
