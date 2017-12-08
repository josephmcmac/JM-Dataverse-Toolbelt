using System;
using System.Collections.Generic;

namespace JosephM.Application.Options
{
    public class ApplicationOptions : IApplicationOptions
    {
        public void AddOption(string group, string optionLabel, Action action, string description = null)
        {
        }

        public IEnumerable<IApplicationOption> GetAllOptions()
        {
            return new IApplicationOption[0];
        }
    }
}
