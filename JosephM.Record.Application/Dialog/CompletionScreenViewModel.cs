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
        public CompletionScreenViewModel(Action onClose, string heading, IEnumerable<XrmButtonViewModel> options,
            object completionObject,
            IApplicationController controller)
            : base(controller)
        {
            Heading = new HeadingViewModel(heading, controller);
            CompletionHeadingText = heading;
            CompletionOptions = options;

            if (completionObject != null)
            {
                var formController = FormController.CreateForObject(completionObject, ApplicationController, null);
                CompletionDetails = new ObjectEntryViewModel(null, null, completionObject, formController);
                CompletionDetails.IsReadOnly = true;
                CompletionDetails.PropertyChanged += CompletionDetails_PropertyChanged;
            }

            //CompletionDetails = new ObjectsGridSectionViewModel("Summary", completionDetails, controller);
            CloseButton = new XrmButtonViewModel("Close", onClose, controller);
        }

        private void CompletionDetails_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ObjectEntryViewModel.MainFormInContext))
                OnPropertyChanged(nameof(DisplayCompletionHeading));
        }

        public string CompletionHeadingText { get; set; }
        public HeadingViewModel Heading { get; set; }

        public IEnumerable<XrmButtonViewModel> CompletionOptions { get; private set; }

        public bool ShowCompletionDetails
        {
            get { return CompletionDetails != null; }
        }

        public ObjectEntryViewModel CompletionDetails { get; set; }

        public XrmButtonViewModel CloseButton { get; private set; }

        public bool DisplayCompletionHeading
        {
            get { return CompletionDetails == null || CompletionDetails.MainFormInContext; }
        }
    }
}