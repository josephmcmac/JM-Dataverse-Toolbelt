#region

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.RecordEntry.Section;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.ObjectMapping;
using JosephM.Record.IService;
using JosephM.Record.Service;
using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Application.ViewModel.Dialog;
using System.Collections.Generic;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Threading;
using System.Reflection;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class ObjectEntryViewModel : RecordEntryFormViewModel
    {
        private ObjectRecord _objectRecord;

        public override int GridPageSize { get { return IsReadOnly ? 25 : 0; } }

        public ObjectEntryViewModel(Action onSave, Action onCancel, object objectToEnter, FormController formController, IDictionary<string, IEnumerable<string>> onlyValidate = null)
            : this(onSave, onCancel, objectToEnter, formController, null, null, onlyValidate)
        {
        }

        public ObjectEntryViewModel(Action onSave, Action onCancel, object objectToEnter, FormController formController, RecordEntryViewModelBase parentForm, string parentFormReference, IDictionary<string, IEnumerable<string>> onlyValidate = null)
            : base(formController, parentForm, parentFormReference, onlyValidate)
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

        public override string SaveButtonLabel
        {
            get { return "Next"; }
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
                GetObject().GetType()
                    .GetProperty(grid.ReferenceName)
                    .GetSetMethod()
                    .Invoke(GetObject(), new[] {typedEnumerable});
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

        public override bool AllowSaveAndLoad
        {
            get
            {
                var objectType = GetObject().GetType();
                return ApplicationController.AllowSaveRequests
                    && objectType.IsTypeOf(typeof(IAllowSaveAndLoad))
                    && !(objectType == typeof(SaveAndLoadFields))
                    && objectType.GetCustomAttribute<AllowSaveAndLoad>() != null;
            }
        }

        internal override void RefreshEditabilityExtention()
        {
            if (FieldViewModels != null)
            {
                foreach (var field in FieldViewModels)
                {
                    var methods = FormService.GetOnLoadTriggers(field.FieldName, RecordType);
                    foreach (var method in methods)
                    {
                        try
                        {
                            method(this);
                        }
                        catch (Exception ex)
                        {
                            ApplicationController.ThrowException(ex);
                        }
                    }
                }
            }
            if (SubGrids != null)
            {
                foreach (var grid in SubGrids)
                {
                    if (grid.IsLoaded && !grid.HasError && grid.DynamicGridViewModel != null && !grid.DynamicGridViewModel.HasError)
                    {
                        foreach (var item in grid.GridRecords)
                        {
                            foreach (var field in item.FieldViewModels)
                            {
                                var methods = FormService.GetOnLoadTriggers(field.FieldName, item.GetRecordType());
                                foreach (var method in methods)
                                {
                                    try
                                    {
                                        method(item);
                                    }
                                    catch (Exception ex)
                                    {
                                        ApplicationController.ThrowException(ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void PostLoading()
        {
            foreach(var field in FieldViewModels)
            {
                var prop = GetObjectRecordService().GetPropertyInfo(field.FieldName, RecordType);
            }
        }
    }
}
