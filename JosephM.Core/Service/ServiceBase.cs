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
        public TResponse Execute(TRequest request, ServiceRequestController controller, TResponse response = null)
        {
            var thisResponse = response ?? new TResponse();
            try
            {
                ExecuteExtention(request, thisResponse, controller);
            }
            catch (Exception ex)
            {
                thisResponse.SetFatalError(ex);
            }
            return thisResponse;
        }

        public abstract void ExecuteExtention(TRequest request, TResponse response, ServiceRequestController controller);
    }
}