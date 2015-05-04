#region

using System;
using JosephM.Record.Application.Controller;
using Microsoft.Practices.Prism.Commands;

#endregion

namespace JosephM.Record.Application.ApplicationOptions
{
    /// <summary>
    ///     An Option Available In Menus For The Appplication
    /// </summary>
    public class ApplicationViewModel : ViewModelBase
    {
        public ApplicationViewModel(IApplicationController controller)
             : base(controller)
        {
        }

        public string ApplicationName
        {
            get { return ApplicationController.ApplicationName; }
        }
    }
}