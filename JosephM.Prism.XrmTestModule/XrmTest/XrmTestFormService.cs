#region

using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Validation;
using JosephM.Prism.XrmModule.Xrm;

#endregion

namespace JosephM.Prism.XrmTestModule.XrmTest
{
    public class XrmTestFormService : XrmFormService
    {
        public override FormMetadata GetFormMetadata(string recordType)
        {
            if (recordType == "new_testentity")
                return
                    new FormMetadata(new[]
                    {
                        "new_testboolean", "new_teststring", "new_testinteger", "new_testdate", "new_testpicklist"
                    });
            throw new Exception("Form not defined for record type: " + recordType);
        }

        public override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            return new ValidationRuleBase[0];
        }
    }
}