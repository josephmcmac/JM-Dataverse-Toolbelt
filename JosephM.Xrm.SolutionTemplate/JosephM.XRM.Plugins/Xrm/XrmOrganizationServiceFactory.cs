using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;

namespace $safeprojectname$.Xrm
{
    public class XrmOrganizationServiceFactory
    {
        private Dictionary<string, CrmServiceClient> _cachedToolingConnections = new Dictionary<string, CrmServiceClient>();

        private object _lockObject = new Object();

        public IOrganizationService GetOrganisationService(IXrmConfiguration xrmConfiguration)
        {
            if (!xrmConfiguration.UseXrmToolingConnector)
            {
                return XrmConnection.GetOrgServiceProxy(xrmConfiguration);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(xrmConfiguration.ToolingConnectionId))
                {
                    throw new Exception($"{nameof(IXrmConfiguration.ToolingConnectionId)} Is Required On The {nameof(IXrmConfiguration)} When {nameof(IXrmConfiguration.UseXrmToolingConnector)} Is Set");
                }
                lock (_lockObject)
                {
                    if (!_cachedToolingConnections.ContainsKey(xrmConfiguration.ToolingConnectionId))
                    {
                        var loginFrm = new ToolingConnectorForm(xrmConfiguration.ToolingConnectionId, xrmConfiguration.Name);
                        // Login process is Async, thus we need to detect when login is completed and close the form. 
                        loginFrm.ConnectionToCrmCompleted += LoginFrm_ConnectionToCrmCompleted;
                        // Show the dialog here. 
                        loginFrm.ShowDialog();

                        // If the login process completed, assign the connected service to the CRMServiceClient var 
                        if (loginFrm.CrmConnectionMgr != null && loginFrm.CrmConnectionMgr.CrmSvc != null && loginFrm.CrmConnectionMgr.CrmSvc.IsReady)
                        {
                            _cachedToolingConnections.Add(xrmConfiguration.ToolingConnectionId, loginFrm.CrmConnectionMgr.CrmSvc);
                        }
                        else
                        {
                            throw new Exception("A Successful Connection Was Not Made By The Tooling Connector");
                        }
                    }
                    return _cachedToolingConnections[xrmConfiguration.ToolingConnectionId];
                }
            }
        }

        private static void LoginFrm_ConnectionToCrmCompleted(object sender, EventArgs e)
        {
            if (sender is ToolingConnectorForm)
            {
                ((ToolingConnectorForm)sender).Close();
            }
        }
    }
}