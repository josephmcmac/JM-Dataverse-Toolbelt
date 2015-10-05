using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace JosephM.Xrm.Plugins
{
    public static class ExceptionExtentions
    {
        public static string XrmDisplayString(this Exception ex)
        {
            var result = "";
            if (ex != null)
            {
                result = string.Concat(ex.GetType(), ": ", ex.Message, "\n", ex.StackTrace);
                if (ex.InnerException != null)
                    result = string.Concat(result, "\n", XrmDisplayString(ex.InnerException));
                if (ex is FaultException<OrganizationServiceFault>)
                {
                    var crmType = (FaultException<OrganizationServiceFault>)ex;
                    if (crmType.Detail != null)
                        result = string.Concat(result, (string)"\nTrace:\n", (string)crmType.Detail.TraceText);
                }
            }
            return result;
        }
    }
}