using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [RequiresConnection]
    public class DeployWebResourceDialog
        : ServiceRequestDialog<DeployWebResourceService, DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
        public DeployWebResourceDialog(DeployWebResourceService service, IDialogController dialogController)
            : base(service, dialogController)
        {
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if (Response.ResponseItems != null)
            {
                addProperty("Import Count", Response.ResponseItems.Count().ToString());
            }
            return dictionary;
        }
    }
}
