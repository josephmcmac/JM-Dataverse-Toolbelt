using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Attributes;
using JosephM.Record.Extentions;

namespace JosephM.Prism.XrmModule.Forms
{
    public class XrmFormService : FormServiceBase
    {
        public override FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null)
        {
            switch (recordType)
            {
                case Entities.solution:
                    {
                        return new FormMetadata(new[]
                        {
                            new FormFieldSection("Solution", new []
                            {
                                new PersistentFormField(Fields.solution_.friendlyname),
                                new PersistentFormField(Fields.solution_.uniquename),
                                new PersistentFormField(Fields.solution_.publisherid),
                                new PersistentFormField(Fields.solution_.version),
                                new PersistentFormField(Fields.solution_.configurationpageid),
                                new PersistentFormField(Fields.solution_.description)
                            })
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
                            }),
                            new FormFieldSection("Contact Details", new []
                            {
                                new PersistentFormField(Fields.publisher_.address1_telephone1),
                                new PersistentFormField(Fields.publisher_.emailaddress),
                                new PersistentFormField(Fields.publisher_.supportingwebsiteurl),
                            }),
                            new FormFieldSection("Address", new []
                            {
                                new PersistentFormField(Fields.publisher_.address1_line1),
                                new PersistentFormField(Fields.publisher_.address1_line2),
                                new PersistentFormField(Fields.publisher_.address1_city),
                                new PersistentFormField(Fields.publisher_.address1_stateorprovince),
                                new PersistentFormField(Fields.publisher_.address1_postalcode),
                                new PersistentFormField(Fields.publisher_.address1_country),
                            })
                        });
                    }
            }
            //consider virtual owner (display as name not loaded)
            if (recordService != null)
                return new FormMetadata(recordService.GetFields(recordType).OrderBy(f => recordService.GetFieldLabel(f, recordType)).ToArray());
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
                                    break;
                                }
                        }
                        break;
                    }
            }
            return conditions;
        }
    }
}
