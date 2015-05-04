#region

using System;
using Microsoft.Practices.Prism.Regions;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.TabArea;

#endregion

namespace JosephM.Record.Application.Navigation
{
    public class NavigationErrorViewModel : TabAreaViewModelBase, INavigationAware
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