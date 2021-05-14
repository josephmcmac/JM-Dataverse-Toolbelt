using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using System.ComponentModel;

namespace JosephM.Deployment.MigrateInternal
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

        private string _field;
        public string Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
                OnPropertyChanged(nameof(Field));
            }
        }

        private int _countToProcess;
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

        public int TotalRecordsProcessed
        {
            get
            {
                return InternalResponse.TotalRecordsProcessed;
            }
        }

        public int TotalRecordsUpdated
        {
            get
            {
                return InternalResponse.TotalRecordsUpdated;
            }
        }

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
