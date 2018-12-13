using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.InstanceComparer.AddToSolution
{
    public class AddToSolutionDialog :
        ServiceRequestDialog<AddToSolutionService, AddToSolutionRequest, AddToSolutionResponse, AddToSolutionResponseItem>
    {
        public AddToSolutionDialog(XrmRecordService xrmRecordService, IDialogController dialogController, AddToSolutionRequest request = null, Action onClose = null)
            : base(new AddToSolutionService(xrmRecordService), dialogController, xrmRecordService, request: request, onClose: onClose)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "Add To Solution Completed";
        }
    }
}