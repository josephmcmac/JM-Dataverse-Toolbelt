using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.Prism.XrmModule.Test
{
    public class FakeXrmApplicationController : FakeApplicationController
    {
        public FakeXrmApplicationController(SavedXrmConnections.SavedXrmConnections connections)
        {
            this.RegisterInstance<ISavedXrmConnections>(connections);
        }
    }
}
