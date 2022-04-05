using Microsoft.Xrm.Sdk;
using System;

namespace $safeprojectname$.Xrm
{
    public abstract class XrmActionRegistration : IPlugin
    {
        public XrmActionRegistration(string unsecureConfig, string secureConfig)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }

        public string UnsecureConfig { get; }
        public string SecureConfig { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var actionInstance = CreateActionInstance(context.MessageName);
            actionInstance.ServiceProvider = serviceProvider;
            actionInstance.UnsecureConfig = UnsecureConfig;
            actionInstance.SecureConfig = SecureConfig;
            actionInstance.PostActionSynch();
        }

        public abstract XrmAction CreateActionInstance(string messageName);
    }
}