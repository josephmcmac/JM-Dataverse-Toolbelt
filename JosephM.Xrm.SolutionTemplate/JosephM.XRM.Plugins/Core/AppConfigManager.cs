#region

using System;
using System.Collections.Generic;
using System.Configuration;

#endregion

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Class For Loading Objects From app.config Through Reflection
    ///     Class Type Must Have An Empty Constructor And Only Accessible Get/Set Properties Of Limited Types Are Implemented
    ///     To Define The Object In app.config Add A Section Item To The 'configSections' Section Of App.Config With The name
    ///     Of The Class Type
    ///     e.g.  <section name="TestObject" type="JosephM.Core.CustomConfigurationSection, JosephM.Core" />
    ///     And Add The Section With key/values For The Objects Properties To Load
    ///     e.g
    ///     <TestObject>
    ///         <ConfigurationItem>
    ///             <add key="StringField" value="StringValue" />
    ///             <add key="BooleanField" value="1" />
    ///             <add key="IntField" value="1" />
    ///         </ConfigurationItem>
    ///     </TestObject>
    /// </summary>
    public class AppConfigManager
    {
        /// <summary>
        ///     Creates A new Instance Of Type T and Loads Properties From app.config
        /// </summary>
        public T Resolve<T>() where T : new()
        {
            var configObject = new T();
            var type = configObject.GetType();
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var section = (CustomConfigurationSection)config.GetSection(type.Name);
                var sectionFields = section.ConfigurationItems;

                foreach (var property in configObject.GetType().GetProperties())
                {
                    try
                    {
                        var rawConfigString = sectionFields.GetValue(property.Name);
                        configObject.SetPropertyByString(property.Name, rawConfigString);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(
                    string.Format(
                        "Error Loading {0} Object From app.config. Ensure The Section Is Defined In 'configSections' With The Correct Type And Assembly Name e.g. type=\"JosephM.Core.AppConfig.CustomConfigurationSection, JosephM.Core\"",
                        type.Name), ex);
            }

            return configObject;
        }

        /// <summary>
        ///     Saves The Objects Values Into the Section For Its Type In app.config
        /// </summary>
        public void SetConfigurationSectionObject(object configObject)
        {
            var type = configObject.GetType();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = (CustomConfigurationSection)config.GetSection(type.Name);
            var sectionFields = section.ConfigurationItems;

            foreach (var property in configObject.GetType().GetReadWriteProperties())
            {
                var propertyValue = property.GetGetMethod().Invoke(configObject, new object[] { });
                if (propertyValue is IEnumerable<string>)
                    sectionFields.SetValue(property.Name, string.Join(",", (IEnumerable<string>)propertyValue));
                else
                    sectionFields.SetValue(property.Name, propertyValue);
            }
            config.Save();
        }
    }
}