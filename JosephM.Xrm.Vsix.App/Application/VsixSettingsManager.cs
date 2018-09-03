using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using JosephM.Application.Desktop.Application;

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
            if (SolutionSettingTypes.ContainsKey(type))
            {
                var fileName = MapTypeToFileName(settingsType ?? typeof(TSettingsObject));

                string read = fileName != null ? VisualStudioService.GetSolutionItemText(fileName) : null;

                if (string.IsNullOrEmpty(read))
                    return new TSettingsObject();
                else
                {
                    if (type == typeof(XrmRecordConfiguration))
                    {
                        //csan't recall why but this type is saved as a dictionary rather than just an object
                        //maybe due to interfaces
                        var dictionary = string.IsNullOrEmpty(read)
                            ? new Dictionary<string, string>()
                            : (Dictionary<string, string>)
                                JsonHelper.JsonStringToObject(read, typeof(Dictionary<string, string>));

                        var xrmConfig = new TSettingsObject();
                        foreach (var prop in xrmConfig.GetType().GetReadWriteProperties())
                        {
                            if (dictionary.ContainsKey(prop.Name))
                                xrmConfig.SetPropertyByString(prop.Name, dictionary[prop.Name]);
                        }
                        return xrmConfig;
                    }
                    else
                        return (TSettingsObject)JsonHelper.JsonStringToObject(read, type);
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
            if (SolutionSettingTypes.ContainsKey(type))
                VisualStudioService.AddSolutionItem(MapTypeToFileName(type), settingsObject);
            else
                DesktopSettingsManager.SaveSettingsObject(settingsObject, settingsType: settingsType);
        }

        private string MapTypeToFileName(Type type)
        {
            return SolutionSettingTypes.ContainsKey(type)
                ? SolutionSettingTypes[type]
                : null;
        }

        /// <summary>
        /// defines these types are stored in the visual studio solution with the given file name
        /// if no enytry then it is stored in the user settings file for global use (rather than solution specific)
        /// </summary>
        private Dictionary<Type, string> SolutionSettingTypes
        {
            get
            {
                return new Dictionary<Type, string>
                {
                    { typeof(XrmPackageSettings), "xrmpackage.xrmsettings" },
                    { typeof(XrmRecordConfiguration), "solution.xrmconnection" }
                };
            }
        }

        void ISettingsManager.ProcessNamespaceChange(string newNamespace, string oldNamespace)
        {
        }
    }
}
