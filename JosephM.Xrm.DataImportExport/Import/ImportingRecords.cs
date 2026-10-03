using JosephM.Core.Attributes;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace JosephM.Xrm.DataImportExport.Import
{
    [DoNotAllowGridOpen]
    public class ImportingRecords : INotifyPropertyChanged
    {
        private readonly HashSet<string> _createdIds = new HashSet<string>();
        private readonly HashSet<string> _updatedIds = new HashSet<string>();
        private readonly HashSet<string> _skippedNoChangeIds = new HashSet<string>();
        private readonly Dictionary<string, List<string>> _fieldsForRetryById = new Dictionary<string, List<string>>();
        private readonly object _stateLock = new object();

        // simple counters exposed to UI (atomic reads/writes)
        private int _createdCount;
        private int _updatedCount;
        private int _noChangeCount;
        private int _fieldsForRetryCount;
        private int _errors;

        // Public API - these return quickly and enqueue mutations
        public bool HasBeenCreated(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            lock (_stateLock)
            {
                return _createdIds.Contains(id);
            }
        }

        public void AddedCreated(IEnumerable<IRecord> recordsCreated)
        {
            if (recordsCreated == null) return;
            var ids = recordsCreated.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id).ToArray();
            if (!ids.Any()) return;
            // update created ids synchronously so HasBeenCreated sees them immediately
            var added = 0;
            lock (_stateLock)
            {
                foreach (var id in ids)
                {
                    if (_createdIds.Add(id))
                        added++;
                }
                if (added > 0)
                {
                    _createdCount = _createdIds.Count;
                    OnPropertyChanged(nameof(Created));
                }
            }
        }

        public void AddedUpdated(IEnumerable<IRecord> recordsUpdated)
        {
            if (recordsUpdated == null) return;
            var ids = recordsUpdated.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id).ToArray();
            if (!ids.Any()) return;
            lock (_stateLock)
            {
                var addedUpdated = 0;
                var removedFromSkipped = 0;
                foreach (var id in ids)
                {
                    if (!_createdIds.Contains(id) && _updatedIds.Add(id))
                        addedUpdated++;
                    if (_skippedNoChangeIds.Remove(id))
                        removedFromSkipped++;
                }
                if (addedUpdated > 0)
                {
                    _updatedCount = _updatedIds.Count;
                    OnPropertyChanged(nameof(Updated));
                }
                if (removedFromSkipped > 0)
                {
                    _noChangeCount = _skippedNoChangeIds.Count;
                    OnPropertyChanged(nameof(NoChange));
                }
            }
        }

        public void AddSkippedNoChange(IEnumerable<IRecord> recordsSkipped)
        {
            if (recordsSkipped == null) return;
            var ids = recordsSkipped.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id).ToArray();
            if (!ids.Any()) return;
            lock (_stateLock)
            {
                var added = 0;
                foreach (var id in ids)
                {
                    if (_skippedNoChangeIds.Add(id))
                        added++;
                }
                if (added > 0)
                {
                    _noChangeCount = _skippedNoChangeIds.Count;
                    OnPropertyChanged(nameof(NoChange));
                }
            }
        }

        public void AddFieldForRetry(IRecord entity, string field)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id) || string.IsNullOrWhiteSpace(field)) return;
            lock (_stateLock)
            {
                if (!_fieldsForRetryById.TryGetValue(entity.Id, out var list))
                {
                    list = new List<string>();
                    _fieldsForRetryById[entity.Id] = list;
                }
                list.Add(field);
                _fieldsForRetryCount = _fieldsForRetryById.Sum(kv => kv.Value.Count);
                OnPropertyChanged(nameof(FieldsToRetry));
            }
        }

        public void RemoveFieldForRetry(IRecord entity, string field)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id) || string.IsNullOrWhiteSpace(field)) return;
            lock (_stateLock)
            {
                if (!_fieldsForRetryById.TryGetValue(entity.Id, out var list)) return;
                if (list.Remove(field))
                {
                    if (list.Count == 0) _fieldsForRetryById.Remove(entity.Id);
                    _fieldsForRetryCount = _fieldsForRetryById.Sum(kv => kv.Value.Count);
                    OnPropertyChanged(nameof(FieldsToRetry));
                }
            }
        }

        public void RemoveForRetry(IRecord entity)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id)) return;
            lock (_stateLock)
            {
                if (_fieldsForRetryById.TryGetValue(entity.Id, out var list))
                {
                    _fieldsForRetryById.Remove(entity.Id);
                    _fieldsForRetryCount = _fieldsForRetryById.Sum(kv => kv.Value.Count);
                    OnPropertyChanged(nameof(FieldsToRetry));
                }
            }
        }

        // Properties (cheap reads)
        [DisplayOrder(15)][GridWidth(125)] public int Total { get; set; }
        [DisplayOrder(10)] public string Type { get; set; }
        [DisplayOrder(20)][GridWidth(125)] public int Created => _createdCount;
        [DisplayOrder(30)][GridWidth(125)] public int Updated => _updatedCount;
        [DisplayOrder(40)][GridWidth(125)] public int NoChange => _noChangeCount;
        [DisplayOrder(50)][GridWidth(125)]
        public int Errors { get => _errors; set => _errors = value; }
        [DisplayOrder(60)][GridWidth(125)] public int FieldsToRetry => _fieldsForRetryCount;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddError()
        {
            Interlocked.Increment(ref _errors);
            OnPropertyChanged(nameof(Errors));
        }
    }
}