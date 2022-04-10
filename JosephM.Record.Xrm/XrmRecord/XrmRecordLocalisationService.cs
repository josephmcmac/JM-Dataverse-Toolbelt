using JosephM.Record.IService;
using JosephM.Xrm;
using System;
using System.Globalization;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordLocalisationService : IRecordLocalisationService
    {
        public XrmRecordLocalisationService(XrmLocalisationService xrmLocalisationService)
        {
            XrmLocalisationService = xrmLocalisationService;
        }

        public string DateTimeFormatString => XrmLocalisationService.DateTimeFormatString;

        public string DateFormatString => XrmLocalisationService.DateFormatString;

        public XrmLocalisationService XrmLocalisationService { get; }

        public NumberFormatInfo NumberFormatInfo => XrmLocalisationService.NumberFormatInfo;

        public DateTime ConvertUtcToLocalTime(DateTime utcDateTime)
        {
            return XrmLocalisationService.ConvertUtcToLocalTime(utcDateTime);
        }
    }
}
