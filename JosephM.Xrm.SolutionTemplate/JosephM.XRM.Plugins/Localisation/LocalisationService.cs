using System;
using System.Globalization;
using System.Linq;

namespace $safeprojectname$.Localisation
{
    public class LocalisationService
    {
        public LocalisationService(ILocalisationSettings localisationSettings)
        {
            LocalisationSettings = localisationSettings;
        }

        public ILocalisationSettings LocalisationSettings { get; set; }

        public DateTime TargetTodayUniversal
        {
            get
            {
                var timeZoneId = LocalisationSettings.TargetTimeZoneId;
                var utcNow = DateTime.UtcNow;
                var targetNow = _utcNames.Contains(timeZoneId)
                    ? utcNow
                    : TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById(
                    LocalisationSettings.TargetTimeZoneId));
                var today = targetNow.Date;
                return today.ToUniversalTime();
            }
        }

        public DateTime TargetToday
        {
            get { return ConvertToTargetTime(TargetTodayUniversal); }
        }

        public DateTime TodayUnspecifiedType
        {
            get
            {
                var today = TargetToday;
                return new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Unspecified);
            }
        }

        public string DecimalSeparator
        {
            get
            {
                return LocalisationSettings.DecimalSeparator;
            }
        }

        public string DateFormatString
        {
            get
            {
                return LocalisationSettings.DateFormatString;
            }
        }

        public string TimeFormatString
        {
            get
            {
                return LocalisationSettings.TimeFormatString;
            }
        }

        public string DateTimeFormatString
        {
            get
            {
                return $"{DateFormatString} {TimeFormatString}";
            }
        }

        public DateTime ConvertToTargetTime(DateTime dateTime)
        {
            var timeZoneId = LocalisationSettings.TargetTimeZoneId;
            return _utcNames.Contains(timeZoneId)
                ? dateTime
                : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, timeZoneId);
        }

        public DateTime ConvertToTargetDayUtc(DateTime day)
        {
            var timeZoneId = LocalisationSettings.TargetTimeZoneId;
            var sourceDate = day.Date;
            var targetDayTime = _utcNames.Contains(timeZoneId)
                ? day
                : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(day, timeZoneId);
            var difference = sourceDate - targetDayTime;
            return sourceDate.Add(difference).ToUniversalTime();
        }

        public DateTime AsUnspecifiedType(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Unspecified);
        }

        public DateTime ConvertTargetToUtc(DateTime date)
        {
            var timeZoneId = LocalisationSettings.TargetTimeZoneId;
            return _utcNames.Contains(timeZoneId)
                ? date
                : TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public string ToDateDisplayString(DateTime dateTime)
        {
            return dateTime.ToString(LocalisationSettings.DateFormatString);
        }

        public string ToDateTimeDisplayString(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormatString);
        }

        public NumberFormatInfo NumberFormatInfo
        {
            get
            {
                return LocalisationSettings.NumberFormatInfo ?? NumberFormatInfo.InvariantInfo;
            }
        }

        private static string[] _utcNames = new[] { "Coordinated Universal Time", "UTC", "(GMT) Coordinated Universal Time" };
    }
}
