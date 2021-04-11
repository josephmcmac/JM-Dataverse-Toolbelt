using JosephM.Application.ViewModel.RecordEntry.Form;
using System;

namespace JosephM.Application.ViewModel.Attributes
{
    public abstract class FieldInContext : Attribute
    {
        public abstract bool IsInContext(RecordEntryViewModelBase recordEntryViewModel);
    }
}
