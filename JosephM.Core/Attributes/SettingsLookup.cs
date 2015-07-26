using System;
using System.Collections.Generic;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Cascading The Record Type To Another Property
    ///     Initally Used For Cacading A selected Record Type To A Record Field Or Lookup Property
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class SettingsLookup : Attribute
    {
        public Type SettingsType { get; private set; }

        public string PropertyName { get; private set; }

        public SettingsLookup(Type settingsType, string propertyName)
        {
            SettingsType = settingsType;
            PropertyName = propertyName;
        }
    }
}