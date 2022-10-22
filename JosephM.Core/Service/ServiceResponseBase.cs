using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JosephM.Core.Service
{
    [Group(Sections.Message, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: -2, displayLabel: false)]
    [Group(Sections.Exception, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: -1, displayLabel: false)]
    [Group(Sections.ResponseItems, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, 1, displayLabel: false)]
    public class ServiceResponseBase<TResponseItem> : IProcessCompletion
        where TResponseItem : ServiceResponseItem
    {
        private readonly List<TResponseItem> _errors = new List<TResponseItem>();
        private bool _hideResponseItems;

        public ServiceResponseBase()
        {
            Success = true;
        }

        public void SetFatalError(Exception ex)
        {
            Success = false;
            Exception = ex;
        }

        [Hidden]
        public bool Success { get; private set; }

        [Hidden]
        public Exception Exception { get; set; }

        public void AddResponseItem(TResponseItem responseItem)
        {
            _errors.Add(responseItem);
        }

        public void AddResponseItems(IEnumerable<TResponseItem> responseItems)
        {
            _errors.AddRange(responseItems);
        }

        [DoNotLimitDisplayHeight]
        [Group(Sections.Message)]
        [DisplayOrder(-2)]
        [PropertyInContextByPropertyNotNull(nameof(Message))]
        public string Message { get; set; }

        [DoNotLimitDisplayHeight]
        [Group(Sections.Exception)]
        [DisplayOrder(-1)]
        [PropertyInContextByPropertyNotNull(nameof(Exception))]
        public string FatalErrorDescription
        {
            get
            {
                return "Fatal Error: " + Exception.DisplayString();
            }
        }

        [Hidden]
        public bool HideResponseItems
        {
            get { return _hideResponseItems; }
            set
            {
                _hideResponseItems = value;
            }
        }

        [AllowGridFullScreen]
        [AllowDownload]
        [Group(Sections.ResponseItems)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyValue(nameof(HasResponseItems), true)]
        [PropertyInContextByPropertyValue(nameof(HideResponseItems), false)]
        public IEnumerable<TResponseItem> ResponseItems
        {
            get { return _errors; }
        }

        public IEnumerable<TResponseItem> GetResponseItemsWithError()
        {
            return ResponseItems.Where(e => e.HasError);
        }

        IEnumerable<object> IProcessCompletion.GetResponseItemsWithError()
        {
            return GetResponseItemsWithError();
        }

        [Hidden]
        public bool HasResponseItems
        {
            get { return ResponseItems.Any(); }
        }

        [Hidden]
        public bool HasResponseItemError
        {
            get { return ResponseItems.Any(r => r.HasError); }
        }

        [Hidden]
        public bool HasError
        {
            get { return Exception != null || HasResponseItemError; }
        }

        bool IProcessCompletion.Success => Success;

        Exception IProcessCompletion.Exception => Exception;

        private static class Sections
        {
            public const string Message = "Message";
            public const string Exception = "Exception";
            public const string ResponseItems = "Response Items";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}