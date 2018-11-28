using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using JosephM.Application.Desktop.Application;
using System.Linq;

namespace JosephM.Xrm.Vsix.Application
{
    public class VsixSettingsManager : ISettingsManager
    {
        private IVisualStudioService VisualStudioService { get; set; }
        public DesktopSettingsManager DesktopSettingsManager { get; }

        public VsixSettingsManager(IVisualStudioService visualStudioService, DesktopSettingsManager desktopSettingsManager)
        {
            VisualStudioService = visualStudioService;
            DesktopSettingsManager = desktopSettingsManager;
        }

        public TSettingsObject Resolve<TSettingsObject>(Type settingsType = null) where TSettingsObject : new()
        {
            var type = settingsType ?? typeof(TSettingsObject);
            var savedFileSetting = GetSavedSolutionFileSetting(type);
            if (savedFileSetting != null)
            {
                string readFileContents = null;

                var fileName = savedFileSetting.FileName;
                if(savedFileSetting.UsePersonalisedFile)
                    readFileContents = VisualStudioService.GetVsixSettingText(savedFileSetting.PersonalisedFileName);
                if(readFileContents == null)
                    readFileContents = VisualStudioService.GetVsixSettingText(fileName);

                if (string.IsNullOrEmpty(readFileContents))
                    return new TSettingsObject();
                else
                {
                    if (type == typeof(XrmRecordConfiguration))
                    {
                        //this saved as a dictionary rather than object
                        //because the solution template test project
                        //needs to deserialise it to a new defined type
                        //with different namespaces
                        var dictionary = string.IsNullOrEmpty(readFileContents)
                            ? new Dictionary<string, string>()
                            : (Dictionary<string, string>)
                                JsonHelper.JsonStringToObject(readFileContents, typeof(Dictionary<string, string>));

                        var xrmConfig = new TSettingsObject();
                        foreach (var prop in xrmConfig.GetType().GetReadWriteProperties())
                        {
                            if (dictionary.ContainsKey(prop.Name))
                                xrmConfig.SetPropertyByString(prop.Name, dictionary[prop.Name]);
                        }
                        return xrmConfig;
                    }
                    else
                        return (TSettingsObject)JsonHelper.JsonStringToObject(readFileContents, type);
                }
            }
            else
            {
                return DesktopSettingsManager.Resolve<TSettingsObject>(settingsType: settingsType);
            }
        }

        public void SaveSettingsObject(object settingsObject, Type settingsType = null)
        {
            //this bit of a hack but the XrmConnections module tries to save a settings object
            //we dont want because we use the package settings
            //instead so lets just ignore it
            if (settingsObject is SavedXrmConnections)
                return;
            var type = settingsType ?? settingsObject.GetType();
            if (settingsObject is XrmRecordConfiguration)
                type = typeof(XrmRecordConfiguration);
            var savedFileSetting = GetSavedSolutionFileSetting(type);
            if (savedFileSetting != null)
                VisualStudioService.AddVsixSetting(savedFileSetting.UsePersonalisedFile ? savedFileSetting.PersonalisedFileName : savedFileSetting.FileName, settingsObject);
            else
                DesktopSettingsManager.SaveSettingsObject(settingsObject, settingsType: settingsType);
        }

        private SaveSolutionFileSetting GetSavedSolutionFileSetting(Type type)
        {
            return SavedSolutionFileSettings.Any(sfs => sfs.Class == type)
                ? SavedSolutionFileSettings.First(sfs => sfs.Class == type)
                : null;
        }

        /// <summary>
        /// defines these types are stored in the visual studio solution with the given file name
        /// if no enytry then it is stored in the user settings file for global use (rather than solution specific)
        /// </summary>
        private IEnumerable<SaveSolutionFileSetting> SavedSolutionFileSettings
        {
            get
            {
                return new []
                {
                    new SaveSolutionFileSetting(typeof(XrmPackageSettings), "xrmpackage.xrmsettings", true),
                    new SaveSolutionFileSetting(typeof(XrmRecordConfiguration), "solution.xrmconnection", true)
                };
            }
        }

        void ISettingsManager.ProcessNamespaceChange(string newNamespace, string oldNamespace)
        {
        }

        private class SaveSolutionFileSetting
        {
            public SaveSolutionFileSetting(Type type, string fileName, bool usePersonalisedFile)
            {
                Class = type;
                FileName = fileName;
                UsePersonalisedFile = usePersonalisedFile;
            }

            public Type Class { get; set; }
            public string FileName { get; set; }
            public bool UsePersonalisedFile { get; set; }
            public string PersonalisedFileName
            {
                get
                {
                    return Environment.UserName?.ToLower() + "." + FileName;
                }
            }
        }
    }
}
