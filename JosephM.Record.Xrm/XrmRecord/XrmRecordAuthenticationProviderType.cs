using System.ComponentModel;

namespace JosephM.Record.Xrm.XrmRecord
{
    public enum XrmRecordAuthenticationProviderType
    {
        [Description("Windows Integrated")] None = 0,
        ActiveDirectory = 1,
        Federation = 2,
        [Description("Live Id / Office 365")] LiveId = 3,
        OnlineFederation = 4,
    }
}