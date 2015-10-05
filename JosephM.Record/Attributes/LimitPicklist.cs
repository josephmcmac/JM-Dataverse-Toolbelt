using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
