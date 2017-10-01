using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;
using System.Reflection;

namespace JosephM.Application.ViewModel.Attributes
{
    public abstract class BulkAddFunction : Attribute
    {
        public string GetFunctionLabel()
        {
            return "Add Multiple";
        }

        public abstract Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference);

        public abstract Type TargetPropertyType { get; }

        public IRecordService GetLookupService(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return recordForm.RecordService.GetLookupService(GetTargetProperty(recordForm, subGridReference).Name, GetEnumeratedType(recordForm, subGridReference).FullName, subGridReference, recordForm.GetRecord());
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
            var theObject = GetObjectFormService(recordForm).GetObject();

            var type = theObject.GetType();
            var property = type.GetProperty(subGridReference);
            var enumeratedType = property.PropertyType.GetGenericArguments()[0];
            return enumeratedType;
        }

        public static ObjectEntryViewModel GetObjectFormService(RecordEntryViewModelBase recordForm)
        {
            var objectFormService = recordForm as ObjectEntryViewModel;
            if (objectFormService == null)
            {
                throw new ArgumentOutOfRangeException(nameof(recordForm), string.Format("Required To Be Type Of {0}. Actual Type Is {1}", typeof(ObjectEntryViewModel).Name, recordForm.GetType().Name));
            }

            return objectFormService;
        }

        public void Load(RecordEntryViewModelBase recordForm, string subGridReference, QueryViewModel childForm)
        {
            var functions = new[]
            {
                    new CustomGridFunction("Return To Input Form", () =>  recordForm.ClearChildForm()),
                    new CustomGridFunction("Add Selected", () =>
                    {
                        foreach(var selectedRow in childForm.DynamicGridViewModel.SelectedRows)
                        {
                            AddSelectedItem(selectedRow, recordForm, subGridReference);
                        }
                        recordForm.ClearChildForm();
                    })
                };
            childForm.DynamicGridViewModel.LoadGridButtons(functions);
            recordForm.LoadChildForm(childForm);
        }

        public abstract void AddSelectedItem(GridRowViewModel gridRow, RecordEntryViewModelBase recordForm, string subGridReference);
    }
}