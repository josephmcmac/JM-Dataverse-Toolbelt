using System;
using System.Collections.Generic;

namespace JosephM.Application.Application
{
    public interface IApplicationLogger
    {
        void LogEvent(string eventName, IDictionary<string, string> properties = null);
        void LogException(Exception ex);
    }
}
