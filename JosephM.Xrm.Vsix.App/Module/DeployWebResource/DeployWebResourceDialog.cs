using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Xrm.Vsix.Application;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [RequiresConnection]
    public class DeployWebResourceDialog
        : ServiceRequestDialog<DeployWebResourceService, DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
        public DeployWebResourceDialog(DeployWebResourceService service, IVisualStudioService visualStudioService, IDialogController dialogController)
            : base(service, dialogController)
        {
            VisualStudioService = visualStudioService;
        }

        private string AssemblyLoadErrorMessage { get; set; }
        public IVisualStudioService VisualStudioService { get; }

        protected override void LoadDialogExtention()
        {
            //hijack the load method so that we can prepopulate
            //the entered request with various details
            var files = VisualStudioService.GetSelectedFileNamesQualified();
            VisualStudioService.SaveSelectedFiles();
            Request.Files = files;
            SkipObjectEntry = true;

            base.LoadDialogExtention();
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
