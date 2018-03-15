using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace $safeprojectname$.Xrm
{
    /// <summary>
    /// Extention methods for xrm related objects
    /// </summary>
    public static class XrmExtentions
    {
        /// <summary>
        /// Outputs a detailed string for an exception including the trace details for an OrganizationServiceFault
        /// </summary>
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