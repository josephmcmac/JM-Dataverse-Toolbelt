#region

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class ObjectFormService : FormServiceBase
    {
        private FormMetadata _formMetadata;
        private ObjectRecordService ObjectRecordService { get; set; }

        public ObjectFormService(object objectToEnter, ObjectRecordService objectRecordService, IDictionary<string, Type> objectTypeMaps = null)
        {
            ObjectToEnter = objectToEnter;
            ObjectRecordService = objectRecordService;
            ObjectTypeMaps = objectTypeMaps;
        }

        public object ObjectToEnter { get; set; }

        private Type ObjectType
        {
            get { return ObjectToEnter.GetType(); }
        }

        public IDictionary<string, Type> ObjectTypeMaps { get; private set; }

        public override FormMetadata GetFormMetadata(string recordType)
        {
            if (_formMetadata == null)
            {
                var formSections = new List<FormSection>();

                var type = ObjectToEnter.GetType();
                var propertyMetadata = ObjectRecordService.GetFieldMetadata(type.AssemblyQualifiedName);

                var standardFieldSectionName = type.GetDisplayName();

                var fieldSections = type.GetCustomAttributes<Group>();
                var otherSections = new Dictionary<string, List<FormFieldMetadata>>();
                foreach(var section in fieldSections)
                {
                    otherSections[section.Name] = new List<FormFieldMetadata>();
                    var newSection = new FormFieldSection(section.Name, otherSections[section.Name], section.WrapHorizontal, section.Order);
                    formSections.Add(newSection);
                }

                foreach (var property in propertyMetadata.Where(m => m.Readable || m.Writeable))
                {
                    if (property.FieldType == RecordFieldType.Enumerable)
                    {
                        var thisMetadata = (EnumerableFieldMetadata)property;
                        var thisFieldType = thisMetadata.EnumeratedTypeQualifiedName;
                        var gridFields = GetGridMetadata(thisFieldType);
                        var section = new SubGridSection(property.SchemaName.SplitCamelCase(),
                            thisMetadata.EnumeratedTypeQualifiedName,
                            property.SchemaName, gridFields);
                        formSections.Add(section);
                    }
                    else
                    {
                        var fieldMetadata = new PersistentFormField(property.SchemaName);
                        fieldMetadata.Order = property.Order;

                        var propinfo = ObjectRecordService.GetPropertyInfo(property.SchemaName, type.AssemblyQualifiedName);
                        var groupAttribute = propinfo.GetCustomAttribute<Group>();
                        var sectionName = groupAttribute != null
                            ? groupAttribute.Name
                            : standardFieldSectionName;
                        var order = groupAttribute != null
                            ? groupAttribute.Order
                            : 1;
                        var wrapHorizontal = groupAttribute != null
                            ? groupAttribute.WrapHorizontal
                            : false;

                        if (!otherSections.ContainsKey(sectionName))
                        {
                            otherSections[sectionName] = new List<FormFieldMetadata>();
                            var newSection = new FormFieldSection(sectionName, otherSections[sectionName], wrapHorizontal, order);
                            formSections.Add(newSection);
                        }
                        otherSections[sectionName].Add(fieldMetadata);
                    }
                }
                formSections = formSections.OrderBy(s => s.Order).ToList();
                _formMetadata = new FormMetadata(formSections);
            }
            return _formMetadata;
        }

        private IEnumerable<GridFieldMetadata> GetGridMetadata(string thisFieldType)
        {
            //very similar logic in get saved views
            var gridFields = new List<GridFieldMetadata>();
            foreach (var field in ObjectRecordService.GetFields(thisFieldType))
            {
                var propertyInfo = ObjectRecordService.GetPropertyInfo(field, thisFieldType);
                if (propertyInfo.GetCustomAttribute<HiddenAttribute>() == null)
                {
                    var gridField = new GridFieldMetadata(field);
                    gridField.IsEditable = propertyInfo.CanWrite;
                    var orderAttribute = propertyInfo.GetCustomAttribute<DisplayOrder>();
                    if (orderAttribute != null)
                        gridField.Order = orderAttribute.Order;
                    else
                        gridField.Order = 100000;
                    var widthAttribute = propertyInfo.GetCustomAttribute<GridWidth>();
                    if (widthAttribute != null)
                        gridField.WidthPart = widthAttribute.Width;
                    gridFields.Add(gridField);
                }
            }
            return gridFields;
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

        public override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
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
                    ObjectRecordService.GetValidatorAttributes(sectionIdentifier, ObjectType.AssemblyQualifiedName)
                        .Select(va => new PropertyAttributeValidationRule(va));
            }
            else
                return new ValidationRuleBase[0];
        }

        internal override IEnumerable<Action<RecordEntryFormViewModel>> GetOnChanges(string fieldName)
        {
            return GetOnChanges(fieldName, ObjectType.AssemblyQualifiedName);
        }

        internal override IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, string recordType)
        {
            var onChanges = new List<Action<RecordEntryViewModelBase>>();
            AppendLookupForChanges(fieldName, recordType, onChanges);
            AppendConnectionForChanges(fieldName, recordType, onChanges, false);
            AppendInitialiseAttributes(fieldName, recordType, onChanges);
            AppendUniqueOnAttributes(fieldName, recordType, onChanges);
            AppendReadOnlyWhenSetAttributes(fieldName, recordType, onChanges);
            AppendDisplayNameAttributes(fieldName, recordType, onChanges);
            return base.GetOnChanges(fieldName, recordType).Union(onChanges);
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
                                    var typedViewModel = (LookupFieldViewModel) fieldViewModel;
                                    typedViewModel.ConnectionForChanged();
                                }
                                if (fieldViewModel is RecordTypeFieldViewModel)
                                {
                                    var typedViewModel = (RecordTypeFieldViewModel) fieldViewModel;
                                    typedViewModel.ItemsSource = ObjectRecordService
                                        .GetPicklistKeyValues(fieldViewModel.FieldName,
                                            fieldViewModel.GetRecordType())
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
            return methods;
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

        internal override bool AllowAddRow(string subGridName)
        {
            var prop = GetPropertyInfo(subGridName, ObjectType.AssemblyQualifiedName);
            return prop.GetCustomAttribute<DoNotAllowAdd>() == null;
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
                                && dependantViewModel.ValueObject.IsEmpty())
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

        private void AppendLookupForChanges(string fieldName, string recordType, List<Action<RecordEntryViewModelBase>> onChanges)
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

        internal override string GetDependantValue(string field, string recordType, RecordEntryViewModelBase viewModel)
        {
            var propertyInfo = GetPropertyInfo(field, viewModel.GetRecord().Type);
            if (propertyInfo.PropertyType == typeof(FileReference))
            {
                var attr = propertyInfo.GetCustomAttribute<FileMask>();
                return attr == null ? null : attr.Mask;
            }
            else return GetRecordTypeFor(field, viewModel);
        }

        private string GetRecordTypeFor(string field, RecordEntryViewModelBase viewModel)
        {
            var propertyInfo = GetPropertyInfo(field, viewModel.GetRecord().Type);
            if (propertyInfo != null)
            {
                var attribute = propertyInfo.GetCustomAttribute<ReferencedType>();
                if (attribute != null)
                    return attribute.Type;
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
                            var parentsFieldViewmOdel = parentForm.GetRecordTypeFieldViewModel(parentField.Name);
                            if (parentsFieldViewmOdel.Value != null)
                                return parentsFieldViewmOdel.Value.Key;
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
                var newRecord = (ObjectRecord)ObjectRecordService.NewRecord(fieldMetadata.EnumeratedType.AssemblyQualifiedName);
                var newObject = newRecord.Instance;
                var recordService = new ObjectRecordService(newObject, parentForm.ApplicationController);
                var viewModel = new ObjectEntryViewModel(
                    () => onSave(new ObjectRecord(newObject)),
                    onCancel,
                    newObject, new FormController(recordService, new ObjectFormService(newObject, recordService), parentForm.FormController.ApplicationController), parentForm, subGridName, parentForm.OnlyValidate);
                return viewModel;
                //ideally could hide the parent dialog temporarily and load this one
            }
            //if the object specifies use a form then use the form/dialog
            else
                return null;
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
            var viewModel = new ObjectEntryViewModel(
                () => onSave(new ObjectRecord(newObject)),
                onCancel,
                newObject, new FormController(recordService, new ObjectFormService(newObject, recordService), parentForm.FormController.ApplicationController), parentForm, subGridName, parentForm.OnlyValidate);
            return viewModel;
        }

        internal override IEnumerable<Condition> GetLookupConditions(string fieldName, string recordType, string reference, IRecord record)
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

        private readonly object _lockObject = new Object();
        private readonly IDictionary<string, CachedPicklist> _cachedPicklist = new Dictionary<string, CachedPicklist>();

        private class CachedPicklist
        {
            private IEnumerable<Condition> Conditions { get; set; }
            public IRecordService LookupService { get; set; }
            public IEnumerable<IRecord> Picklist { get; set; }


            public CachedPicklist(IEnumerable<IRecord> picklist, IEnumerable<Condition> conditions,
                IRecordService lookupService)
            {
                {
                    Picklist = picklist;
                    Conditions = conditions;
                    LookupService = lookupService;
                }
            }
        }
        internal override IEnumerable<IRecord> GetLookupPicklist(string fieldName, string recordType, string reference, IRecord record, IRecordService lookupService, string recordTypeToLookup)
        {
            var conditions = GetLookupConditions(fieldName, recordType, reference, record);
            lock (_lockObject)
            {
                if (!_cachedPicklist.ContainsKey(fieldName) || _cachedPicklist[fieldName].LookupService != lookupService)
                {
                    var picklist = lookupService.RetrieveAllAndClauses(recordTypeToLookup, conditions,
                        new[] {lookupService.GetPrimaryField(recordTypeToLookup)});
                    var cache = new CachedPicklist(picklist, conditions, lookupService);
                    if (_cachedPicklist.ContainsKey(fieldName))
                        _cachedPicklist[fieldName] = cache;
                    else
                        _cachedPicklist.Add(fieldName, cache);
                }
                return _cachedPicklist[fieldName].Picklist;
            }
        }

        internal override IEnumerable<CustomGridFunction> GetCustomFunctionsFor(string referenceName, RecordEntryViewModelBase recordForm)
        {
            var functions = new Dictionary<string, Action>();
            var objectFormService = recordForm as ObjectEntryViewModel;
            if (objectFormService != null)
            {
                var property = GetPropertyInfo(referenceName, ObjectType.AssemblyQualifiedName);
                var customFunctions = property.GetCustomAttributes<CustomFunction>();
                if (customFunctions != null)
                {
                    foreach (var item in customFunctions)
                    {
                        var thisItem = item;
                        var thisMethod = new Action(() => ObjectToEnter.InvokeMethod(thisItem.FunctionName, recordForm.ApplicationController));
                        functions.Add(thisItem.FunctionName.SplitCamelCase(),
                            () =>
                            {
                                objectFormService.LoadSubgridsToObject();
                                thisMethod();
                                var subGrid = objectFormService.GetSubGridViewModel(referenceName);
                                subGrid.DynamicGridViewModel.ReloadGrid();
                            });
                    }
                }
            }
            return functions.Select(kv => new CustomGridFunction(kv.Key, kv.Value)).ToArray();
        }
    }
}