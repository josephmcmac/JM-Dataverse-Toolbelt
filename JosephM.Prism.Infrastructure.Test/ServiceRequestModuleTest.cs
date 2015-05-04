using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Service;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;

namespace JosephM.Prism.Infrastructure.Test
{
    public class ServiceRequestModuleTest<TModule, TDialog, TService, TRequest, TResponse, TResponseItem> : PrismModuleTest<TModule>
        where TModule : ServiceRequestModule<TDialog, TService, TRequest, TResponse, TResponseItem>, new()
        where TDialog : ServiceRequestDialog<TService, TRequest, TResponse, TResponseItem>
        where TService : ServiceBase<TRequest, TResponse, TResponseItem>
        where TRequest : ServiceRequestBase, new()
        where TResponse : ServiceResponseBase<TResponseItem>, new()
        where TResponseItem : ServiceResponseItem
    {
        protected virtual void PrepareTest()
        {
            
        }

        public void ExecuteTest()
        {
            PrepareTest();
            var dialog = Container.Resolve<TDialog>();
            var dialogController = dialog.Controller;
            dialogController.BeginDialog();
            CompleteTest();
        }

        protected virtual void CompleteTest()
        {

        }
    }
}