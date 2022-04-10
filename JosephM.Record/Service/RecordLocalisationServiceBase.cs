using JosephM.Record.IService;
using System;
using System.Globalization;

namespace JosephM.Record.Service
{
    public class RecordLocalisationServiceBase : IRecordLocalisationService
    {
        public string DateTimeFormatString => $"{DateFormatString} {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}";

        public string DateFormatString => CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        public NumberFormatInfo NumberFormatInfo => CultureInfo.CurrentCulture.NumberFormat;

        public DateTime ConvertUtcToLocalTime(DateTime utcDateTime) => utcDateTime.ToLocalTime();
    }
}
