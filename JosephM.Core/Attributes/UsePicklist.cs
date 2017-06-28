using System;

namespace JosephM.Core.Attributes
{
    public class UsePicklist : Attribute
    {
        public UsePicklist()
        {

        }

        public UsePicklist(string overrideDisplayField)
        {
            OverrideDisplayField = overrideDisplayField;
        }

        public string OverrideDisplayField { get; set; }
    }
}
