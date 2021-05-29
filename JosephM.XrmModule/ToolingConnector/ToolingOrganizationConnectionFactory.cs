using JosephM.Application.Application;
using JosephM.Xrm;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Threading;

namespace JosephM.XrmModule.ToolingConnector
{
    public class ToolingOrganizationConnectionFactory : XrmOrganizationConnectionFactory
    {
        private Dictionary<string, CrmServiceClient> _cachedToolingConnections = new Dictionary<string, CrmServiceClient>();

        private object _lockObject = new Object();

        public IApplicationController ApplicationController { get; }

        public ToolingOrganizationConnectionFactory(IApplicationController applicationController)
        {
            ApplicationController = applicationController;
        }

        public override GetOrganisationConnectionResponse GetOrganisationConnection(IXrmConfiguration xrmConfiguration)
        {
            if (!xrmConfiguration.UseXrmToolingConnector)
            {
                return base.GetOrganisationConnection(xrmConfiguration);
            }
            else
            {
                lock (_lockObject)
                {
                    GetOrganisationConnectionResponse result = null;

                    Action connectionUsingTooling = () =>
                    {
                        if (string.IsNullOrWhiteSpace(xrmConfiguration.ToolingConnectionId))
                        {
                            throw new Exception($"{nameof(IXrmConfiguration.ToolingConnectionId)} Is Required On The {nameof(IXrmConfiguration)} When {nameof(IXrmConfiguration.UseXrmToolingConnector)} Is Set");
                        }

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
                                if (!_cachedToolingConnections.ContainsKey(xrmConfiguration.ToolingConnectionId))
                                {
                                    _cachedToolingConnections.Add(xrmConfiguration.ToolingConnectionId, loginFrm.CrmConnectionMgr.CrmSvc);
                                }
                            }
                            else
                            {
                                throw new Exception("A Successful Connection Was Not Made By The Tooling Connector");
                            }
                        }
                        var service = _cachedToolingConnections[xrmConfiguration.ToolingConnectionId];
                        var organisation = new Organisation(service);
                        result = new GetOrganisationConnectionResponse(service, organisation);
                    };
                    if (ApplicationController.CurrentThreadIsDispatcher())
                    {
                        connectionUsingTooling();
                    }
                    else
                    {
                        var isFinished = false;
                        Exception tempEx = null;
                        ApplicationController.DoOnMainThread(() =>
                        {
                            try
                            {
                                connectionUsingTooling();
                                isFinished = true;
                            }
                            catch (Exception ex)
                            {
                                tempEx = ex;
                                isFinished = true;
                            }
                        });
                        while (!isFinished)
                        {
                            Thread.Sleep(1000);
                        }
                        if (tempEx != null)
                            throw tempEx;
                    }

                    return result;
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