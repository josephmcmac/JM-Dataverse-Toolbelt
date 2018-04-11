using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Application
{
    public class VsixSettingsManager : ISettingsManager
    {
        private IVisualStudioService VisualStudioService { get; set; }

        public VsixSettingsManager(IVisualStudioService visualStudioService)
        {
            VisualStudioService = visualStudioService;
        }

        public TSettingsObject Resolve<TSettingsObject>(Type settingsType = null) where TSettingsObject : new()
        {

            var type = settingsType ?? typeof(TSettingsObject);
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

        public void SaveSettingsObject(object settingsObject, Type settingsType = null)
        {
            //thsi bit of a hack but the XrmConnections module tries to save a settibngs object
            //we dont want because we use the package settings
            //instead so lets just ignore it
            if (settingsObject is SavedXrmConnections)
                return;
            VisualStudioService.AddSolutionItem(MapTypeToFileName(settingsType ?? settingsObject.GetType()), settingsObject);
        }

        private string MapTypeToFileName(Type type)
        {
            if (type == typeof(XrmPackageSettings))
                return "xrmpackage.xrmsettings";
            if (type == typeof(XrmRecordConfiguration))
                return "solution.xrmconnection";
            return null;
        }

        void ISettingsManager.ProcessNamespaceChange(string newNamespace, string oldNamespace)
        {
        }
    }
}
