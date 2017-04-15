using JosephM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Application.Application
{
    public class SavedSettingSelection
    {
        [RequiredProperty]
        [SettingsLookup(typeof(SavedSettings), "SavedRequests")]
        public object Selection { get;  set; }
    }
}
