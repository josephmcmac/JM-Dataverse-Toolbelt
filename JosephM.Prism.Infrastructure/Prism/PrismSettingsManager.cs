#region

using System;
using System.IO;
using System.Runtime.Serialization;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Interfaces To The User Defined And System Application Settings
    /// </summary>
    public class PrismSettingsManager
    {
        public PrismSettingsManager(IApplicationController applicationController)
        {
            ApplicationController = applicationController;
            AppConfigManager = new AppConfigManager();
        }

        private AppConfigManager AppConfigManager { get; set; }
        private IApplicationController ApplicationController { get; set; }

        public TSettingsObject Resolve<TSettingsObject>() where TSettingsObject : new()
        {
            // if the setting exists in the settings folder then get it
            var settingsFilename = Path.Combine(ApplicationController.SettingsPath,
                typeof (TSettingsObject).Name + ".xml");
            if (File.Exists(settingsFilename))
            {
                try
                {
                    return LoadFromSettingsFile<TSettingsObject>(settingsFilename);
                }
                catch (Exception ex)
                {
                    ApplicationController.UserMessage(string.Format("Error Loading Settings From {0}\n{1}",
                        settingsFilename, ex.DisplayString()));
                }
            }
            var standardSettingsFilename = Path.Combine(ApplicationController.ApplicationPath, "StandardSettings",
                typeof (TSettingsObject).Name + ".xml");
            if (File.Exists(standardSettingsFilename))
            {
                try
                {
                    return LoadFromSettingsFile<TSettingsObject>(standardSettingsFilename);
                }
                catch (Exception ex)
                {
                    ApplicationController.UserMessage(string.Format("Error Loading Standard Settings {0}\n{1}",
                        standardSettingsFilename, ex.DisplayString()));
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

        private static TSettingsObject LoadFromSettingsFile<TSettingsObject>(string settingsFilename)
            where TSettingsObject : new()
        {
            TSettingsObject settingsObject;
            //read from serializer
            var serializer = new DataContractSerializer(typeof (TSettingsObject));

            using (var fileStream = new FileStream(settingsFilename, FileMode.Open))
            {
                settingsObject = (TSettingsObject) serializer.ReadObject(fileStream);
            }
            return settingsObject;
        }

        public void SaveSettingsObject(object settingsObject)
        {
            // save to the setting exists in the settings folder then get it
            var type = settingsObject.GetType();
            var serializer = new DataContractSerializer(type);

            var folder = ApplicationController.SettingsPath;
            FileUtility.CheckCreateFolder(folder);

            using (
                var fileStream = new FileStream(Path.Combine(folder, type.Name + ".xml"),
                    FileMode.Create))
            {
                serializer.WriteObject(fileStream, settingsObject);
            }
        }
    }
}