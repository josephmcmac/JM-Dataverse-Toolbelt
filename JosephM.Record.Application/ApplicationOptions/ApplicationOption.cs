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
        public ApplicationOption(string label, Action action)
            : this(label, action, ApplicationOptionType.Main)
        {
        }

        public ApplicationOption(string label, Action action, ApplicationOptionType type)
        {
            Label = label;
            DelegateCommand = new DelegateCommand(action);
            Type = type;
        }

        public ApplicationOptionType Type { get; set; }

        public string Label { get; private set; }

        public DelegateCommand DelegateCommand { get; private set; }

        public override string ToString()
        {
            return Label;
        }
    }
}