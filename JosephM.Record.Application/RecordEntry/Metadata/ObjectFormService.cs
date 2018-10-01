using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Record.Attributes;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class ObjectFormService : FormServiceBase
    {
        private FormMetadata _formMetadata;
        private ObjectRecordService ObjectRecordService { get; set; }

        public ObjectFormService(object objectToEnter, ObjectRecordService objectRecordService, IDictionary<string, Type> objectTypeMaps = null, IEnumerable<string> limitFields = null)
        {
            ObjectToEnter = objectToEnter;
            ObjectRecordService = objectRecordService;
            ObjectTypeMaps = objectTypeMaps;
            LimitFields = limitFields;
        }

        public object ObjectToEnter { get; set; }

        private Type ObjectType
        {
            get { return ObjectToEnter.GetType(); }
        }

        public IDictionary<string, Type> ObjectTypeMaps { get; private set; }
        public IEnumerable<string> LimitFields { get; }

        public override FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null)
        {
            if (_formMetadata == null)
            {
                var formSections = new List<FormFieldSection>();

                var type = ObjectToEnter.GetType();
                var propertyMetadata = ObjectRecordService.GetFieldMetadata(type.AssemblyQualifiedName);

                var standardFieldSectionName = type.GetDisplayName();

                var fieldSections = type.GetCustomAttributes<Group>();
                var otherSections = new Dictionary<string, List<FormFieldMetadata>>();
                foreach (var section in fieldSections)
                {
                    var functions = new List<CustomFormFunction>();
                    if (section.SelectAll)
                    {
                        functions.Add(new CustomFormFunction("SELECTALL", "SELECT ALL", (re) =>
                        {
                            var entryForm = re as RecordEntryFormViewModel;
                            if (entryForm != null)
                            {
                                var thisSection = entryForm.GetFieldSection(section.Name);
                                var booleanFields = thisSection.Fields.Where(f => f is BooleanFieldViewModel).Cast<BooleanFieldViewModel>();
                                var turnOff = booleanFields.All(b => b.Value);
                                foreach (var field in booleanFields)
                                    field.Value = !turnOff;

                                var enumerableFields = thisSection.Fields.Where(f => f is EnumerableFieldViewModel).Cast<EnumerableFieldViewModel>();
                                foreach (var field in enumerableFields)
                                {
                                    if (ObjectRecordService.GetClassType(field.RecordType).IsTypeOf(typeof(ISelectable)))
                                    {
                                        turnOff = field.GridRecords.All(r => r.GetBooleanFieldFieldViewModel(nameof(ISelectable.Selected)).Value);
                                        foreach (var record in field.GridRecords)
                                        {
                                            record.GetBooleanFieldFieldViewModel(nameof(ISelectable.Selected)).Value = !turnOff;
                                        }
                                    }
                                }
                            }
                        }));
                    }
                    otherSections[section.Name] = new List<FormFieldMetadata>();
                    var newSection = new FormFieldSection(section.Name, otherSections[section.Name], section.DisplayLayout, section.Order, customFunctions: functions, displayLabel: section.DisplayLabel);
                    formSections.Add(newSection);
                }

                foreach (var property in propertyMetadata.Where(m => m.Readable || m.Writeable))
                {
                    if (LimitFields != null && !LimitFields.Contains(property.SchemaName))
                        continue;
                    var propinfo = ObjectRecordService.GetPropertyInfo(property.SchemaName, type.AssemblyQualifiedName);
                    var groupAttribute = propinfo.GetCustomAttribute<Group>();

                    string enumerableType = null;
                    if (property is EnumerableFieldMetadata)
                        enumerableType = ((EnumerableFieldMetadata)property).EnumeratedTypeQualifiedName;

                    var displayLabel = property.FieldType != RecordFieldType.Enumerable
                        || groupAttribute != null;

                    var fieldMetadata = new PersistentFormField(property.SchemaName, enumerableType, displayLabel);
                    fieldMetadata.DoNotLimitDisplayHeight = propinfo.GetCustomAttribute<DoNotLimitDisplayHeight>() != null;
                    fieldMetadata.Order = property.Order;

                    string sectionName = null;
                    if (groupAttribute != null)
                        sectionName = groupAttribute.Name;
                    else if (property.FieldType == RecordFieldType.Enumerable)
                        sectionName = property.DisplayName;
                    else
                        sectionName = standardFieldSectionName;

                    var order = 0;
                    if (groupAttribute != null)
                        order = groupAttribute.Order;
                    else if (property.FieldType == RecordFieldType.Enumerable)
                        order = 200000;
                    else
                        order = 1;

                    var displayLayout = groupAttribute != null
                        ? groupAttribute.DisplayLayout
                        : Group.DisplayLayoutEnum.VerticalList;

                    if (!otherSections.ContainsKey(sectionName))
                    {
                        otherSections[sectionName] = new List<FormFieldMetadata>();
                        var newSection = new FormFieldSection(sectionName, otherSections[sectionName], displayLayout, order);
                        formSections.Add(newSection);
                    }
                    otherSections[sectionName].Add(fieldMetadata);
                }
                foreach (var section in formSections)
                {
                    var fields = section.FormFields;
                    if (section.DisplayLayout == Group.DisplayLayoutEnum.HorizontalInputOnly)
                    {
                        foreach (var field in fields)
                        {
                            field.DisplayLabel = false;
                        }
                    }
                    if (fields.Count() == 1)
                    {
                        var field = fields.First();
                        var fieldMt = ObjectRecordService.GetFieldMetadata(field.FieldName, type.AssemblyQualifiedName);
                        if (fieldMt is EnumerableFieldMetadata
                            || fieldMt is MemoFieldMetadata
                            || (fieldMt is StringFieldMetadata && fieldMt.IsMultiline()))
                            field.DisplayLabel = false;
                    }
                }
                formSections = formSections.OrderBy(s => s.Order).ToList();
                _formMetadata = new FormMetadata(formSections);
            }
            return _formMetadata;
        }

        public override bool IsFieldInContext(string fieldName, IRecord record)
        {
            if (record is ObjectRecord)
                return ((ObjectRecord)record).Instance.IsInContext(fieldName);
            throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(IRecord).Name,
                typeof(ObjectRecord).Name));
        }

        public override bool IsSectionInContext(string sectionIdentifier, IRecord record)
        {
            //sections in these forms are for properties of type enumerable
            //so show ifr thaty property (field) is in context
            if (record is ObjectRecord)
            {
                if (ObjectType.GetProperty(sectionIdentifier) != null)
                    return ((ObjectRecord)record).Instance.IsInContext(sectionIdentifier);
                else
                    return true;
            }
            throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(IRecord).Name,
                typeof(ObjectRecord).Name));
        }

        public override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName, string recordType)
        {
            var validators = new List<ValidationRuleBase>();
            var type = ObjectRecordService.GetPropertyType(fieldName, ObjectType.AssemblyQualifiedName);
            var isValidatable = type.IsTypeOf(typeof(IValidatableObject));

            if (isValidatable)
                validators.Add(new IValidatableObjectValidationRule());
            validators.AddRange(ObjectRecordService.GetValidatorAttributes(fieldName, ObjectType.AssemblyQualifiedName)
                    .Select(va => new PropertyAttributeValidationRule(va)));
            return validators;
        }

        public override IEnumerable<ValidationRuleBase> GetSubgridValidationRules(string fieldName, string subGridRecordType)
        {
            return ObjectRecordService.GetValidatorAttributes(fieldName, subGridRecordType)
                .Select(va => new PropertyAttributeValidationRule(va));
        }

        public override IEnumerable<ValidationRuleBase> GetSectionValidationRules(string sectionIdentifier)
        {
            if (ObjectType.GetProperty(sectionIdentifier) != null)
            {
                return
                    ObjectRecordService.GetValidatorAttributes(sectionIdentifier, ObjectType.AssemblyQualifiedName)
                        .Select(va => new PropertyAttributeValidationRule(va));
            }
            else
                return new ValidationRuleBase[0];
        }

        internal override IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, RecordEntryViewModelBase entryViewModel)
        {
            return GetOnChanges(fieldName, ObjectType.AssemblyQualifiedName, entryViewModel);
        }

        internal override IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, string recordType, RecordEntryViewModelBase entryViewModel)
        {
            var onChanges = new List<Action<RecordEntryViewModelBase>>();
            AppendRecordTypeForChanges(fieldName, recordType, onChanges);
            AppendConnectionForChanges(fieldName, recordType, onChanges, false);
            AppendInitialiseAttributes(fieldName, recordType, onChanges);
            AppendUniqueOnAttributes(fieldName, recordType, onChanges);
            AppendReadOnlyWhenSetAttributes(fieldName, recordType, onChanges);
            AppendDisplayNameAttributes(fieldName, recordType, onChanges);
            AppendLookupFieldCascadeChanges(fieldName, recordType, onChanges);
            AppendCascadeOnChanges(fieldName, recordType, onChanges);
            AppendFieldForChanges(fieldName, recordType, onChanges, clearValue: true);
            AppendOnChangeFunctions(fieldName, recordType, onChanges, entryViewModel);
            return base.GetOnChanges(fieldName, recordType, entryViewModel).Union(onChanges);
        }

        private void AppendOnChangeFunctions(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges, RecordEntryViewModelBase entryViewModel)
        {
            var type = ObjectRecordService.GetClassType(recordType);
            var injectedFunctions = entryViewModel.ApplicationController.ResolveInstance(typeof(OnChangeFunctions), recordType) as OnChangeFunctions;
            foreach (var func in injectedFunctions.CustomFunctions)
            {
                Action<RecordEntryViewModelBase> onChange = (revm) => func.Execute(revm, fieldName);
                onChanges.Add(onChange);
            }
        }

        private void AppendCascadeOnChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType)
                .GetCustomAttributes(typeof(CascadeOnChange), true).Cast<CascadeOnChange>();
            foreach (var attribute in lookupForAttributes)
            {
                onChanges.Add(
                    re =>
                    {
                        var changedViewModel = re.GetFieldViewModel(fieldName);
                        if (changedViewModel.ValueObject != null)
                        {
                            var matchingFields = re.FieldViewModels.Where(f => f.FieldName == attribute.TargetProperty);
                            if (matchingFields.Any())
                            {
                                matchingFields.First().ValueObject = changedViewModel.ValueObject;
                            }
                        }
                    });
            }
        }

        private void AppendLookupFieldCascadeChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType)
                .GetCustomAttributes(typeof(LookupFieldCascade), true).Cast<LookupFieldCascade>();
            foreach (var attribute in lookupForAttributes)
            {
                onChanges.Add(
                    re =>
                    {
                        var changedViewModel = re.GetFieldViewModel(fieldName);
                        if (changedViewModel.ValueObject != null)
                        {
                            var changedViewModelLookup = ((LookupFieldViewModel)changedViewModel).Value;
                            var matchingFields = re.FieldViewModels.Where(f => f.FieldName == attribute.TargetProperty);
                            var lookupService = ObjectRecordService.GetLookupService(changedViewModel.FieldName, re.GetRecordType(), re.ParentFormReference, re.GetRecord());
                            if (lookupService != null)
                            {
                                var lookupRecord = lookupService.Get(changedViewModelLookup.RecordType, changedViewModelLookup.Id);
                                if (lookupRecord == null)
                                {
                                    changedViewModel.ValueObject = null;
                                    re.ApplicationController.UserMessage(string.Format("The {0} Was Not Found And The Value Has Been Cleared", changedViewModel.Label));
                                }
                                else
                                {
                                    var sourceFieldValue = lookupRecord.GetField(attribute.SourceRecordField);
                                    if (matchingFields.Any())
                                    {
                                        matchingFields.First().ValueObject = sourceFieldValue;
                                    }
                                }
                            }
                        }
                    });
            }
        }

        private void AppendConnectionForChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges, bool isOnLoad)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType)
                .GetCustomAttributes(typeof(ConnectionFor), true).Cast<ConnectionFor>();
            foreach (var attribute in lookupForAttributes)
            {
                onChanges.Add(
                    re =>
                    {
                        var changedViewModel = re.GetFieldViewModel(fieldName);
                        if (changedViewModel.ValueObject != null)
                        {
                            var matchingFields =
                                re.FieldViewModels.Where(f => f.FieldName == attribute.Property);
                            if (matchingFields.Any())
                            {
                                var fieldViewModel = matchingFields.First();
                                if (!isOnLoad && fieldViewModel is LookupFieldViewModel)
                                {
                                    var typedViewModel = (LookupFieldViewModel)fieldViewModel;
                                    typedViewModel.ConnectionForChanged();
                                }
                                if (fieldViewModel is RecordTypeFieldViewModel)
                                {
                                    var typedViewModel = (RecordTypeFieldViewModel)fieldViewModel;

                                    typedViewModel.ItemsSource = ObjectRecordService
                                        .GetPicklistKeyValues(fieldViewModel.FieldName,
                                            fieldViewModel.GetRecordType(), fieldViewModel.RecordEntryViewModel.ParentFormReference, fieldViewModel.RecordEntryViewModel.GetRecord())
                                        .Select(p => new RecordType(p.Key, p.Value))
                                        .Where(rt => !rt.Value.IsNullOrWhiteSpace())
                                        .OrderBy(rt => rt.Value)
                                        .ToArray();
                                }
                            }
                        }
                    });
            }
        }

        internal override IEnumerable<Action<RecordEntryViewModelBase>> GetOnLoadTriggers(string fieldName, string recordType)
        {
            var methods = new List<Action<RecordEntryViewModelBase>>();
            AppendReadOnlyWhenSetAttributes(fieldName, recordType, methods);
            AppendDisplayNameAttributes(fieldName, recordType, methods);
            AppendConnectionForChanges(fieldName, recordType, methods, true);
            AppendLookupFieldCascadeChanges(fieldName, recordType, methods);
            AppendSubGridButtons(fieldName, recordType, methods);
            AppendFieldForChanges(fieldName, recordType, methods);
            return methods;
        }

        private void AppendSubGridButtons(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> methods)
        {
            var fieldMetadata = ObjectRecordService.GetFieldMetadata(fieldName, recordType);
            if (fieldMetadata.FieldType == RecordFieldType.Enumerable)
            {
                methods.Add(
                    re => re.StartNewAction(() =>
                    {
                        if (re is RecordEntryFormViewModel)
                        {
                            var refvm = re as RecordEntryFormViewModel;
                            var customFunctions = GetCustomFunctionsFor(fieldName, refvm).ToList();
                            var fieldVm = refvm.GetEnumerableFieldViewModel(fieldName);
                            fieldVm.DynamicGridViewModel.AddGridButtons(customFunctions);
                        }
                    }));
            }

        }

        private void AppendReadOnlyWhenSetAttributes(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var attributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType).GetCustomAttribute<ReadOnlyWhenSet>();
            if (attributes != null)
            {
                onChanges.Add(
                    re => re.StartNewAction(() =>
                    {
                        //just need to if this in a grid then set all others off
                        var fieldViewModel = re.GetFieldViewModel(fieldName);
                        if (fieldViewModel != null)
                        {
                            fieldViewModel.IsEditable = fieldViewModel.ValueObject == null;
                        }
                    }));
            }
        }

        public override bool AllowAddNew(string fieldName, string recordType)
        {
            var prop = GetPropertyInfo(fieldName, recordType);
            return prop.GetCustomAttribute<DoNotAllowAdd>() == null;
        }

        public override bool AllowDelete(string fieldName, string recordType)
        {
            var prop = GetPropertyInfo(fieldName, recordType);
            return prop.GetCustomAttribute<DoNotAllowDelete>() == null;
        }

        public override bool AllowGridOpen(string fieldName, RecordEntryViewModelBase recordForm)
        {
            var prop = GetPropertyInfo(fieldName, recordForm.GetRecordType());
            if (prop.PropertyType.GenericTypeArguments.Count() == 1
                && prop.PropertyType.GenericTypeArguments[0].GetCustomAttribute<DoNotAllowGridOpen>() != null)
                return false;
            return
                prop.GetCustomAttribute<DoNotAllowGridOpen>() == null;
        }

        public override bool UsePicklist(string fieldName, string recordType)
        {
            var prop = GetPropertyInfo(fieldName, recordType);
            return prop.GetCustomAttribute<UsePicklist>() != null;
        }

        private void AppendUniqueOnAttributes(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var attributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType).GetCustomAttribute<UniqueOn>();
            if (attributes != null)
            {
                onChanges.Add(
                    re => re.StartNewAction(() =>
                    {
                        //just need to if this in a grid then set all others off
                        if (re is GridRowViewModel)
                        {
                            var gridRowViewModel = (GridRowViewModel)re;
                            var fieldViewModel = gridRowViewModel.GetFieldViewModel(fieldName) as BooleanFieldViewModel;
                            if (fieldViewModel != null && fieldViewModel.Value)
                            {
                                foreach (var row in gridRowViewModel.GridViewModel.GridRecords.ToArray())
                                {
                                    if (row != gridRowViewModel)
                                    {
                                        ((BooleanFieldViewModel)row.GetFieldViewModel(fieldName)).Value = false;
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
                                && (initialiseForAttribute.AlwaysSetIfNotEmpty || dependantViewModel.ValueObject.IsEmpty()))
                                dependantViewModel.ValueObject = initialiseForAttribute.InitialValue;
                        }
                    });
                }
            }
        }

        private void AppendDisplayNameAttributes(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            foreach (var property in ObjectRecordService.GetFields(recordType))
            {
                var propertyInfo = ObjectRecordService.GetPropertyInfo(property, recordType);
                var attributes = propertyInfo
                    .GetCustomAttributes<DisplayNameForPropertyValueAttribute>()
                    .Where(a => a.Property == fieldName);
                if (attributes.Any())
                {
                    onChanges.Add((re) =>
                    {
                        foreach (var attribute in attributes)
                        {
                            var dependencyViewModel = re.GetFieldViewModel(fieldName);
                            var dependantViewModel = re.GetFieldViewModel(propertyInfo.Name);
                            if (dependencyViewModel.ValueObject != null
                                && dependencyViewModel.ValueObject.Equals(attribute.Value))
                                dependantViewModel.Label = attribute.Label;
                        }
                    });
                }
            }
        }

        private void AppendRecordTypeForChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType)
                .GetCustomAttributes(typeof(RecordTypeFor), true).Cast<RecordTypeFor>();
            foreach (var attribute in lookupForAttributes)
            {
                onChanges.Add(
                    re => re.StartNewAction(() =>
                    {
                        var recordTypeViewModel = re.GetRecordTypeFieldViewModel(fieldName);
                        var matchingFields = re.FieldViewModels.Where(f => f.FieldName == attribute.PropertyPaths.First());
                        if (matchingFields.Any())
                        {
                            var fieldViewModel = matchingFields.First();
                            if (fieldViewModel is LookupFieldViewModel)
                            {
                                var typedViewModel = (LookupFieldViewModel)fieldViewModel;
                                var selectedRecordType = recordTypeViewModel.Value == null
                                    ? null
                                    : recordTypeViewModel.Value.Key;
                                typedViewModel.RecordTypeToLookup = selectedRecordType;
                                typedViewModel.Value = null;
                            }
                            else if (fieldViewModel is RecordFieldFieldViewModel)
                            {
                                var typedViewModel = (RecordFieldFieldViewModel)fieldViewModel;
                                var selectedRecordType = recordTypeViewModel.Value == null
                                    ? null
                                    : recordTypeViewModel.Value.Key;
                                typedViewModel.RecordTypeForField = selectedRecordType;
                            }
                            else if (fieldViewModel is RecordFieldMultiSelectFieldViewModel)
                            {
                                var typedViewModel = (RecordFieldMultiSelectFieldViewModel)fieldViewModel;
                                var selectedRecordType = recordTypeViewModel.Value == null
                                    ? null
                                    : recordTypeViewModel.Value.Key;
                                typedViewModel.RecordTypeForField = selectedRecordType;
                            }
                        }
                        else
                        {
                            if (re is ObjectEntryViewModel)
                            {
                                if (attribute.PropertyPaths.Count() < 2)
                                    throw new NullReferenceException(string.Format("The {0} Attribute References an Enumerable Property But Does Not Specify The Property On The Enumerated Type. The Value Is {1} And Should Be Of Form Property1.Property2", typeof(RecordTypeFor).Name, attribute.LookupProperty));
                                var oevm = (ObjectEntryViewModel)re;
                                var matchingGrids = oevm.SubGrids.Where(sg => sg.ReferenceName == attribute.PropertyPaths.First());
                                if (matchingGrids.Any())
                                {
                                    //clear the rows as they are no longer relevant for the change in type
                                    matchingGrids.First().ClearRows();
                                }
                            }
                        }
                    }));
            }
        }

        private void AppendFieldForChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges, bool clearValue = false)
        {
            var lookupForAttributes = ObjectRecordService.GetPropertyInfo(fieldName, recordType)
                .GetCustomAttributes(typeof(RecordFieldFor), true).Cast<RecordFieldFor>();
            foreach (var attribute in lookupForAttributes)
            {
                onChanges.Add(
                    re => re.StartNewAction(() =>
                    {
                        var recordFieldViewModel = re.GetRecordFieldFieldViewModel(fieldName);
                        if (recordFieldViewModel.Value != null)
                        {
                            var matchingFields = re.FieldViewModels.Where(f => f.FieldName == attribute.PropertyPaths.First());
                            foreach (var fieldViewModel in matchingFields.ToArray())
                            {
                                var selectedFieldName = recordFieldViewModel.Value.Key;
                                var selectedFieldRecordType = GetRecordTypeFor(recordFieldViewModel.FieldName, re);
                                var lookupService = re.RecordService.GetLookupService(recordFieldViewModel.FieldName, recordFieldViewModel.GetRecordType(), null, recordFieldViewModel.RecordEntryViewModel.GetRecord());
                                if (lookupService != null)
                                {
                                    //get the source field type
                                    var fieldType = lookupService.GetFieldType(selectedFieldName, selectedFieldRecordType);
                                    //get the section the target field is in and its field metadata
                                    var metadata = re.FormService.GetFormMetadata(re.GetRecordType(), ObjectRecordService);
                                    FormFieldMetadata fieldMetadata = null;
                                    string sectionName = null;
                                    foreach (var sectionMetadata in metadata.FormSections.Cast<FormFieldSection>())
                                    {
                                        foreach (var field in sectionMetadata.FormFields)
                                        {
                                            if (field.FieldName == fieldViewModel.FieldName)
                                            {
                                                sectionName = sectionMetadata.SectionLabel;
                                                fieldMetadata = field;
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrWhiteSpace(sectionName))
                                    {
                                        var targetPropInfo = ObjectRecordService.GetPropertyType(attribute.PropertyPaths.First(), recordType);
                                        if (targetPropInfo.IsEnum)
                                        {
                                            //if target is an enum then filter the items source for the field type
                                            var picklistFieldViewModel = fieldViewModel as PicklistFieldViewModel;
                                            var picklistOptions = ObjectRecordService.GetPicklistKeyValues(picklistFieldViewModel.FieldName, picklistFieldViewModel.GetRecordType(), fieldType.ToString(), picklistFieldViewModel.RecordEntryViewModel.GetRecord());
                                            if (clearValue)
                                                picklistFieldViewModel.Value = null;
                                            picklistFieldViewModel.ItemsSource = picklistOptions;
                                        }
                                        else
                                        {
                                            //otherwise is a field input dynamic for the field's tpye
                                            //now we need to create the view model for the target field as the correct type
                                            //okay now we need to replace the old field view model for this field
                                            var explicitTargetType = fieldType == RecordFieldType.Lookup || fieldType == RecordFieldType.Customer || fieldType == RecordFieldType.Owner
                                                ? lookupService.GetLookupTargetType(selectedFieldName, selectedFieldRecordType)
                                                : null;
                                            var explicitPicklistOptions = fieldType == RecordFieldType.Picklist
                                                                        || fieldType == RecordFieldType.Status
                                                                        || fieldType == RecordFieldType.State
                                                                        || fieldType == RecordFieldType.Integer
                                                                        || fieldType == RecordFieldType.RecordType
                                                ? lookupService.GetPicklistKeyValues(selectedFieldName, selectedFieldRecordType)
                                                : null;
                                            if (clearValue)
                                                fieldViewModel.ValueObject = null;
                                            var newFieldViewModel = fieldMetadata.CreateFieldViewModel(re.GetRecordType(), re.RecordService, re, re.ApplicationController, explicitFieldType: fieldType, explicitLookupTargetType: explicitTargetType, explicitPicklistOptions: explicitPicklistOptions);
                                            var section = re.FieldSections.First(s => s.SectionLabel == sectionName);
                                            var index = section.Fields.Count;
                                            for (var i = 0; i < section.Fields.Count; i++)
                                            {
                                                if (section.Fields[i].FieldName == fieldViewModel.FieldName)
                                                {
                                                    index = i;
                                                    break;
                                                }
                                            }

                                            re.DoOnMainThread(() =>
                                            {
                                                if (section.Fields.Count > index)
                                                    section.Fields.RemoveAt(index);

                                                if (clearValue)
                                                    fieldViewModel.ValueObject = null;
                                                if (newFieldViewModel is BooleanFieldViewModel)
                                                    newFieldViewModel.ValueObject = false;
                                                section.Fields.Insert(index, newFieldViewModel);
                                                re.RefreshVisibility();
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }));
            }
        }

        internal override string GetDependantValue(string field, string recordType, RecordEntryViewModelBase viewModel)
        {
            var propertyInfo = GetPropertyInfo(field, viewModel.GetRecord().Type);
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(FileReference))
            {
                var attr = propertyInfo.GetCustomAttribute<FileMask>();
                return attr == null ? null : attr.Mask;
            }
            else
            {
                return GetRecordTypeFor(field, viewModel);
            }
        }

        private string GetRecordTypeFor(string field, RecordEntryViewModelBase viewModel)
        {
            var split = field?.Split('.') ?? new string[0];
            if (!split.Any())
                return null;
            var propertyInfo = GetPropertyInfo(split.First(), viewModel.GetRecord().Type);
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType.Name == "IEnumerable`1" && split.Count() > 1)
                {
                    var enumeratedType = propertyInfo.PropertyType.GenericTypeArguments[0];
                    propertyInfo = enumeratedType.GetProperty(split.ElementAt(1)) ?? propertyInfo;
                }
                var referenceAttributes = propertyInfo.GetCustomAttributes<ReferencedType>();
                if (referenceAttributes != null && referenceAttributes.Any())
                    return string.Join(",", referenceAttributes.Select(attribute => attribute.Type));
            }
            foreach (var parentField in ObjectRecordService.GetPropertyInfos(viewModel.GetRecordType()))
            {
                var lookupForAttributes =
                    parentField.GetCustomAttributes(typeof(RecordTypeFor), true).Cast<RecordTypeFor>();
                foreach (var lookupForAttribute in lookupForAttributes)
                {
                    if (lookupForAttribute.LookupProperty == field)
                    {
                        //can't use the field view model as called on load and may trigger infinite loop loading field list
                        var record = viewModel.GetRecord();
                        if (record is ObjectRecord)
                        {
                            var objectrecord = (ObjectRecord)record;
                            var propertyValue = objectrecord.Instance.GetPropertyValue(parentField.Name);
                            if (propertyValue is RecordType)
                                return ((RecordType)propertyValue).Key;
                        }
                    }
                }
            }
            var parentForm = viewModel.ParentForm;
            if (parentForm is ObjectEntryViewModel)
            {
                foreach (var parentField in ((ObjectEntryViewModel)parentForm).GetObject().GetType().GetProperties())
                {
                    var lookupForAttributes =
                        parentField.GetCustomAttributes(typeof(RecordTypeFor), true).Cast<RecordTypeFor>();
                    foreach (var lookupForAttribute in lookupForAttributes)
                    {
                        if (lookupForAttribute.PropertyPaths.Count() == 2 &&
                            lookupForAttribute.PropertyPaths.First() == viewModel.ParentFormReference &&
                            lookupForAttribute.PropertyPaths.Last() == field)
                        {
                            var parentObjectRecord = parentForm.GetRecord() as ObjectRecord;
                            if (parentObjectRecord != null)
                            {
                                var recordTypeFor = parentObjectRecord.Instance.GetPropertyValue(parentField.Name) as RecordType;
                                if (recordTypeFor != null)
                                    return recordTypeFor.Key;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private PropertyInfo GetPropertyInfo(string field, string type)
        {
            var propertyInfo = ObjectRecordService.GetPropertyInfo(field, type);
            return propertyInfo;
        }

        internal override string GetLookupTargetType(string field, string recordType, RecordEntryViewModelBase recordForm)
        {
            return GetRecordTypeFor(field, recordForm);
        }

        internal override RecordEntryFormViewModel GetLoadRowViewModel(string subGridName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave, Action onCancel)
        {
            var propertyInfo = ObjectToEnter.GetType().GetProperty(subGridName);
            if (propertyInfo.GetCustomAttribute<FormEntry>() != null)
            {
                //lets start a dialog to add it on complete
                var fieldMetadata = (EnumerableFieldMetadata)ObjectRecordService.GetFieldMetadata(propertyInfo.Name, ObjectToEnter.GetType().AssemblyQualifiedName);
                var newRecord = (ObjectRecord)ObjectRecordService.NewRecord(fieldMetadata.EnumeratedTypeQualifiedName);
                var newObject = newRecord.Instance;
                var recordService = new ObjectRecordService(newObject, ObjectRecordService.LookupService, ObjectRecordService.OptionSetLimitedValues, ObjectRecordService, subGridName, parentForm.ApplicationController);
                var formService = new ObjectFormService(newObject, recordService);
                formService.AllowLookupFunctions = AllowLookupFunctions;
                var viewModel = new ObjectEntryViewModel(
                    () => onSave(new ObjectRecord(newObject)),
                    onCancel,
                    newObject, new FormController(recordService, formService, parentForm.FormController.ApplicationController), parentForm, subGridName, parentForm.OnlyValidate
                    , saveButtonLabel: "Add");
                return viewModel;
                //ideally could hide the parent dialog temporarily and load this one
            }
            //if the object specifies use a form then use the form/dialog
            else
                return null;
        }

        public override bool AllowGridFieldEditEdit(string fieldName)
        {
            var propertyInfo = ObjectToEnter.GetType().GetProperty(fieldName);
            return propertyInfo == null || !propertyInfo.PropertyType.GenericTypeArguments.Any() || propertyInfo.PropertyType.GenericTypeArguments[0].GetCustomAttribute<DoNotAllowGridEdit>() == null;
        }

        public override bool AllowNestedGridEdit(string subGridName, string fieldName)
        {
            var gridClass = GetPropertyInfo(subGridName, ObjectType.AssemblyQualifiedName);
            var gridEnumerableProperty = GetPropertyInfo(fieldName, gridClass.PropertyType.GenericTypeArguments[0].AssemblyQualifiedName);
            return gridEnumerableProperty != null && gridEnumerableProperty.GetCustomAttribute<AllowNestedGridEdit>() != null;
        }

        public override RecordEntryFormViewModel GetEditRowViewModel(string subGridName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave, Action onCancel, GridRowViewModel gridRow)
        {
            var record = gridRow.GetRecord();
            if (!(record is ObjectRecord))
                throw new NotSupportedException(string.Format("Error Expected Object Of Type {0}", typeof(ObjectRecord).Name));
            var newRecord = (ObjectRecord)record;
            //need to load the existing row to this
            //lets start a dialog to add it on complete
            var mapper = new ClassSelfMapper();
            var newObject = mapper.Map(newRecord.Instance);
            var recordService = new ObjectRecordService(newObject, ObjectRecordService.LookupService, ObjectRecordService.OptionSetLimitedValues, ObjectRecordService, subGridName, parentForm.ApplicationController);
            var formService = new ObjectFormService(newObject, recordService);
            formService.AllowLookupFunctions = AllowLookupFunctions;
            var viewModel = new ObjectEntryViewModel(
                parentForm.IsReadOnly ? (Action)null : () => onSave(new ObjectRecord(newObject)),
                onCancel,
                newObject, new FormController(recordService, formService, parentForm.FormController.ApplicationController), parentForm, subGridName, parentForm.OnlyValidate
                , saveButtonLabel: "Apply Changes", cancelButtonLabel: parentForm.IsReadOnly ? "Return" : null);
            return viewModel;
        }

        public override RecordEntryFormViewModel GetEditEnumerableViewModel(string subGridName, string fieldName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave, Action onCancel, GridRowViewModel gridRow)
        {
            var record = gridRow.GetRecord();
            if (!(record is ObjectRecord))
                throw new NotSupportedException(string.Format("Error Expected Object Of Type {0}", typeof(ObjectRecord).Name));
            var newRecord = (ObjectRecord)record;
            //need to load the existing row to this
            //lets start a dialog to add it on complete
            var mapper = new ClassSelfMapper();
            var newObject = mapper.Map(newRecord.Instance);
            var recordService = new ObjectRecordService(newObject, ObjectRecordService.LookupService, ObjectRecordService.OptionSetLimitedValues, ObjectRecordService, subGridName, parentForm.ApplicationController);
            var formService = new ObjectFormService(newObject, recordService, limitFields: new[] { fieldName });
            formService.AllowLookupFunctions = AllowLookupFunctions;
            var viewModel = new ObjectEntryViewModel(
                parentForm.IsReadOnly ? (Action)null : () => onSave(new ObjectRecord(newObject)),
                onCancel,
                newObject, new FormController(recordService, formService, parentForm.FormController.ApplicationController), parentForm, subGridName, parentForm.OnlyValidate
                , saveButtonLabel: "Apply Changes", cancelButtonLabel: parentForm.IsReadOnly ? "Return" : null);
            return viewModel;
        }

        public override IEnumerable<Condition> GetLookupConditions(string fieldName, string recordType, string reference, IRecord record)
        {
            var propertyInfo = GetPropertyInfo(fieldName, recordType);
            var attr = propertyInfo.GetCustomAttributes<LookupCondition>();
            var conditions = attr == null
                ? new Condition[0].ToList()
                : attr.Select(a => a.ToCondition()).ToList();
            var otherCondition = ObjectRecordService.GetLookupConditionFors(fieldName, recordType, reference, record);
            if (otherCondition != null)
            {
                conditions.Add(otherCondition);
            }
            return conditions;
        }



        internal override IEnumerable<ReferenceFieldViewModel<T>.ReferencePicklistItem> OrderPicklistItems<T>(string fieldName, string recordType, IEnumerable<ReferenceFieldViewModel<T>.ReferencePicklistItem> picklistItems)
        {
            var orderPriority = GetPropertyInfo(fieldName, recordType).GetCustomAttribute<OrderPriority>();
            if (orderPriority == null)
                return picklistItems.OrderBy(p => { return p.Record == null ? 0 : 1; }).ThenBy(p => p.Name).ToArray();

            return picklistItems.ToList()
                .OrderBy(p => { return p.Record == null ? 0 : 1; })
                .ThenBy(p =>
                {
                    return orderPriority.PriorityValues.Contains(p.Name)
                    ? orderPriority.PriorityValues.ToList().IndexOf(p.Name)
                    : 999999;
                }).ThenBy(p => p.Name).ToArray();
        }

        internal override bool InitialisePicklistIfOneOption(string fieldName, string recordType)
        {
            return GetPropertyInfo(fieldName, recordType).GetCustomAttribute<InitialiseIfOneOption>() != null;
        }

        internal override string GetPicklistDisplayField(string fieldName, string recordType, IRecordService lookupService, string recordTypeToLookup)
        {
            var picklistAttribute = GetPropertyInfo(fieldName, recordType).GetCustomAttribute<UsePicklist>();
            return picklistAttribute != null && !string.IsNullOrWhiteSpace(picklistAttribute.OverrideDisplayField)
                ? picklistAttribute.OverrideDisplayField
                : lookupService.GetPrimaryField(recordTypeToLookup);
        }

        public override IEnumerable<CustomGridFunction> GetCustomFunctionsFor(string referenceName, RecordEntryFormViewModel recordForm)
        {
            var functions = new Dictionary<string, Action>();
            var recordType = recordForm.GetEnumerableFieldViewModel(referenceName).RecordType;
            if (recordType == null)
                return new CustomGridFunction[0];

            var enumeratedType = ObjectRecordService.GetClassType(recordType);
            var customFunctions = enumeratedType.GetCustomAttributes<CustomFunction>();
            if (customFunctions != null)
            {
                foreach (var item in customFunctions)
                {
                    functions.Add(item.GetFunctionLabel(), item.GetCustomFunction(recordForm, referenceName));
                }
            }
            var allowDownloadAttribute = ObjectRecordService.GetPropertyInfo(referenceName, recordForm.RecordType).GetCustomAttribute<AllowDownload>();
            if (allowDownloadAttribute != null)
            {
                functions.Add("Download CSV", () => { recordForm.GetEnumerableFieldViewModel(referenceName).DynamicGridViewModel.DownloadCsv(); });
            }
            var customGridFunctions = functions.Select(kv => new CustomGridFunction(kv.Key, kv.Key, kv.Value)).ToList();
            var typesToResolve = GetTypesToResolve(enumeratedType);
            foreach (var typeToResolve in typesToResolve)
            {
                var injectedFunctions = recordForm.ApplicationController.ResolveInstance(typeof(CustomGridFunctions), typeToResolve.AssemblyQualifiedName) as CustomGridFunctions;
                customGridFunctions.AddRange(injectedFunctions.CustomFunctions);
            }
            return customGridFunctions;
        }

        internal override IEnumerable<CustomFormFunction> GetCustomFunctions(string recordType, RecordEntryFormViewModel recordForm)
        {
            var type = ObjectRecordService.GetClassType(recordType);
            var customGridFunctions = new List<CustomFormFunction>();
            var typesToResolve = GetTypesToResolve(type);
            foreach (var typeToResolve in typesToResolve)
            {
                var injectedFunctions = recordForm.ApplicationController.ResolveInstance(typeof(CustomFormFunctions), typeToResolve.AssemblyQualifiedName) as CustomFormFunctions;
                customGridFunctions.AddRange(injectedFunctions.CustomFunctions);
            }
            return customGridFunctions;
        }

        private static List<Type> GetTypesToResolve(Type type)
        {
            return type.GetInterfaces().Union(new[] { type }).ToList();
        }

        public override Action GetBulkAddFunctionFor(string referenceName, RecordEntryViewModelBase recordForm)
        {
            if (!AllowLookupFunctions)
                return null;
            var functions = new Dictionary<string, Action>();
            var enumeratedType = ObjectRecordService.GetPropertyType(referenceName, recordForm.GetRecordType()).GenericTypeArguments[0];
            var customFunction = enumeratedType.GetCustomAttribute<BulkAddFunction>();
            return customFunction != null
                ? customFunction.GetCustomFunction(recordForm, referenceName)
                : null;
        }

        public override void LoadPropertyChangedEvent(FieldViewModelBase fieldViewModel)
        {
            var objectRecord = fieldViewModel.GetRecordForm().GetRecord() as ObjectRecord;
            if (objectRecord != null)
            {
                var theClass = ObjectRecordService.GetClassType(objectRecord.Type);
                if (theClass.IsTypeOf(typeof(INotifyPropertyChanged)))
                {
                    var iNotify = (INotifyPropertyChanged)objectRecord.Instance;
                    iNotify.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
                    {
                        fieldViewModel.ApplicationController.DoOnMainThread(() =>
                        {
                            fieldViewModel.OnChangeBase();
                        });
                    };
                }
            }
        }
    }
}