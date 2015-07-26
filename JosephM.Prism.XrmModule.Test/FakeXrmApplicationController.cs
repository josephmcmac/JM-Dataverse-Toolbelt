using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Practices.Unity;

namespace JosephM.Prism.XrmModule.Test
{
    public class FakeXrmApplicationController : FakeApplicationController
    {
        public FakeXrmApplicationController(SavedXrmConnections.SavedXrmConnections connections)
        {
            Container.RegisterInstance<ISavedXrmConnections>(connections);
        }
    }
}
