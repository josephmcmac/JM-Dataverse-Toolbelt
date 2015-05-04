#region

using System;

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
            OnDoubleClick = () => { };
            OnKeyDown = () => { };
        }

        public bool CanDelete { get; set; }
        public Action<GridRowViewModel> DeleteRow;
        public Action OnDoubleClick;
        public Action OnKeyDown;
    }
}