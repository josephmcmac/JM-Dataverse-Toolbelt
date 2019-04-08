using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JosephM.Application.ViewModel.Attributes
{
    public abstract class BulkAddFunction : Attribute
    {
        public abstract Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference);

        public abstract Type TargetPropertyType { get; }

        public static RecordEntryViewModelBase GetEntryViewModel(RecordEntryViewModelBase recordForm)
        {
            return recordForm;
        }

        public PropertyInfo GetTargetProperty(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            PropertyInfo targetPropertyInfo = null;
            foreach (var enumeratedTypeProperty in GetEnumeratedType(recordForm, subGridReference).GetProperties())
            {
                if (enumeratedTypeProperty.PropertyType == TargetPropertyType)
                {
                    targetPropertyInfo = enumeratedTypeProperty;
                }
            }

            return targetPropertyInfo;
        }

        public static Type GetEnumeratedType(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var theRecord = GetEntryViewModel(recordForm).GetRecord() as ObjectRecord;
            if (theRecord == null)
                throw new NullReferenceException(string.Format("Expected Object Of Type {0}. Actual Type Is {1}", typeof(ObjectRecord).Name, GetEntryViewModel(recordForm).GetRecord()?.GetType()?.Name ?? "(null)"));
            var theObject = theRecord.Instance;

            var type = theObject.GetType();
            var property = type.GetProperty(subGridReference);
            var enumeratedType = property.PropertyType.GetGenericArguments()[0];
            return enumeratedType;
        }

        public IRecordService GetLookupService(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var reference = string.Format("{0}{1}", (recordForm.ParentFormReference == null ? null : recordForm.ParentFormReference + "."), subGridReference);

            return recordForm.RecordService.GetLookupService(GetTargetProperty(recordForm, subGridReference).Name, GetEnumeratedType(recordForm, subGridReference).FullName, reference, recordForm.GetRecord());
        }

        public void InsertNewItem(RecordEntryViewModelBase recordForm, string subGridReference, IRecord recordToInsert)
        {
            //if the grid field is a subgrid then we add to the grid
            //otherwise if it is within a subgrid, it is just a display string
            //in which case lets just add it to the object
            var enumerableField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);

            if (enumerableField.DynamicGridViewModel == null)
            {
                var objectRecord = recordToInsert as ObjectRecord;
                if (objectRecord == null)
                    throw new ArgumentOutOfRangeException(nameof(recordToInsert), string.Format("Expected Object of Type {0}. Actual Type Was {1}", typeof(ObjectRecord).Name, recordToInsert?.GetType()?.Name ?? "(null)"));
                var items = new List<object>();
                if (enumerableField.ValueObject as IEnumerable != null)
                {
                    foreach (var item in enumerableField.ValueObject as IEnumerable)
                        items.Add(item);
                }
                items.Add(objectRecord.Instance);
                enumerableField.Value = (IEnumerable)GetEnumeratedType(recordForm, subGridReference).ToNewTypedEnumerable(items);
            }
            else
            {
                var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
                enumerableField.InsertRecord(recordToInsert, 0);
            }
        }
    }
}