using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Record.Query
{
    public class Filter
    {
        public FilterOperator ConditionOperator { get; set; }

        public List<Condition> Conditions { get; set; }

        public List<Filter> SubFilters { get; set; }
        public bool IsQuickFindFilter { get; set; }

        public Filter()
        {
            Conditions = new List<Condition>();
            SubFilters = new List<Filter>();
        }


        public void AddCondition(string fieldname, ConditionType conditionType, string value)
        {
            Conditions.Add(new Condition(fieldname, conditionType, value));
        }

        public bool MeetsFilter(IRecord record)
        {
            var result = true;
            if(ConditionOperator == FilterOperator.Or)
            {
                result = 
                    (!Conditions.Any() && !SubFilters.Any())
                    || Conditions.Any(c => c.MeetsCondition(record))
                    || SubFilters.Any(f => f.MeetsFilter(record));
            }
            else
            {
                result = Conditions.All(c => c.MeetsCondition(record))
                    && SubFilters.All(f => f.MeetsFilter(record));
            }

            return result;

        }
    }
}