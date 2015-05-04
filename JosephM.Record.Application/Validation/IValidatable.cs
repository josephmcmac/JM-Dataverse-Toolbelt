#region

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using JosephM.Core.Service;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.Validation
{
    public interface IValidatable
    {
        RecordEntryViewModelBase GetRecordForm();

        string ReferenceName { get; }
    }
}