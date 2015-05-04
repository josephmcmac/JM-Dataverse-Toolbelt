using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Dialog;

namespace JosephM.Prism.Infrastructure.Module
{
    /// <summary>
    ///     Base Class For A Module Which Plugs An Implemented Services Main Operation Into The Application
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TDialog"></typeparam>
    /// <typeparam name="TResponseItem"></typeparam>
    public class ServiceRequestModule<TDialog, TService, TRequest, TResponse, TResponseItem> : PrismModuleBase
        where TDialog : ServiceRequestDialog<TService, TRequest, TResponse, TResponseItem>
        where TService : ServiceBase<TRequest, TResponse, TResponseItem>
        where TRequest : ServiceRequestBase, new()
        where TResponse : ServiceResponseBase<TResponseItem>, new()
        where TResponseItem : ServiceResponseItem
    {
        public override void RegisterTypes()
        {
            RegisterType<TService>();
            RegisterTypeForNavigation<TDialog>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption(MainOperationName, MenuNames.Crm, ServiceRequestDialogCommand);
        }

        protected virtual string MainOperationName
        {
            get { return (typeof (TRequest)).GetDisplayName(); }
        }

        private void ServiceRequestDialogCommand()
        {
            NavigateTo<TDialog>();
        }
    }
}