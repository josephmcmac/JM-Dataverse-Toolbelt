#region

using System;
using System.Linq;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Application.RecordEntry.Section;
using JosephM.Record.Application.Validation;
using JosephM.Record.IService;
using JosephM.Record.Service;

#endregion

namespace JosephM.Record.Application.RecordEntry.Form
{
    public class ObjectEntryViewModel : RecordEntryFormViewModel
    {
        private readonly ObjectRecord _objectRecord;
        private readonly Action _onCancel;
        private readonly Action _onSave;

        public ObjectEntryViewModel(Action onSave, Action onCancel, object objectToEnter, FormController formController)
            : base(formController)
        {
            _objectRecord = new ObjectRecord(objectToEnter);
            _onSave = onSave;
            _onCancel = onCancel;
            RecordType = _objectRecord.Type;
        }

        protected object GetObject()
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

        public override string SaveButtonLabel
        {
            get { return "Next"; }
        }

        protected override void PreValidateExtention()
        {
            //unload the ienumerable grids into the property
            //should be dynamic if possible set to enumerable query then toarray it when save
            foreach (var grid in SubGrids)
            {
                var typedEnumerable = MapGridToEnumerableValue(grid);
                GetObject().GetType()
                    .GetProperty(grid.ReferenceName)
                    .GetSetMethod()
                    .Invoke(GetObject(), new[] { typedEnumerable });
            }
        }

        public override void OnSaveExtention()
        {
            _onSave();
        }

        private object MapGridToEnumerableValue(GridSectionViewModel grid)
        {
            var objectRecordService = GetObjectRecordService();
            var records = grid.GridRecords.Select(g => g.Record);
            if (records.Any(r => !(r is ObjectRecord)))
                throw new TypeLoadException(string.Format("Expected {0} Of Type {1} Created By {2}",
                    typeof (IRecord).Name, typeof (ObjectRecord).Name, typeof (ObjectRecordService).Name));
            var type = objectRecordService.GetClassType(grid.RecordType);
            var objectEnumerable = records.Cast<ObjectRecord>().Select(r => r.Instance);
            return type.ToNewTypedEnumerable(objectEnumerable);
        }

        public override void OnCanceEntension()
        {
            _onCancel();
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
    }
}