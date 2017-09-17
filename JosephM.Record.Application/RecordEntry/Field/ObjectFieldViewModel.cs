#region

using System;
using System.Collections.Generic;
using System.Reflection;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Service;
using System.Linq;
using JosephM.Core.AppConfig;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Application.Application;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    //okay so i need to somehow get this type for a settings lookup
    //and have it load the grid objects from the settings

    public class ObjectFieldViewModel : ReferenceFieldViewModel<object>
    {
        public ObjectFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
            bool usePicklist)
            : base(fieldName, fieldLabel, recordForm, usePicklist)
        {
            //okay i need to identify that this is getting the lookups from the settings
            SettingsAttribute = GetSettingLookupAttribute();
            if (SettingsAttribute == null)
            {
                //throw new NotImplementedException(
                //    string.Format(
                //        "The {0} Type Has Only Been Implemented For Object Properties With {1} Attribute. You Will Need To Review Instantiating a Different Type Of View Model For The {2} Type Of You Property {3} Or Extending It For Your Needs",
                //        typeof(ObjectFieldViewModel).Name, typeof(SettingsLookup).Name, RecordTypeToLookup, FieldName));
            }
            else
            {
                var objectRecordService = (ObjectRecordService)recordForm.RecordService;

                var lookupType = objectRecordService.ObjectTypeMaps != null && objectRecordService.ObjectTypeMaps.ContainsKey(FieldName)
                    ? objectRecordService.ObjectTypeMaps[FieldName]
                    : null;

                var settingsObject = objectRecordService.ObjectTypeMaps != null && objectRecordService.ObjectTypeMaps.ContainsKey(SettingsAttribute.PropertyName)
                    ? ApplicationController.ResolveType<ISettingsManager>().Resolve<SavedSettings>(objectRecordService.ObjectTypeMaps[SettingsAttribute.PropertyName])
                    : ApplicationController.ResolveType(SettingsAttribute.SettingsType);
                XrmButton = new XrmButtonViewModel("Search", Search, ApplicationController);
                if (settingsObject != null)
                {
                    _lookupService = new ObjectRecordService(settingsObject, ApplicationController, objectRecordService.ObjectTypeMaps);
                    if (!UsePicklist)
                    {
                        LoadLookupGrid();
                    }
                }
            }
        }

        public SettingsLookup SettingsAttribute { get; set; }

        private IRecordService _lookupService;
        public override IRecordService LookupService
        {
            get { return _lookupService; }
        }

        private SettingsLookup GetSettingLookupAttribute()
        {
            var settingsAttribute = GetPropertyInfo().GetCustomAttribute<SettingsLookup>();
            return settingsAttribute;
        }

        public override void SetValue(IRecord selectedRecord)
        {
            if (selectedRecord == null)
                Value = null;
            else
            {
                Value = ((ObjectRecord)selectedRecord).Instance;
            }
        }

        protected override ReferencePicklistItem MatchSelectedItemInItemsSourceToValue()
        {
            if (ValueObject == null)
                return null;
            else if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    if (((ObjectRecord) item.Record).Instance.ToString() == ValueObject.ToString())
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public override ReferencePicklistItem GetValueAsPicklistItem()
        {
            if (Value == null)
                return null;
            //just use this type of irecord as only for an irecord reference and otherwise may throw type error
            var iReocrd = new RecordObject(RecordTypeToLookup);
            //iReocrd.Id = Value.Id;
            return new ReferencePicklistItem(iReocrd, Value.ToString());
        }

        protected override void MatchValueToSelectedItems()
        {
            object newValue = null;
            if (SelectedItem != null)
                newValue = ((ObjectRecord)SelectedItem.Record).Instance;
            if (newValue != Value)
                Value = newValue;
        }

        protected override IEnumerable<ReferencePicklistItem> GetPicklistOptions()
        {
            return GetSearchResults()
                .Select(r => new ReferencePicklistItem(r, r.GetStringField(LookupService.GetPrimaryField(r.Type))))
                .ToArray();
        }

        protected override IEnumerable<IRecord> GetSearchResults()
        {
            var records = LookupService.GetLinkedRecords(null, null, GetSettingLookupAttribute().PropertyName, null);
            if (!UsePicklist && !EnteredText.IsNullOrWhiteSpace())
                records = records.Where(r => r.GetStringField("ToString") != null && r.GetStringField("ToString").ToLower().StartsWith(EnteredText.ToLower()));
            return records;
        }

        public override string RecordTypeToLookup
        {
            get
            {
                return GetPropertyInfo().PropertyType.Name;
            }
            set
            {
                //unsure what to do maybe refactor other to method and remove
                throw new NotImplementedException();
            }
        }

        private PropertyInfo GetPropertyInfo()
        {
            var record = RecordEntryViewModel.GetRecord();
            if(!(record is ObjectRecord))
                throw new NotImplementedException(string.Format("Only Implemented For {0} Of Type {1}", typeof(IRecord).Name, typeof(ObjectRecord).Name));

            return ((ObjectRecord)record).Instance.GetType().GetProperty(FieldName);
        }

        protected override string GetValueName()
        {
            if (Value == null)
                return null;
            else
                return Value.ToString();
        }
    }
}