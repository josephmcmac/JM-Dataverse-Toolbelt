#region

using JosephM.Application.Options;
using JosephM.Application.ViewModel.Shared;
using System;

#endregion

namespace JosephM.Application.ViewModel.ApplicationOptions
{
    /// <summary>
    ///     An Option Available In Menus For The Appplication
    /// </summary>
    public class ApplicationOption : IApplicationOption
    {
        public ApplicationOption(string label, Action action, string description = null, int order = 0)
        {
            Label = label;
            Description = description;
            Order = order;
            DelegateCommand = new MyCommand(action);
        }

        public string Label { get; private set; }

        public string Description { get; private set; }
        public int Order { get; }
        public MyCommand DelegateCommand { get; private set; }

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