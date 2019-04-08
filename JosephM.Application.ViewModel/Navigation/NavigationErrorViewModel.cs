#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Core.Extentions;
using System;

#endregion

namespace JosephM.Application.ViewModel.Navigation
{
    public class NavigationErrorViewModel : TabAreaViewModelBase
    {
        public NavigationErrorViewModel(Exception exception, IApplicationController controller)
            : base(controller)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }

        public string ErrorDisplay
        {
            get { return Exception.DisplayString(); }
        }

        public override string TabLabel
        {
            get { return "Error"; }
        }
    }
}