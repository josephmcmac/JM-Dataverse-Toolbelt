#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace JosephM.Core.Service
{
    public class ServiceResponseBase<TResponseItem>
        where TResponseItem : ServiceResponseItem
    {
        private readonly List<TResponseItem> _errors = new List<TResponseItem>();

        public ServiceResponseBase()
        {
            Success = true;
        }

        public void SetFatalError(Exception ex)
        {
            Success = false;
            Exception = ex;
        }

        public bool Success { get; private set; }
        public Exception Exception { get; set; }

        public void AddResponseItem(TResponseItem responseItem)
        {
            _errors.Add(responseItem);
        }

        public void AddResponseItems(IEnumerable<TResponseItem> responseItems)
        {
            _errors.AddRange(responseItems);
        }

        public IEnumerable<TResponseItem> ResponseItems
        {
            get { return _errors; }
        }

        public IEnumerable<TResponseItem> ResponseItemsWithError
        {
            get { return ResponseItems.Where(e => e.HasError); }
        }

        public bool HasError
        {
            get { return Exception != null || ResponseItems.Any(r => r.HasError); }
        }
    }
}