using System;
using System.Globalization;

namespace JosephM.Record.IService
{
    public interface IRecordLocalisationService
    {
        string DateTimeFormatString { get; }
        string DateFormatString { get; }
        NumberFormatInfo NumberFormatInfo { get; }
        DateTime ConvertUtcToLocalTime(DateTime utcDateTime);
    }
}
