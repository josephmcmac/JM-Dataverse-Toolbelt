using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.XRM.VSIX.Dialogs
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
            string read = VisualStudioService.GetSolutionItemText(MapTypeToFileName(settingsType ?? typeof(TSettingsObject)));
            if (string.IsNullOrEmpty(read))
                return (TSettingsObject)type.CreateFromParameterlessConstructor();
            return (TSettingsObject)JsonHelper.JsonStringToObject(read, type);
        }

        public void SaveSettingsObject(object settingsObject, Type settingsType = null)
        {
            VisualStudioService.AddSolutionItem(MapTypeToFileName(settingsType ?? settingsObject.GetType()), settingsObject);
        }

        private string MapTypeToFileName(Type type)
        {
            if (type == typeof(XrmPackageSettings))
                return "xrmpackage.xrmsettings";
            throw new NotImplementedException(string.Format("Only the type {0} is implemented for settings", typeof(XrmPackageSettings).Name));
        }
    }
}
