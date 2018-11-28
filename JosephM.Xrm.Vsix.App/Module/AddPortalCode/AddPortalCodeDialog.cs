using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using System;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    [RequiresConnection]
    public class AddPortalCodeDialog
        : ServiceRequestDialog<AddPortalCodeService, AddPortalCodeRequest, AddPortalCodeResponse, AddPortalCodeResponseItem>
    {
        public AddPortalCodeDialog(AddPortalCodeService service, IDialogController dialogController)
            : base(service, dialogController, service.Service)
        {

        }

        protected override void LoadDialogExtention()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var selectedItems = visualStudioService.GetSelectedItems();
            string selectedItemName = null;
            foreach(var item in selectedItems)
            {
                selectedItemName = item.Name;
            }

            Request.ProjectName = selectedItemName ?? throw new NullReferenceException("Error getting selected project name");

            if (!Service.Service.RecordTypeExists(Entities.adx_website))
                throw new Exception("The Dialog Cannot Run As The Dynamics Instance Does Not Contain The Portal Record Types");
            base.LoadDialogExtention();
        }

        protected override void CompleteDialogExtention()
        {
            CompletionMessage = "Export Completed";
            base.CompleteDialogExtention();
        }
    }
}
