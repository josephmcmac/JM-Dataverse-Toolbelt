using JosephM.Core.Attributes;
using JosephM.Record.IService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JosephM.Xrm.DataImportExport.Import
{
    [DoNotAllowGridOpen]
    public class ImportingRecords : INotifyPropertyChanged, IDisposable
    {
        // single-threaded mutation: worker processes queued actions
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private readonly Task _worker;
        private readonly SynchronizationContext _uiContext;

        // internal state (mutated only by worker)
        private readonly HashSet<string> _createdIds = new HashSet<string>();
        private readonly HashSet<string> _updatedIds = new HashSet<string>();
        private readonly HashSet<string> _skippedNoChangeIds = new HashSet<string>();
        private readonly Dictionary<string, List<string>> _fieldsForRetryById = new Dictionary<string, List<string>>();

        // simple counters exposed to UI (atomic reads/writes)
        private int _createdCount;
        private int _updatedCount;
        private int _noChangeCount;
        private int _fieldsForRetryCount;
        private int _errors;

        public ImportingRecords()
        {
            _uiContext = SynchronizationContext.Current;
            _worker = Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        // Enqueue helpers (non-blocking)
        private void Enqueue(Action a)
        {
            try { _queue.Add(a); } catch { /* swallow if completed */ }
        }

        private void ProcessQueue()
        {
            foreach (var action in _queue.GetConsumingEnumerable())
            {
                try { action(); }
                catch { /* ignore worker errors to keep processing */ }
            }
        }

        // Public API - these return quickly and enqueue mutations
        public bool HasBeenCreated(string id)
        {
            // best-effort: check counter-based existence via created ids may be slightly stale
            return !string.IsNullOrWhiteSpace(id) && VolatileReadHashContains(_createdIds, id);
        }

        public void AddedCreated(IEnumerable<IRecord> recordsCreated)
        {
            if (recordsCreated == null) return;
            var ids = recordsCreated.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id).ToArray();
            if (!ids.Any()) return;
            Enqueue(() =>
            {
                var added = 0;
                foreach (var id in ids)
                {
                    if (_createdIds.Add(id))
                        added++;
                }
                if (added > 0)
                {
                    _createdCount = _createdIds.Count;
                    PostNotify(nameof(Created));
                }
            });
        }

        public void AddedUpdated(IEnumerable<IRecord> recordsUpdated)
        {
            if (recordsUpdated == null) return;
            var ids = recordsUpdated.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id).ToArray();
            if (!ids.Any()) return;
            Enqueue(() =>
            {
                var addedUpdated = 0;
                var removedFromSkipped = 0;
                foreach (var id in ids)
                {
                    if (!_createdIds.Contains(id) && _updatedIds.Add(id))
                        addedUpdated++;
                    if (_skippedNoChangeIds.Remove(id))
                        removedFromSkipped--;
                }
                if (addedUpdated > 0)
                {
                    _updatedCount = _updatedIds.Count;
                    PostNotify(nameof(Updated));
                }
                if (removedFromSkipped > 0)
                {
                    _noChangeCount = _skippedNoChangeIds.Count;
                    PostNotify(nameof(NoChange));
                }
            });
        }

        public void AddSkippedNoChange(IEnumerable<IRecord> recordsSkipped)
        {
            if (recordsSkipped == null) return;
            var ids = recordsSkipped.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id).ToArray();
            if (!ids.Any()) return;
            Enqueue(() =>
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
                    PostNotify(nameof(NoChange));
                }
            });
        }

        public void AddFieldForRetry(IRecord entity, string field)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id) || string.IsNullOrWhiteSpace(field)) return;
            var id = entity.Id;
            Enqueue(() =>
            {
                if (!_fieldsForRetryById.TryGetValue(id, out var list))
                {
                    list = new List<string>();
                    _fieldsForRetryById[id] = list;
                }
                list.Add(field);
                _fieldsForRetryCount = _fieldsForRetryById.Sum(kv => kv.Value.Count);
                PostNotify(nameof(FieldsToRetry));
            });
        }

        public void RemoveFieldForRetry(IRecord entity, string field)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id) || string.IsNullOrWhiteSpace(field)) return;
            var id = entity.Id;
            Enqueue(() =>
            {
                if (!_fieldsForRetryById.TryGetValue(id, out var list)) return;
                if (list.Remove(field))
                {
                    if (list.Count == 0) _fieldsForRetryById.Remove(id);
                    _fieldsForRetryCount = _fieldsForRetryById.Sum(kv => kv.Value.Count);
                    PostNotify(nameof(FieldsToRetry));
                }
            });
        }

        public void RemoveForRetry(IRecord entity)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id)) return;
            var id = entity.Id;
            Enqueue(() =>
            {
                if (_fieldsForRetryById.TryGetValue(id, out var list))
                {
                    _fieldsForRetryById.Remove(id);
                    _fieldsForRetryCount = _fieldsForRetryById.Sum(kv => kv.Value.Count);
                    PostNotify(nameof(FieldsToRetry));
                }
            });
        }

        // Properties (cheap reads)
        [DisplayOrder(15)][GridWidth(125)] public int Total { get; set; }
        [DisplayOrder(10)] public string Type { get; set; }
        [DisplayOrder(20)][GridWidth(125)] public int Created => Volatile.Read(ref _createdCount);
        [DisplayOrder(30)][GridWidth(125)] public int Updated => Volatile.Read(ref _updatedCount);
        [DisplayOrder(40)][GridWidth(125)] public int NoChange => Volatile.Read(ref _noChangeCount);
        [DisplayOrder(50)][GridWidth(125)]
        public int Errors { get => Volatile.Read(ref _errors); set => Interlocked.Exchange(ref _errors, value); }
        [DisplayOrder(60)][GridWidth(125)] public int FieldsToRetry => Volatile.Read(ref _fieldsForRetryCount);

        public event PropertyChangedEventHandler PropertyChanged;

        // Post a single property change to the UI context if available
        private void PostNotify(string prop)
        {
            if (_uiContext != null)
                _uiContext.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)), null);
            else
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        // Post multiple property changes in one UI callback
        private void PostNotify(params string[] props)
        {
            if (_uiContext != null)
            {
                _uiContext.Post(_ =>
                {
                    foreach (var p in props)
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
                }, null);
            }
            else
            {
                foreach (var p in props)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
            }
        }

        // Add an error count (quick, non-blocking)
        public void AddError()
        {
            Enqueue(() =>
            {
                _errors++;
                PostNotify(nameof(Errors));
            });
        }

        // Best-effort check on worker-mutated hash sets
        private static bool VolatileReadHashContains(HashSet<string> set, string id)
        {
            try { return set.Contains(id); } catch { return false; }
        }

        // Dispose/stop worker
        public void Dispose()
        {
            try { _queue.CompleteAdding(); _worker.Wait(2000); } catch { }
        }
    }
}