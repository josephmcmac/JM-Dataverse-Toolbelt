using JosephM.Application.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.XRM.VSIX.Dialogs
{
    public class VsixSettingsManager : ISettingsManager
    {
        public TSettingsObject Resolve<TSettingsObject>(Type settingsType = null) where TSettingsObject : new()
        {
            //okay need to get the package settings object if implementes this

            throw new NotImplementedException();
        }

        public void SaveSettingsObject(object settingsObject, Type settingsType = null)
        {
            throw new NotImplementedException();
        }
    }
}
