using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Attributes;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Crud.Validations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Crud
{
    public class XrmFormService : FormServiceBase
    {
        public override FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null)
        {
            switch (recordType)
            {
                case Entities.webresource:
                case Entities.plugintype:
                case Entities.role:
                case Entities.sdkmessage:
                    {
                        return null;
                    }
                case Entities.solution:
                    {
                        return new FormMetadata(new[]
                        {
                            new FormFieldSection("Solution", new []
                            {
                                new PersistentFormField(Fields.solution_.friendlyname),
                                new PersistentFormField(Fields.solution_.publisherid),
                                new PersistentFormField(Fields.solution_.uniquename),
                                new PersistentFormField(Fields.solution_.version) { EditableFormWidth = 60 },
                                new PersistentFormField(Fields.solution_.configurationpageid),
                                new PersistentFormField(Fields.solution_.description)
                            }, Group.DisplayLayoutEnum.VerticalCentered)
                        });
                    }
                    case Entities.publisher:
                    {
                        return new FormMetadata(new[]
                        {
                            new FormFieldSection("Publisher", new []
                            {
                                new PersistentFormField(Fields.publisher_.friendlyname),
                                new PersistentFormField(Fields.publisher_.uniquename),
                                new PersistentFormField(Fields.publisher_.customizationprefix),
                                new PersistentFormField(Fields.publisher_.customizationoptionvalueprefix),
                                new PersistentFormField(Fields.publisher_.description),
                            }, Group.DisplayLayoutEnum.VerticalCentered),
                            new FormFieldSection("Contact Details", new []
                            {
                                new PersistentFormField(Fields.publisher_.address1_telephone1),
                                new PersistentFormField(Fields.publisher_.emailaddress),
                                new PersistentFormField(Fields.publisher_.supportingwebsiteurl),
                            }, Group.DisplayLayoutEnum.VerticalCentered),
                            new FormFieldSection("Address", new []
                            {
                                new PersistentFormField(Fields.publisher_.address1_line1),
                                new PersistentFormField(Fields.publisher_.address1_line2),
                                new PersistentFormField(Fields.publisher_.address1_city),
                                new PersistentFormField(Fields.publisher_.address1_stateorprovince),
                                new PersistentFormField(Fields.publisher_.address1_postalcode),
                                new PersistentFormField(Fields.publisher_.address1_country),
                            }, Group.DisplayLayoutEnum.VerticalCentered)
                        });
                    }
            }
            //consider virtual owner (display as name not loaded)
            if (recordService != null)
            {
                var fields = recordService
                    .GetFields(recordType)
                    .OrderBy(f => recordService.GetFieldLabel(f, recordType))
                    .ToArray();
                return new FormMetadata(fields, recordService.GetDisplayName(recordType));
            }
            return null;
        }

        public override IEnumerable<Condition> GetLookupConditions(string fieldName, string recordType, string reference, IRecord record)
        {
            var conditions = new List<Condition>();
            switch(recordType)
            {
                case Entities.solution:
                    {
                        switch (fieldName)
                        {
                            case Fields.solution_.publisherid:
                                {
                                    
                                    conditions.Add(new Condition(Fields.publisher_.isreadonly, ConditionType.Equal, false));
                                    conditions.Add(new Condition(Fields.publisher_.uniquename, ConditionType.NotEqual, "MicrosoftCorporation"));
                                    break;
                                }
                            case Fields.solution_.configurationpageid:
                                {
                                    conditions.Add(new Condition(Fields.webresource_.webresourcetype, ConditionType.Equal, OptionSets.WebResource.Type.WebpageHTML));
                                    break;
                                }
                        }
                        break;
                    }
            }
            return conditions;
        }

        public override bool UsePicklist(string fieldName, string recordType)
        {
            switch (recordType)
            {
                case Entities.solution:
                    {
                        switch (fieldName)
                        {
                            case Fields.solution_.publisherid:
                                {
                                    return true;
                                }
                        }
                        break;
                    }
            }
            return false;
        }


        public override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName, string recordType)
        {
            var conditions = new List<ValidationRuleBase>();
            switch (recordType)
            {
                case Entities.publisher:
                    {
                        switch (fieldName)
                        {
                            case Fields.publisher_.uniquename:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    conditions.Add(new PropertyAttributeValidationRule(new SolutionOrPublisherNameValidation()));
                                    break;
                                }
                            case Fields.publisher_.friendlyname:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    break;
                                }
                            case Fields.publisher_.customizationprefix:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    conditions.Add(new PropertyAttributeValidationRule(new PrefixValidation()));
                                    break;
                                }
                            case Fields.publisher_.customizationoptionvalueprefix:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    break;
                                }
                        }
                        break;
                    }
                case Entities.solution:
                    {
                        switch (fieldName)
                        {
                            case Fields.solution_.uniquename:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    conditions.Add(new PropertyAttributeValidationRule(new SolutionOrPublisherNameValidation()));
                                    break;
                                }
                            case Fields.solution_.friendlyname:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    break;
                                }
                            case Fields.solution_.publisherid:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    break;
                                }
                            case Fields.solution_.version:
                                {
                                    conditions.Add(new PropertyAttributeValidationRule(new RequiredProperty()));
                                    conditions.Add(new PropertyAttributeValidationRule(new VersionPropertyValidator()));
                                    break;
                                }
                        }
                        break;
                    }
            }
            return conditions;
        }

        public override IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, string recordType, RecordEntryViewModelBase entryViewModel)
        {
            var onChanges = new List<Action<RecordEntryViewModelBase>>();
            switch (recordType)
            {
                case Entities.solution:
                    {
                        switch (fieldName)
                        {
                            case Fields.solution_.friendlyname:
                                {
                                    onChanges.Add((rf) =>
                                    {
                                        var friendlyName = rf.GetStringFieldFieldViewModel(Fields.solution_.friendlyname).Value;
                                        var uniqueNameViewModel = rf.GetStringFieldFieldViewModel(Fields.solution_.uniquename);
                                        if (!string.IsNullOrEmpty(friendlyName)
                                            && string.IsNullOrEmpty(uniqueNameViewModel.Value))
                                        {
                                            uniqueNameViewModel.Value = friendlyName.Replace(" ", "");
                                        }
                                    });
                                    break;
                                }
                            case Fields.solution_.uniquename:
                                {
                                    onChanges.Add((rf) =>
                                    {
                                        var uniqueNameViewModel = rf.GetStringFieldFieldViewModel(Fields.solution_.uniquename);
                                        var uniqueName = uniqueNameViewModel.Value;
                                        var stripInvalidChars = uniqueNameViewModel;
                                        var validNonLetterDigitsChars = "_";
                                        var updatedUniqueName = uniqueName != null
                                        ? new string(uniqueName.Where(c => char.IsLetterOrDigit(c) || validNonLetterDigitsChars.Contains(c)).ToArray())
                                        : null;
                                        if(updatedUniqueName != uniqueName)
                                        {
                                            uniqueNameViewModel.Value = updatedUniqueName;
                                        }
                                    });
                                    break;
                                }
                        }
                        break;
                    }
                case Entities.publisher:
                    {
                        switch (fieldName)
                        {
                            case Fields.publisher_.friendlyname:
                                {
                                    onChanges.Add((rf) =>
                                    {
                                        var friendlyName = rf.GetStringFieldFieldViewModel(Fields.publisher_.friendlyname).Value;
                                        var uniqueNameViewModel = rf.GetStringFieldFieldViewModel(Fields.publisher_.uniquename);
                                        if (!string.IsNullOrEmpty(friendlyName)
                                            && string.IsNullOrEmpty(uniqueNameViewModel.Value))
                                        {
                                            uniqueNameViewModel.Value = friendlyName.Replace(" ", "");
                                        }
                                    });
                                    break;
                                }
                        }
                        break;
                    }
            }
            return base.GetOnChanges(fieldName, recordType, entryViewModel).Union(onChanges);
        }

        public override IEnumerable<Action<RecordEntryViewModelBase>> GetOnLoadTriggers(string fieldName, string recordType)
        {
            var methods = new List<Action<RecordEntryViewModelBase>>();
            if (recordType == Entities.solution)
            {
                switch (fieldName)
                {
                    case Fields.solution_.version:
                        {
                            methods.Add((rf) =>
                            {
                                var versionViewModel = rf.GetStringFieldFieldViewModel(Fields.solution_.version);
                                if (string.IsNullOrEmpty(versionViewModel.Value))
                                {
                                    versionViewModel.Value = "1.0.0.0";
                                }
                            });
                            break;
                        }
                }
            }
            return base.GetOnLoadTriggers(fieldName, recordType).Union(methods);
        }
    }
}
