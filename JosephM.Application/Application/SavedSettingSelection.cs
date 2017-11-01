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
        //todo change this to display a grid with a select option and remove in the settings dropdown
        [RequiredProperty]
        [SettingsLookup(typeof(SavedSettings), "SavedRequests")]
        public object Selection { get;  set; }

        public IEnumerable<object> SavedRequests { get; set; }
    }
}
