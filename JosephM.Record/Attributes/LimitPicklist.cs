using System;

namespace JosephM.Record.Attributes
{
    public class LimitPicklist : Attribute
    {
        public object[] ToInclude;

        public LimitPicklist(params object[] toInclude)
        {
            ToInclude = toInclude;
        }
    }
}
