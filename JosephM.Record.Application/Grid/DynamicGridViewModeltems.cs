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
    }
}