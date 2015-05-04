#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.Grid
{
    /// <summary>
    ///     A Type Which Will Have A Set Of Records To Display In A Grid With Dynamically Generated Columns
    /// </summary>
    public interface IDynamicGridViewModel
    {
        /// <summary>
        ///     Metadata For The Columns To Display For Each Record in The Grid
        /// </summary>
        IEnumerable<GridFieldMetadata> RecordFields { get; }

        /// <summary>
        ///     IRecordService For Retreiving Data And Associated Metadata For The Grid
        /// </summary>
        IRecordService RecordService { get; }

        /// <summary>
        ///     The Record Type Displayed In The Grid
        /// </summary>
        string RecordType { get; }

        /// <summary>
        ///     The Active Data In The Grid
        /// </summary>
        ObservableCollection<GridRowViewModel> GridRecords { get; }

        /// <summary>
        ///     The Currently Selected Row In The Grid
        /// </summary>
        GridRowViewModel SelectedRow { get; set; }

        /// <summary>
        ///     Invokes An Action To Execute On The Primary STA Thread Of The Application
        /// </summary>
        void DoOnMainThread(Action action);

        /// <summary>
        ///     Invokes An Action To Execute Asynchronously
        /// </summary>
        void DoOnAsynchThread(Action action);

        /// <summary>
        ///     Performs An Action While Setting A Loading Item To Display
        /// </summary>
        void DoWhileLoading(string message, Action action);

        /// <summary>
        ///     Access To The IRecordService And Metadata Required By The Grid
        /// </summary>
        FormController FormController { get; }

        /// <summary>
        ///     Defines If Data In The Grid Should Be Editable
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        ///     Additional Properties For The IDynamicGridViewModel
        /// </summary>
        DynamicGridViewModelItems DynamicGridViewModelItems { get; }
    }
}