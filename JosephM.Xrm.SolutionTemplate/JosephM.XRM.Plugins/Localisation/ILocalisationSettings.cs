using System.Globalization;

namespace $safeprojectname$.Localisation
{
    public interface ILocalisationSettings
    {
        string TargetTimeZoneId { get; }
        string DateFormatString { get; }
        string TimeFormatString { get; }
        string DecimalSeparator { get; }
        NumberFormatInfo NumberFormatInfo { get; }
    }
}