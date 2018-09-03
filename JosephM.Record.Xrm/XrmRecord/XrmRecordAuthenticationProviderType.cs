using System.ComponentModel;

namespace JosephM.Record.Xrm.XrmRecord
{
    public enum XrmRecordAuthenticationProviderType
    {
        [Description("Windows Integrated")] None = 0,
        ActiveDirectory = 1,
        Federation = 2,
        [Description("Online / Office 365")] LiveId = 3,
        OnlineFederation = 4,
    }
}