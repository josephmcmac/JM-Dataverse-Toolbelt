using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace JosephM.Application.Desktop.Application
{
    /// <summary>
    ///     Interfaces To The User Defined And System Application Settings
    /// </summary>
    public class DesktopSettingsManager : ISettingsManager
    {
        public DesktopSettingsManager(IApplicationController applicationController)
        {
            ApplicationController = applicationController;
            AppConfigManager = new AppConfigManager();
        }

        private AppConfigManager AppConfigManager { get; set; }
        private IApplicationController ApplicationController { get; set; }

        public TSettingsObject Resolve<TSettingsObject>(Type settingsType = null) where TSettingsObject : new()
        {
            // if the setting exists in the settings folder then get it
            string settingsFilename = GetSettingsFileName(typeof(TSettingsObject), settingsType);
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

        private string GetSettingsFileName(Type settingsObject, Type settingsType = null)
        {
            return Path.Combine(ApplicationController.SettingsPath,
                settingsObject.Name + GenerateSuffix(settingsType) + ".xml");
        }

        private TSettingsObject LoadFromSettingsFile<TSettingsObject>(string settingsFilename, Type settingsType = null)
            where TSettingsObject : new()
        {
            TSettingsObject settingsObject;
            var type = typeof(TSettingsObject);
            var knownTypes = GetKnownTypes(settingsType);
            var serializer = new DataContractSerializer(type, knownTypes);

            var text = File.ReadAllText(settingsFilename);
            foreach(var nameSpaceChange in _nameSpaceReplacements)
            {
                var nameSpacePrefix = "xmlns=\"http://schemas.datacontract.org/2004/07/";
                text = text.Replace(nameSpacePrefix + nameSpaceChange.Key + "\"", nameSpacePrefix + nameSpaceChange.Value + "\"");
            }

            using (var textStream = new MemoryStream(Encoding.Default.GetBytes(text)))
            {
                settingsObject = (TSettingsObject)serializer.ReadObject(textStream);
            }
            return settingsObject;
        }

        private static List<Type> GetKnownTypes(Type settingsType)
        {
            //adding these in because some have object fields
            //which are a different type depending on the field
            var knownTypes = new List<Type>(new[]
            {
                typeof(Lookup),
                typeof(PicklistOption),
            });
            if (settingsType != null)
                knownTypes.Add(settingsType);
            return knownTypes;
        }

        public void SaveSettingsObject(object settingsObject, Type settingsType = null)
        {
            // save to the setting exists in the settings folder then get it
            var type = settingsObject.GetType();
            var knownTypes = GetKnownTypes(settingsType);
            var serializer = new DataContractSerializer(type, knownTypes);


            var folder = ApplicationController.SettingsPath;
            FileUtility.CheckCreateFolder(folder);

            var settings = new XmlWriterSettings { Indent = true };

            using (var w = XmlWriter.Create(Path.Combine(folder, type.Name + GenerateSuffix(settingsType) + ".xml"), settings))
            {
                serializer.WriteObject(w, settingsObject);
            }
        }

        private static string GenerateSuffix(Type settingsType)
        {
            return (settingsType != null ? "_" + settingsType.Name : "");
        }

        void ISettingsManager.ProcessNamespaceChange(string newNamespace, string oldNamespace)
        {
            if (_nameSpaceReplacements.ContainsKey(newNamespace))
                throw new Exception($"{oldNamespace} has already been loaded as a changed namespace");

            _nameSpaceReplacements.Add(oldNamespace, newNamespace);
        }

        private Dictionary<string, string> _nameSpaceReplacements = new Dictionary<string, string>();
    }
}