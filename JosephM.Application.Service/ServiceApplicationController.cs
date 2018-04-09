using System;
using System.Collections.Generic;
using System.Diagnostics;
using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Record.Application.Fakes;
using Microsoft.Practices.Unity;

namespace JosephM.Prism.Infrastructure.Prism
{
    public class ServiceApplicationController : ApplicationControllerBase
    {
        public ServiceApplicationController(string applicationName) 
            : base(applicationName, new ServiceDependencyContainer(new UnityContainer()))
        {
            EventSource = applicationName;
        }

        private string EventSource { get; set; }

        public override void Remove(string regionName, object item)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetObjects(string regionName)
        {
            throw new NotImplementedException();
        }

        public override void RequestNavigate(string regionName, Type type, UriQuery uriQuery)
        {
            throw new NotImplementedException();
        }

        public override void UserMessage(string message)
        {
            EventLog.WriteEntry(EventSource, message, EventLogEntryType.Error);
        }

        public override bool UserConfirmation(string message)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFolderName()
        {
            throw new NotImplementedException();
        }
    }
}
