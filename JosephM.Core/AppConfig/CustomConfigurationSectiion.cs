#region

using System;
using System.Configuration;

#endregion

namespace JosephM.Core.AppConfig
{
    public class CustomConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("ConfigurationItem")]
        public CustomConfigurationElementCollection ConfigurationItems
        {
            get { return (CustomConfigurationElementCollection) (base["ConfigurationItem"]); }
        }
    }

    [ConfigurationCollection(typeof (ConfigurationElement))]
    public class CustomConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CustomConfigurationElement) (element)).Key;
        }

        public string GetValue(string key)
        {
            string response = null;
            try
            {
                response = ((CustomConfigurationElement) base.BaseGet(key)).Value;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(string.Concat("Error retrieving ", key,
                    " property - note this is case sensitive"), ex);
            }
            return response;
        }

        public void SetValue(string key, object value)
        {
            var stringValue = value != null ? value.ToString() : "";
            var configElement = (CustomConfigurationElement) base.BaseGet(key);
            if (configElement != null)
                configElement.Value = stringValue;
            else
            {
                configElement = new CustomConfigurationElement()
                {
                    Key = key,
                    Value = stringValue
                };
                base.BaseAdd(configElement);
            }
        }
    }

    public class CustomConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("key", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Key
        {
            get { return ((string) (base["key"])); }
            set { base["key"] = value; }
        }

        [ConfigurationProperty("value", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Value
        {
            get { return ((string) (base["value"])); }
            set { base["value"] = value; }
        }
    }
}