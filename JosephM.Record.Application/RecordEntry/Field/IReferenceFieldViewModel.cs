#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.Shared;
using JosephM.Record.IService;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public interface IReferenceFieldViewModel
    {
        string RecordTypeToLookup { get; set; }

        IRecordService LookupService { get; }

        RecordEntryViewModelBase RecordEntryViewModel { get; }

        void Search();

        void SelectLookupGrid();

        bool Searching { get; }
    }
}