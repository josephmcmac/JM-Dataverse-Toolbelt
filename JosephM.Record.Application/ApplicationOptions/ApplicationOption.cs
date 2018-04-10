#region

using JosephM.Application.Options;
using Prism.Commands;
using System;

#endregion

namespace JosephM.Application.ViewModel.ApplicationOptions
{
    /// <summary>
    ///     An Option Available In Menus For The Appplication
    /// </summary>
    public class ApplicationOption : IApplicationOption
    {
        public ApplicationOption(string label, Action action, string description = null)
        {
            Label = label;
            Description = description;
            DelegateCommand = new DelegateCommand(action);
        }

        public string Label { get; private set; }

        public string Description { get; private set; }

        public DelegateCommand DelegateCommand { get; private set; }

        public void InvokeMethod()
        {
            DelegateCommand.Execute();
        }

        public override string ToString()
        {
            return Label;
        }
    }
}