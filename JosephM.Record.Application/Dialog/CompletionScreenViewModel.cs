#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.Shared;

#endregion

namespace JosephM.Record.Application.Dialog
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
            CompletionOptions = options;
            CompletionDetails = new ObjectsGridSectionViewModel("Summary", completionDetails, controller);
            CloseButton = new XrmButtonViewModel("Close", onClose, controller);
        }

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