using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using System;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public abstract class CustomFunction : Attribute
    {
        public abstract string GetFunctionLabel();

        public abstract Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference);
    }
}