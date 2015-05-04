#region

using System;
using JosephM.Core.Log;

#endregion

namespace JosephM.Core.Service
{
    public abstract class ServiceBase<TRequest, TResponse, TResponseItem>
        where TRequest : ServiceRequestBase
        where TResponse : ServiceResponseBase<TResponseItem>, new()
        where TResponseItem : ServiceResponseItem
    {
        public TResponse Execute(TRequest request, LogController controller)
        {
            var response = new TResponse();
            try
            {
                ExecuteExtention(request, response, controller);
            }
            catch (Exception ex)
            {
                response.SetFatalError(ex);
            }
            return response;
        }

        public abstract void ExecuteExtention(TRequest request, TResponse response, LogController controller);
    }
}