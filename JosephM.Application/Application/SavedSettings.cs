using JosephM.Core.Attributes;
using System.Collections.Generic;

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
