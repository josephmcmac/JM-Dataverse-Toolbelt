using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using JosephM.Core.Attributes;
using System.ComponentModel;

namespace JosephM.Xrm.MigrateInternal
{
    public class MigratedLookupField : INotifyPropertyChanged
    {
        public MigratedLookupField(BulkCopyFieldValueResponse internalResponse)
        {
            InternalResponse = internalResponse;
            InternalResponse.PropertyChanged += (s,e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

        public BulkCopyFieldValueResponse GetInternalResponse()
        {
            return InternalResponse;
        }

        private BulkCopyFieldValueResponse InternalResponse { get; }

        private string _entityType;
        [DisplayOrder(5)]
        public string EntityType
        {
            get
            {
                return _entityType;
            }
            set
            {
                _entityType = value;
                OnPropertyChanged(nameof(EntityType));
            }
        }

        private string _sourceField;
        [DisplayOrder(10)]
        public string SourceField
        {
            get
            {
                return _sourceField;
            }
            set
            {
                _sourceField = value;
                OnPropertyChanged(nameof(SourceField));
            }
        }

        private string _targetField;
        [DisplayOrder(10)]
        public string TargetField
        {
            get
            {
                return _targetField;
            }
            set
            {
                _targetField = value;
                OnPropertyChanged(nameof(TargetField));
            }
        }

        private int _countToProcess;
        [DisplayOrder(15)]
        [GridWidth(125)]
        public int CountToProcess
        {
            get
            {
                return _countToProcess;
            }
            set
            {
                _countToProcess = value;
                OnPropertyChanged(nameof(CountToProcess));
            }
        }

        [DisplayOrder(20)]
        [GridWidth(125)]
        public int TotalRecordsProcessed
        {
            get
            {
                return InternalResponse.TotalRecordsProcessed;
            }
        }

        [DisplayOrder(25)]
        [GridWidth(125)]
        public int TotalRecordsUpdated
        {
            get
            {
                return InternalResponse.TotalRecordsUpdated;
            }
        }

        [DisplayOrder(30)]
        [GridWidth(125)]
        public int NumberOfErrors
        {
            get
            {
                return InternalResponse.NumberOfErrors;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
