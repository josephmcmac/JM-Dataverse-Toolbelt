using System;

namespace JosephM.Application.Application
{
    public interface ISettingsManager
    {
        TSettingsObject Resolve<TSettingsObject>(Type settingsType = null) where TSettingsObject : new();
        void SaveSettingsObject(object settingsObject, Type settingsType = null);

        void ProcessNamespaceChange(string newNamespace, string oldNamespace);
    }
}