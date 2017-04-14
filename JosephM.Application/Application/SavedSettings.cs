using JosephM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Application.Application
{
    public class SavedSettings
    {
        public SavedSettings()
        {
            SavedRequests = new object[0];
        }

        [DoNotAllowAdd]
        public IEnumerable<object> SavedRequests { get;  set; }
    }
}
