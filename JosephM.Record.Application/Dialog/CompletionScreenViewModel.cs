#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    /// <summary>
    ///     For Providing A Summary Of What Happened During A Dialog Process Which Has Completed
    /// </summary>
    public class CompletionScreenViewModel : ViewModelBase
    {
        public CompletionScreenViewModel(Action onClose, object completionObject,
            IApplicationController controller)
            : base(controller)
        {
            if (completionObject != null)
            {
                var formController = FormController.CreateForObject(completionObject, ApplicationController, null);
                CompletionDetails = new ObjectEntryViewModel(null, onClose, completionObject, formController, cancelButtonLabel: "Close");
                CompletionDetails.IsReadOnly = true;
            }
        }

        public bool ShowCompletionDetails
        {
            get { return CompletionDetails != null; }
        }

        public string CompletionHeadingText { get; set; }

        public ObjectEntryViewModel CompletionDetails { get; set; }

    }
}