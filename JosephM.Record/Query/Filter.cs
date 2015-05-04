using System.Collections.Generic;

namespace JosephM.Record.Query
{
    public class Filter
    {
        public FilterOperator ConditionOperator { get; set; }

        public List<Condition> Conditions { get; set; }

        public Filter()
        {
            Conditions = new List<Condition>();
        }


        public void AddCondition(string p1, ConditionType conditionType, string p2)
        {
            Conditions.Add(new Condition(p1, conditionType, p2));
        }
    }
}