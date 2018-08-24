using JosephM.Core.Attributes;
using System.Collections.Generic;

namespace JosephM.Application.Application
{
    [Instruction("Select Saved Input, Then Click The Load Button Above The Grid To Load It Into The Form")]
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
