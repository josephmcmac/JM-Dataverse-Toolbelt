#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Shared;
using System;
using System.Collections.Generic;
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
            IEnumerable<object> completionDetails,
            IApplicationController controller)
            : base(controller)
        {
            Heading = new HeadingViewModel(heading, controller);
            CompletionHeadingText = heading;
            CompletionOptions = options;
            CompletionDetails = new ObjectsGridSectionViewModel("Summary", completionDetails, controller);
            CloseButton = new XrmButtonViewModel("Close", onClose, controller);
        }

        public string CompletionHeadingText { get; set; }
        public HeadingViewModel Heading { get; set; }

        public IEnumerable<XrmButtonViewModel> CompletionOptions { get; private set; }

        public bool ShowCompletionDetails
        {
            get { return CompletionDetails != null && CompletionDetails.Items.Any(); }
        }

        public ObjectsGridSectionViewModel CompletionDetails { get; set; }

        public XrmButtonViewModel CloseButton { get; private set; }
    }
}