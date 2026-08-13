using JosephM.Core.Attributes;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JosephM.Xrm.DataImportExport.Import
{
    [DoNotAllowGridOpen]
    public class ImportingRecords : INotifyPropertyChanged
    {
        private object _lockObject = new object();
        private IDictionary<string, IRecord> _createdEntities = new SortedDictionary<string, IRecord>();
        private List<IRecord> _updatedEntities = new List<IRecord>();
        private List<IRecord> _skippedNoChangeEntities = new List<IRecord>();
        private IDictionary<IRecord, List<string>> _fieldsForRetry = new Dictionary<IRecord, List<string>>();
        private int _errors;

        public IDictionary<string, IRecord> GetCreatedEntities()
        {
            return _createdEntities;
        }

        public bool HasBeenCreated(string id)
        {
            return _createdEntities.ContainsKey(id);
        }

        public void AddedCreated(IRecord entity)
        {
            lock (_lockObject)
            {
                if (!_createdEntities.ContainsKey(entity.Id))
                {
                    _createdEntities.Add(entity.Id, entity);
                    OnPropertyChanged(nameof(Created));
                }
            }
        }

        public void AddedUpdated(IRecord entity)
        {
            lock (_lockObject)
            {
                if (!_createdEntities.ContainsKey(entity.Id)
                && !_updatedEntities.Contains(entity))
                {
                    _updatedEntities.Add(entity);
                    OnPropertyChanged(nameof(Updated));
                }
                if (_skippedNoChangeEntities.Contains(entity))
                    _skippedNoChangeEntities.Remove(entity);
            }
        }

        [DisplayOrder(15)]
        [GridWidth(125)]
        public int Total
        {
            get; set;
        }

        [DisplayOrder(10)]
        public string Type { get; set; }
        [DisplayOrder(20)]
        [GridWidth(125)]
        public int Created
        {
            get
            {
                lock (_lockObject)
                {
                    return _createdEntities.Count;
                }
            }
        }
        [DisplayOrder(30)]
        [GridWidth(125)]
        public int Updated
        {
            get
            {
                lock (_lockObject)
                {
                    return _updatedEntities.Count;
                }
            }
        }

        [DisplayOrder(40)]
        [GridWidth(125)]
        public int NoChange
        {
            get
            {
                lock (_lockObject)
                {
                    return _skippedNoChangeEntities.Count;
                }
            }
        }

        [DisplayOrder(50)]
        [GridWidth(125)]
        public int Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged(nameof(Errors));
            }
        }

        [DisplayOrder(60)]
        [GridWidth(125)]
        public int FieldsToRetry
        {
            get
            {
                lock (_lockObject)
                {
                    return _fieldsForRetry.SelectMany(kv => kv.Value).Count();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddError()
        {
            Errors++;
        }

        public void AddFieldForRetry(IRecord entity, string field)
        {
            lock (_lockObject)
            {
                if (!_fieldsForRetry.ContainsKey(entity))
                    _fieldsForRetry.Add(entity, new List<string>());
                _fieldsForRetry[entity].Add(field);
                OnPropertyChanged(nameof(FieldsToRetry));
            }
        }

        public void AddSkippedNoChange(IRecord entity)
        {
            lock (_lockObject)
            {
                if (!_skippedNoChangeEntities.Contains(entity))
                {
                    _skippedNoChangeEntities.Add(entity);
                    OnPropertyChanged(nameof(NoChange));
                }
            }
        }

        public void RemoveFieldForRetry(IRecord entity, string field)
        {
            lock (_lockObject)
            {
                if (!_fieldsForRetry.ContainsKey(entity))
                    return;
                _fieldsForRetry[entity].Remove(field);
                if (!_fieldsForRetry[entity].Any())
                {
                    _fieldsForRetry.Remove(entity);
                }
                OnPropertyChanged(nameof(FieldsToRetry));
            }
        }

        public void RemoveForRetry(IRecord entity)
        {
            lock (_lockObject)
            {
                if (_fieldsForRetry.ContainsKey(entity))
                { 
                    _fieldsForRetry.Remove(entity);
                    OnPropertyChanged(nameof(FieldsToRetry));
                }
            }
        }
    }
}