#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.Shared;
using JosephM.Record.IService;
using JosephM.Record.Service;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    //okay so i need to somehow get this type for a settings lookup
    //and have it load the grid objects from the settings

    public class ObjectFieldViewModel : ReferenceFieldViewModel<object>
    {
        public ObjectFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm)
            : this(fieldName, fieldLabel, recordForm, null)
        {
        }
        
        public ObjectFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm, IRecordService lookupService)
            : base(fieldName, fieldLabel, recordForm, lookupService)
        {
            //okay i need to identify that this is getting the lookups from the settings
            var settingsAttribute = GetSettingLookupAttribute();
            if (settingsAttribute == null)
                throw new NotImplementedException(string.Format("The {0} Type Has Only Been Implemented For Object Properties With {1} Attribute. You Will Need To Review Instantiating a Different Type Of View Model For The {2} Type Of You Property {3} Or Extending It For Your Needs", typeof(ObjectFieldViewModel).Name, typeof(SettingsLookup).Name, RecordTypeToLookup, FieldName));
            {
                var settingsObject = ApplicationController.ResolveType(settingsAttribute.SettingsType);
                SetLookupService(new ObjectRecordService(settingsObject));
                //todo better to delay object resolution until lookup in case new one added or something
                //could maybe map when chnage settings
            }
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

        protected override IEnumerable<IRecord> GetSearchResults()
        {
            var records = LookupService.GetLinkedRecords(null, null, GetSettingLookupAttribute().PropertyName, null);
            if (!EnteredText.IsNullOrWhiteSpace())
                records = records.Where(r => r.GetStringField("ToString") != null && r.GetStringField("ToString").ToLower().StartsWith(EnteredText.ToLower()));
            return records;
        }

        //private ObjectEntryViewModel ObjectRecordEntryForm
        //{
        //    get
        //    {
        //        //need to get the type of the parent form and then get the property type
        //        if (RecordEntryViewModel is ObjectEntryViewModel)
        //        {
        //            return (ObjectEntryViewModel)RecordEntryViewModel;
        //        }
        //        else
        //            throw new NotImplementedException(string.Format("Only Implemented For Type Of {0}", typeof(ObjectEntryViewModel).Name));
        //    }
        //}

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