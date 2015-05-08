#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.ObjectMapping;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Section;
using JosephM.Record.Application.Validation;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Service;

#endregion

namespace JosephM.Record.Application.RecordEntry.Metadata
{
    public class ObjectFormService : FormServiceBase
    {
        private FormMetadata _formMetadata;
        private ObjectRecordService ObjectRecordService { get; set; }

        public ObjectFormService(object objectToEnter, ObjectRecordService objectRecordService)
        {
            ObjectToEnter = objectToEnter;
            ObjectRecordService = objectRecordService;
        }

        private Type FormInstanceType
        {
            get { return null; }
        }

        private object ObjectToEnter { get; set; }

        private Type ObjectType
        {
            get { return ObjectToEnter.GetType(); }
        }

        public override FormMetadata GetFormMetadata(string recordType)
        {
            if (_formMetadata == null)
            {
                var formSections = new List<FormSection>();

                var type = ObjectToEnter.GetType();
                var propertyMetadata = RecordMetadataFactory.GetClassFieldMetadata(type);
                var primaryFieldSection = new List<FormFieldMetadata>();
                formSections.Add(new FormFieldSection(type.Name.SplitCamelCase(), primaryFieldSection));
                foreach (var property in propertyMetadata.Where(m => m.Readable || m.Writeable))
                {
                    if (property.FieldType == RecordFieldType.Enumerable)
                    {
                        var thisMetadata = (EnumerableFieldMetadata) property;
                        var gridFields = new List<GridFieldMetadata>();
                        foreach (var field in ObjectRecordService.GetFields(thisMetadata.EnumeratedType))
                        {
                            var propertyInfo = ObjectRecordService.GetPropertyInfo(field, thisMetadata.EnumeratedType);
                            var gridField = new GridFieldMetadata(field);
                            gridField.IsEditable = propertyInfo.CanWrite;
                            var orderAttribute = propertyInfo.GetCustomAttribute<DisplayOrder>();
                            if (orderAttribute != null)
                                gridField.Order = orderAttribute.Order;
                            var widthAttribute = propertyInfo.GetCustomAttribute<GridWidth>();
                            if (widthAttribute != null)
                                gridField.WidthPart = widthAttribute.Width;
                            gridFields.Add(gridField);
                        }
                        var section = new SubGridSection(property.SchemaName.SplitCamelCase(),
                            thisMetadata.EnumeratedType,
                            property.SchemaName, gridFields);
                        formSections.Add(section);
                    }
                    else
                    {
                        primaryFieldSection.Add(new PersistentFormField(property.SchemaName));
                    }
                }

                _formMetadata = new FormMetadata(formSections);
            }
            return _formMetadata;
        }

        protected override Type GetFormInstanceType(string recordType)
        {
            return FormInstanceType;
        }

        public override bool IsFieldInContext(string fieldName, IRecord record)
        {
            if (record is ObjectRecord)
                return ((ObjectRecord) record).Instance.IsInContext(fieldName);
            throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof (IRecord).Name,
                typeof (ObjectRecord).Name));
        }

        public override bool IsSectionInContext(string sectionIdentifier, IRecord record)
        {
            //sections in these forms are for properties of type enumerable
            //so show ifr thaty property (field) is in context
            if (record is ObjectRecord)
            {
                if (ObjectType.GetProperty(sectionIdentifier) != null)
                    return ((ObjectRecord) record).Instance.IsInContext(sectionIdentifier);
                else
                    return true;
            }
            throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(IRecord).Name,
                typeof(ObjectRecord).Name));
        }

        public override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            var validators = new List<ValidationRuleBase>();
            var type = ObjectRecordService.GetPropertyType(fieldName, ObjectType.Name);
            var isValidatable = type.IsTypeOf(typeof (IValidatableObject));

            if(isValidatable)
                validators.Add(new IValidatableObjectValidationRule());
            validators.AddRange(ObjectRecordService.GetValidatorAttributes(fieldName, ObjectType.Name)
                    .Select(va => new PropertyAttributeValidationRule(va)));
            return validators;
        }

        public override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName, string subGridRecordType)
        {
            return ObjectRecordService.GetValidatorAttributes(fieldName, subGridRecordType)
                .Select(va => new PropertyAttributeValidationRule(va));


        }

        public override IEnumerable<ValidationRuleBase> GetSectionValidationRules(string sectionIdentifier)
        {
            if (ObjectType.GetProperty(sectionIdentifier) != null)
            {
                return
                    ObjectRecordService.GetValidatorAttributes(sectionIdentifier, ObjectType.Name)
                        .Select(va => new PropertyAttributeValidationRule(va));
            }
            else
                return new ValidationRuleBase[0];
        }

        internal override IEnumerable<Action<RecordEntryFormViewModel>> GetOnChanges(string fieldName)
        {
            return GetOnChanges(fieldName, ObjectType.Name);
        }

        internal override IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, string recordType)
        {

            var onChanges = new List<Action<RecordEntryViewModelBase>>();
            AppendLookupForChanges(fieldName, recordType, onChanges);
            AppendInitialiseAttributes(fieldName, recordType, onChanges);
            AppendUniqueOnAttributes(fieldName, recordType, onChanges);

            return base.GetOnChanges(fieldName, recordType).Union(onChanges);
        }

        private void AppendUniqueOnAttributes(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType).GetCustomAttribute<UniqueOn>();
            if(lookupForAttributes != null)
            {
                onChanges.Add(
                    re => re.StartNewAction(() =>
                    {
                        //just need to if this in a grid then set all others off
                        if (re is GridRowViewModel)
                        {
                            var gridRowViewModel = (GridRowViewModel) re;
                            var fieldViewModel = gridRowViewModel.GetFieldViewModel(fieldName) as BooleanFieldViewModel;
                            if (fieldViewModel != null && fieldViewModel.Value)
                            {
                                foreach (var row in gridRowViewModel.GridViewModel.GridRecords.ToArray())
                                {
                                    if (row != gridRowViewModel)
                                    {
                                        ((BooleanFieldViewModel) row.GetFieldViewModel(fieldName)).Value = false;
                                    }
                                }
                            }
                        }
                    }));
            }
        }

        private void AppendInitialiseAttributes(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            foreach (var property in ObjectRecordService.GetFields(recordType))
            {
                var propertyInfo = ObjectRecordService.GetPropertyInfo(property, recordType);
                var initialiseForAttributes = propertyInfo
                    .GetCustomAttributes<InitialiseFor>()
                    .Where(a => a.PropertyDependency == fieldName);
                if (initialiseForAttributes.Any())
                {
                    onChanges.Add((re) =>
                    {
                        foreach (var initialiseForAttribute in initialiseForAttributes)
                        {
                            var dependencyViewModel = re.GetFieldViewModel(fieldName);
                            var dependantViewModel = re.GetFieldViewModel(propertyInfo.Name);
                            if (dependencyViewModel.ValueObject != null
                                && dependencyViewModel.ValueObject.Equals(initialiseForAttribute.ForValue)
                                && dependantViewModel.ValueObject.IsEmpty())
                                dependantViewModel.ValueObject = initialiseForAttribute.InitialValue;
                        }
                    });
                }
            }
        }

        private void AppendLookupForChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType)
                .GetCustomAttributes(typeof (RecordTypeFor), true).Cast<RecordTypeFor>();
            foreach (var attribute in lookupForAttributes)
            {
                onChanges.Add(
                    re => re.StartNewAction(() =>
                    {
                        var recordTypeViewModel = re.GetFieldViewModel(fieldName) as RecordTypeFieldViewModel;
                        if (recordTypeViewModel == null)
                            throw new Exception("Expected On Change For Field Of Type Field Of Type: " +
                                                typeof (LookupFieldViewModel).Name);
                        var fieldViewModel = re.GetFieldViewModel(attribute.LookupProperty);
                        if (fieldViewModel is LookupFieldViewModel)
                        {
                            var typedViewModel = (LookupFieldViewModel) fieldViewModel;
                            var selectedRecordType = recordTypeViewModel.Value == null
                                ? null
                                : recordTypeViewModel.Value.Key;
                            typedViewModel.RecordTypeToLookup = selectedRecordType;
                            typedViewModel.Value = null;
                        }
                        else if (fieldViewModel is RecordFieldFieldViewModel)
                        {
                            var typedViewModel = (RecordFieldFieldViewModel) fieldViewModel;
                            var selectedRecordType = recordTypeViewModel.Value == null
                                ? null
                                : recordTypeViewModel.Value.Key;
                            typedViewModel.RecordTypeForField = selectedRecordType;
                        }
                    }));
            }
        }

        internal override RecordEntryFormViewModel GetLoadRowViewModel(string subGridName, FormController formController, Action<IRecord> onSave, Action onCancel)
        {
            var propertyInfo = ObjectToEnter.GetType().GetProperty(subGridName);
            if (propertyInfo.GetCustomAttribute<FormEntry>() != null)
            {
                //lets start a dialog to add it on complete
                var newRecord = (ObjectRecord)ObjectRecordService.NewRecord(propertyInfo.PropertyType.GetGenericArguments()[0].Name);
                var newObject = newRecord.Instance;
                var recordService = new ObjectRecordService(newObject);
                var viewModel = new ObjectEntryViewModel(
                    () => onSave(new ObjectRecord(newObject)),
                    onCancel,

                    newObject, new FormController(recordService, new ObjectFormService(newObject, recordService), formController.ApplicationController));
                return viewModel;
                //ideally could hide the parent dialog temporarily and load this one
            }
                //if the object specifies use a form then use the form/dialog
            else
                return null;
        }

        internal override RecordEntryFormViewModel GetEditRowViewModel(string subGridName, FormController formController, Action<IRecord> onSave, Action onCancel, GridRowViewModel gridRow)
        {
            var record = gridRow.GetRecord();
            if(!(record is ObjectRecord))
                throw new NotSupportedException(string.Format("Error Expected Object Of Type {0}", typeof(ObjectRecord).Name));
            var newRecord = (ObjectRecord) record;
            //need to load the exitsing row to this
            //lets start a dialog to add it on complete
            var mapper = new ClassSelfMapper();
            var newObject = mapper.Map(newRecord.Instance);
            var recordService = new ObjectRecordService(newObject, ObjectRecordService.LookupService, ObjectRecordService.OptionSetLimitedValues);
            var viewModel = new ObjectEntryViewModel(
                () => onSave(new ObjectRecord(newObject)),
                onCancel,
                newObject, new FormController(recordService, new ObjectFormService(newObject, recordService), formController.ApplicationController));
            return viewModel;
        }
    }
}