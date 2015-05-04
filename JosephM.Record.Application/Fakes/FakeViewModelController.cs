#region

using System.Windows.Threading;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Record.Application.Fakes
{
    internal class FakeViewModelController
    {
        public FakeViewModelController()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            ApplicationController = new FakeApplicationController();
        }

        public Dispatcher Dispatcher { get; private set; }


        public IApplicationController ApplicationController { get; private set; }
    }
}