#region

using System;
using JosephM.Application.Options;
using Microsoft.Practices.Prism.Commands;

#endregion

namespace JosephM.Application.ViewModel.ApplicationOptions
{
    /// <summary>
    ///     An Option Available In Menus For The Appplication
    /// </summary>
    public class ApplicationOption
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

        public override string ToString()
        {
            return Label;
        }
    }
}