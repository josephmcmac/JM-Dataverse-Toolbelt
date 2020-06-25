using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class ObjectEntryViewModel : RecordEntryFormViewModel
    {
        private ObjectRecord _objectRecord;

        public override int GridPageSize { get { return IsReadOnly ? 25 : 0; } }

        public ObjectEntryViewModel(Action onSave, Action onCancel, object objectToEnter, FormController formController, IDictionary<string, IEnumerable<string>> onlyValidate = null, string saveButtonLabel = null, string cancelButtonLabel = null)
            : this(onSave, onCancel, objectToEnter, formController, null, null, onlyValidate, saveButtonLabel: saveButtonLabel, cancelButtonLabel: cancelButtonLabel)
        {
        }

        public ObjectEntryViewModel(Action onSave, Action onCancel, object objectToEnter, FormController formController, RecordEntryViewModelBase parentForm, string parentFormReference, IDictionary<string, IEnumerable<string>> onlyValidate = null, string saveButtonLabel = null, string cancelButtonLabel = null)
            : base(formController, parentForm, parentFormReference, onlyValidate, saveButtonLabel: saveButtonLabel, cancelButtonLabel: cancelButtonLabel)
        {
            _objectRecord = new ObjectRecord(objectToEnter);
            OnSave = onSave;
            OnCancel = onCancel;
            RecordType = _objectRecord.Type;
        }

        public object GetObject()
        {
            return _objectRecord.Instance;
        }

        public override IRecord GetRecord()
        {
            return _objectRecord;
        }

        public ObjectRecordService GetObjectRecordService()
        {
            var service = RecordService;
            if (!(service is ObjectRecordService))
                throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof (IRecordService).Name,
                    typeof (ObjectRecordService).Name));
            return ((ObjectRecordService) service);
        }

        public ObjectFormService GetObjectFormService()
        {
            var service = FormService;
            if (!(service is ObjectFormService))
                throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(FormServiceBase).Name,
                    typeof(ObjectFormService).Name));
            return ((ObjectFormService)service);
        }

        protected override void PreValidateExtention()
        {
            //unload the ienumerable grids into the property
            //should be dynamic if possible set to enumerable query then toarray it when save
            LoadSubgridsToObject();
        }

        public void LoadSubgridsToObject()
        {
            foreach (var grid in SubGrids)
            {
                var typedEnumerable = MapGridToEnumerableValue(grid);
                var setMethod = GetObject().GetType()
                    .GetProperty(grid.ReferenceName)
                    .GetSetMethod();
                if (setMethod == null)
                    throw new NullReferenceException($"Could not access set method for property '{grid.ReferenceName}' in class '{GetObject().GetType().Name}'. This property needs a public set accessor");
                setMethod.Invoke(GetObject(), new[] {typedEnumerable});
            }
        }

        private object MapGridToEnumerableValue(EnumerableFieldViewModel grid)
        {
            var objectRecordService = GetObjectRecordService();

            try
            {
                var records = grid.DynamicGridViewModel.GridRecords.Select(g => g.Record);
                if (records.Any(r => !(r is ObjectRecord)))
                    throw new TypeLoadException(string.Format("Expected {0} Of Type {1} Created By {2}",
                        typeof(IRecord).Name, typeof(ObjectRecord).Name, typeof(ObjectRecordService).Name));
                var type = objectRecordService.GetClassType(grid.RecordType);
                var objectEnumerable = records.Cast<ObjectRecord>().Select(r => r.Instance);
                return type.ToNewTypedEnumerable(objectEnumerable);
            }
            catch(Exception ex)
            {
                if (grid.DynamicGridViewModel.GridLoadError)
                    throw new NullReferenceException($"There Was An Error Loading The {grid.FieldName} Property's {nameof(EnumerableFieldViewModel.DynamicGridViewModel)}: {grid.DynamicGridViewModel.ErrorMessage}", ex);
                throw;
            }
        }

        public override IsValidResponse ValidateFinal()
        {
            var theObject = GetObject();
            if (theObject.GetType().IsTypeOf(typeof (IValidatableObject)))
            {
                return ((IValidatableObject) theObject).Validate();
            }
            return new IsValidResponse();
        }

        protected override void PostLoading()
        {
            foreach(var field in FieldViewModels)
            {
                var prop = GetObjectRecordService().GetPropertyInfo(field.FieldName, RecordType);
            }
        }

        public override string Instruction => IsGridFullScreenForm
            ? null
            : GetObjectRecordService().GetInstruction(RecordType);

        public bool IsGridFullScreenForm { get; set; }
    }
}
