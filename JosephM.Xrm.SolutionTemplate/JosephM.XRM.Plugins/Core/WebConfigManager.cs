#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;

#endregion

namespace $safeprojectname$.Core
{
    public class WebConfigManager
    {
        public T Resolve<T>() where T : new()
        {
            var configObject = new T();
            var type = configObject.GetType();
            try
            {
                var section = ConfigurationManager.GetSection(type.Name) as NameValueCollection;
                if (section == null)
                    throw new ConfigurationErrorsException(string.Concat(type.Name, " section not found"));

                foreach (var property in configObject.GetType().GetProperties())
                {
                    try
                    {
                        var rawConfigString = section[property.Name];
                        SetFromRawString(property, configObject, rawConfigString);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(
                    string.Concat("Error retrieving ", type.Name, " object from config"), ex);
            }

            return configObject;
        }

        internal void SetFromRawString(PropertyInfo property, object configObject, string rawConfigString)
        {
            var propertyType = property.PropertyType;
            if (propertyType == typeof(bool))
                property.GetSetMethod().Invoke(configObject, new object[] { rawConfigString == "1" });
            else if (propertyType.IsEnum)
                property.GetSetMethod()
                    .Invoke(configObject, new[] { Enum.Parse(propertyType, rawConfigString) });
            else if (propertyType == typeof(IEnumerable<string>))
                property.GetSetMethod()
                    .Invoke(configObject, new[] { rawConfigString.Split(',') });
            else if (propertyType == typeof(Password))
                property.GetSetMethod()
                    .Invoke(configObject, new[] { new Password(rawConfigString, false, false) });
            else if (propertyType.HasStringConstructor())
                property.GetSetMethod()
                    .Invoke(configObject,
                        new[]
                        {
                            propertyType.GetStringConstructorInfo()
                                .Invoke(new object[] {rawConfigString})
                        });
            else
                property.GetSetMethod().Invoke(configObject, new object[] { rawConfigString });
        }

        public void SetConfigurationSectionObject(object configObject)
        {
            var type = configObject.GetType();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = (CustomConfigurationSection)config.GetSection(type.Name);
            var sectionFields = section.ConfigurationItems;

            foreach (var property in configObject.GetType().GetProperties())
            {
                var propertyValue = property.GetGetMethod().Invoke(configObject, new object[] { });
                sectionFields.SetValue(property.Name, propertyValue);
            }
            config.Save();
        }
    }
}