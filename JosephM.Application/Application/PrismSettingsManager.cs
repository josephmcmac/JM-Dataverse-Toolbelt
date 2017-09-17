#region

using System;
using System.IO;
using System.Runtime.Serialization;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;
using JosephM.Core.Service;

#endregion

namespace JosephM.Application.Application
{
    /// <summary>
    ///     Interfaces To The User Defined And System Application Settings
    /// </summary>
    public class PrismSettingsManager : ISettingsManager
    {
        public PrismSettingsManager(IApplicationController applicationController)
        {
            ApplicationController = applicationController;
            AppConfigManager = new AppConfigManager();
        }

        private AppConfigManager AppConfigManager { get; set; }
        private IApplicationController ApplicationController { get; set; }

        public TSettingsObject Resolve<TSettingsObject>(Type settingsType = null) where TSettingsObject : new()
        {
            // if the setting exists in the settings folder then get it
            var settingsFilename = Path.Combine(ApplicationController.SettingsPath,
                typeof (TSettingsObject).Name + GenerateSuffix(settingsType) + ".xml");
            if (File.Exists(settingsFilename))
            {
                try
                {
                    return LoadFromSettingsFile<TSettingsObject>(settingsFilename, settingsType);
                }
                catch (Exception ex)
                {
                    ApplicationController.UserMessage(string.Format("Error Loading Settings From {0}\n{1}",
                        settingsFilename, ex.DisplayString()));
                }
            }
            // else get it from app config
            try
            {
                return AppConfigManager.Resolve<TSettingsObject>();
            }
            catch (Exception ex)
            {
                //ApplicationController.UserMessage(string.Format("Error Loading Settings. Settings Will Be Empty\n{0}",
                //    ex.DisplayString()));
            }
            return new TSettingsObject();
        }

        private static TSettingsObject LoadFromSettingsFile<TSettingsObject>(string settingsFilename, Type settingsType = null)
            where TSettingsObject : new()
        {
            TSettingsObject settingsObject;
            var type = typeof(TSettingsObject);
            //read from serializer
            var serializer = settingsType == null
                ? new DataContractSerializer(type)
                : new DataContractSerializer(type, new[] { settingsType });

            using (var fileStream = new FileStream(settingsFilename, FileMode.Open))
            {
                settingsObject = (TSettingsObject) serializer.ReadObject(fileStream);
            }
            return settingsObject;
        }

        public void SaveSettingsObject(object settingsObject, Type settingsType = null)
        {
            // save to the setting exists in the settings folder then get it
            var type = settingsObject.GetType();
            var serializer = settingsType == null
                ? new DataContractSerializer(type)
                : new DataContractSerializer(type, new[] { settingsType });
            

            var folder = ApplicationController.SettingsPath;
            FileUtility.CheckCreateFolder(folder);

            using (
                var fileStream = new FileStream(Path.Combine(folder, type.Name + GenerateSuffix(settingsType) + ".xml"),
                    FileMode.Create))
            {
                serializer.WriteObject(fileStream, settingsObject);
            }
        }

        private static string GenerateSuffix(Type settingsType)
        {
            return (settingsType != null ? "_" + settingsType.Name : "");
        }
    }
}