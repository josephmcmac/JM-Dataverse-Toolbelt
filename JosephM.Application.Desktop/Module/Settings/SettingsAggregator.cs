using System;
using System.Collections.Generic;

namespace JosephM.Application.Desktop.Module.Settings
{
    public class SettingsAggregator
    {
        private List<Type> _types = new List<Type>();

        public void AddSettingType(Type type)
        {
            _types.Add(type);
        }

        public IEnumerable<Type> SettingTypes
        {
            get
            {
                return _types.ToArray();
            }
        }
    }
}
