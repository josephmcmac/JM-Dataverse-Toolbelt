using System;

namespace JosephM.Application.Options
{
    public interface IApplicationOptions
    {
        void AddOption(string optionLabel, Action action, ApplicationOptionType type);
    }
}
