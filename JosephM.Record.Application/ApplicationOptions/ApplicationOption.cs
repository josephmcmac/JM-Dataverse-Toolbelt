#region

using System;
using Microsoft.Practices.Prism.Commands;

#endregion

namespace JosephM.Record.Application.ApplicationOptions
{
    /// <summary>
    ///     An Option Available In Menus For The Appplication
    /// </summary>
    public class ApplicationOption
    {
        public ApplicationOption(string label, string menuGroup, Action action)
            : this(label, menuGroup, action, ApplicationOptionType.Main)
        {
        }

        public ApplicationOption(string label, string menuGroup, Action action, ApplicationOptionType type)
        {
            Label = label;
            MenuGroup = menuGroup;
            DelegateCommand = new DelegateCommand(action);
            Type = type;
        }

        public ApplicationOptionType Type { get; set; }

        public string MenuGroup { get; private set; }

        public string Label { get; private set; }

        public DelegateCommand DelegateCommand { get; private set; }

        public override string ToString()
        {
            return Label;
        }
    }
}