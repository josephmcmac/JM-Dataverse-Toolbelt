using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace $safeprojectname$.Rollups
{
    /// <summary>
    /// Configuration object for a custom rollup/Rollup field
    /// Use the RollupService to process the Rollup in plugins
    /// For each configured type you will need plugin triggers registered for
    /// The type containing the rollup field on preoperation create - to initialise its value
    /// The type containing the field Rollupd on postevent create synch, postevent update synch, postevent delete synch - to process changes
    /// the preimage will need to contain all dependency fields
    /// </summary>
    public class LookupRollup
    {
        public LookupRollup(string recordTypeWithRollup, string rollupField, string recordTypeRolledup,
            string fieldRolledUp, RollupType rollupType, string lookupName, Type objectType)
        {
            LookupName = lookupName;
            ObjectType = objectType;
            RecordTypeWithRollup = recordTypeWithRollup;
            RollupField = rollupField;
            RecordTypeRolledup = recordTypeRolledup;
            RollupType = rollupType;
            FieldRolledup = fieldRolledUp;
            Filters = new ConditionExpression[] { };
            AddFilter("statecode", ConditionOperator.Equal, 0);
            LinkEntity = null;
            if (ObjectType != null)
            {
                if (ObjectType == typeof(decimal))
                    NullAmount = (decimal)0;
                else if (ObjectType == typeof(int))
                    NullAmount = (int)0;
                else if (ObjectType == typeof(Money))
                    NullAmount = new Money(0);
                else if (ObjectType == typeof(bool))
                    NullAmount = false;
            }
        }

        public Type ObjectType { get; set; }
        public string LookupName { get; private set; }
        public string RecordTypeWithRollup { get; private set; }
        public string RollupField { get; private set; }
        public string RecordTypeRolledup { get; private set; }
        public RollupType RollupType { get; private set; }
        public ConditionExpression[] Filters { get; private set; }
        public LinkEntity LinkEntity { get; set; }

        public string FieldRolledup { get; private set; }

        public void ClearFilters()
        {
            Filters = new ConditionExpression[0];
        }

        public void AddFilter(string fieldName, ConditionOperator conditionoperator, object value)
        {
            Filters =
                Filters.Concat(new[] { new ConditionExpression(fieldName, conditionoperator, value) }).ToArray();
        }

        public object NullAmount { get; set; }
    }
}