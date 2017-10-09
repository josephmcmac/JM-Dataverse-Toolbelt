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
using JosephM.Application.ViewModel.RecordEntry.Metadata;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    //todo the different lookup classes need more verification scripts
    //settings/readonly/query/owner/grid/lookupgrid etc

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
                var settingsObject = GetSettingsObject();

                XrmButton = new XrmButtonViewModel("Search", Search, ApplicationController);
                if (settingsObject != null)
                {
                    _lookupService = new ObjectRecordService(settingsObject, ApplicationController, GetObjectRecordService().ObjectTypeMaps);
                    if (!UsePicklist)
                    {
                        LoadLookupGrid();
                    }
                }

                NewAction = () =>
                {
                    //todo try/catch

                    var propertyInfo = settingsObject.GetType().GetProperty(SettingsAttribute.PropertyName);
                    var enumerateType = propertyInfo.PropertyType.GenericTypeArguments[0];
                    var newSettingObject = enumerateType.CreateFromParameterlessConstructor();

                    var objectRecordService = new ObjectRecordService(newSettingObject, ApplicationController, null);
                    var formService = new ObjectFormService(newSettingObject, objectRecordService);
                    var formController = new FormController(objectRecordService, formService, ApplicationController);

                    Action onSave = () =>
                    {
                        //add the new item to the permanent settings
                        var settingsManager = ApplicationController.ResolveType<ISettingsManager>();
                        settingsObject = GetSettingsObject();
                        var settingsService = new ObjectRecordService(settingsObject, ApplicationController);
                        var currentSettings = settingsService.RetrieveAll(enumerateType.AssemblyQualifiedName, null)
                            .Select(r => ((ObjectRecord)r).Instance)
                            .ToList();
                        currentSettings.Add(newSettingObject);
                        settingsObject.SetPropertyValue(SettingsAttribute.PropertyName, enumerateType.ToNewTypedEnumerable(currentSettings));
                        settingsManager.SaveSettingsObject(settingsObject);
                        ValueObject = newSettingObject;
                        if (UsePicklist)
                        {
                            //reload the picklist
                            ValueObject = newSettingObject;
                            LoadPicklistItems();
                        }
                        else
                        {
                            SetEnteredTestWithoutClearingValue(ValueObject.ToString());
                        }
                        RecordEntryViewModel.ClearChildForm();
                    };

                    var newSettingForm = new ObjectEntryViewModel(onSave, RecordEntryViewModel.ClearChildForm, newSettingObject, formController);
                    RecordEntryViewModel.LoadChildForm(newSettingForm);
                };
            }
        }

        private object GetSettingsObject()
        {
            return GetObjectRecordService().ObjectTypeMaps != null && GetObjectRecordService().ObjectTypeMaps.ContainsKey(SettingsAttribute.PropertyName)
                ? ApplicationController.ResolveType<ISettingsManager>().Resolve<SavedSettings>(GetObjectRecordService().ObjectTypeMaps[SettingsAttribute.PropertyName])
                : ApplicationController.ResolveType(SettingsAttribute.SettingsType);
        }

        private ObjectRecordService GetObjectRecordService()
        {
            return (ObjectRecordService) GetRecordForm().RecordService;
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