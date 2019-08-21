using System;
using System.Collections.Generic;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Specifies not to inlcude types in a lookup picklist
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class RecordTypeExclusions : Attribute
    {
        private string[] _recordTypes { get; set; }

        public RecordTypeExclusions(params string[] recordTypes)
        {
            _recordTypes = recordTypes;
        }

        public IEnumerable<string> RecordTypes
        {
            get
            {
                return _recordTypes;
            }
        }
    }
}