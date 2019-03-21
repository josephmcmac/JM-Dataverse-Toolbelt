using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Collections.Generic;

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

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("Create Folder For Website", Request.CreateFolderForWebsiteName.ToString());
            if (Request.RecordsToExport != null)
            {
                foreach(var type in Request.RecordsToExport)
                {
                    addProperty("Include " + type.RecordType?.Value, type.Selected.ToString());
                    addProperty("Include All " + type.RecordType?.Value, type.IncludeAll.ToString());
                }
            }
            return dictionary;
        }
    }
}
