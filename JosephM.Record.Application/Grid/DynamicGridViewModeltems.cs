#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Annotations;
using System.Windows.Documents;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Record.Application.Grid
{
    /// <summary>
    ///     Container For Properties Required By A IDynamicGridViewModel Without Having To Implement Them For Each Concrete
    ///     Type
    /// </summary>
    public class DynamicGridViewModelItems
    {
        public DynamicGridViewModelItems()
        {
            DeleteRow =
                r =>
                {
                    throw new NullReferenceException(
                        "The Delete Action Has Not Been Populated. The DeleteRow Property Requires Setting On The " +
                        typeof (DynamicGridViewModelItems).Name);
                };
            EditRow =
                r =>
                {
                    throw new NullReferenceException(
                        "The Edit Action Has Not Been Populated. The Edit Property Requires Setting On The " +
                        typeof(DynamicGridViewModelItems).Name);
                };
            OnDoubleClick = () => { };
            OnKeyDown = () => { };
        }

        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public Action<GridRowViewModel> DeleteRow { get; set; }
        public Action<GridRowViewModel> EditRow { get; set; }
        public Action OnDoubleClick { get; set; }
        public Action OnKeyDown { get; set; }

        private string _lastSortField;
        private bool _lastSortAscending = false;
        /// <summary>
        /// sorts the grid
        /// put in here to avoid repeating for each concrete IDynamicGridViewModel type
        /// </summary>
        /// <param name="theGridViewModel"></param>
        /// <param name="sortField"></param>
        public void SortIt(IDynamicGridViewModel theGridViewModel, string sortField)
        {
            //todo no validation script for this
            //just copy all into a new list (sorting as go), clear the collection then add each sorted item
            if (theGridViewModel.GridRecords.Any())
            {
                var newList = new List<GridRowViewModel>();
                foreach (var item in theGridViewModel.GridRecords)
                {
                    var value1 = item.GetFieldViewModel(sortField).ValueObject;
                    if (value1 == null)
                    {
                        newList.Insert(0, item);
                        continue;
                    }
                    foreach (var item2 in newList)
                    {
                        var value2 = item2.GetFieldViewModel(sortField).ValueObject;
                        if (value2 == null)
                        {
                            continue;
                        }
                        else if (!(value1 is Enum) && value1 is IComparable)
                        {
                            if (((IComparable)value1).CompareTo(value2) < 0)
                            {
                                newList.Insert(newList.IndexOf(item2), item);
                                break;
                            }
                            else
                                continue;
                        }
                        var sortString1 = value1.ToString();
                        var sortString2 = value2.ToString();
                        if (value1 is Enum)
                            sortString1 = ((Enum) value1).GetDisplayString();
                        if (value2 is Enum)
                            sortString2 = ((Enum)value2).GetDisplayString();
                        if (String.Compare(sortString1, sortString2, StringComparison.Ordinal) < 0)
                        {
                            newList.Insert(newList.IndexOf(item2), item);
                            break;
                        }
                    }
                    if(!newList.Contains(item))
                        newList.Add(item);
                }
                //just a check for already sorted ascending the sort descending
                if (_lastSortField != sortField)
                    _lastSortAscending = true;
                else
                {
                    if (_lastSortAscending)
                        newList.Reverse();
                    _lastSortAscending = !_lastSortAscending;
                }
                _lastSortField = sortField;
                
                theGridViewModel.GridRecords.Clear();
                foreach (var item in newList)
                    theGridViewModel.GridRecords.Add(item);
            }
        }
    }
}