using System;

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
                var localNow = DateTime.Now;
                var targetNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(localNow,
                    LocalisationSettings.TargetTimeZoneId);
                var difference = localNow - targetNow;
                return targetNow.Date.Add(difference).ToUniversalTime();
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

        public DateTime ConvertToTargetTime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, LocalisationSettings.TargetTimeZoneId);
        }

        public DateTime ConvertToTargetDayUtc(DateTime day)
        {
            var sourceDate = day.Date;
            var targetDayTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(day,
                LocalisationSettings.TargetTimeZoneId);
            var difference = sourceDate - targetDayTime;
            return sourceDate.Add(difference).ToUniversalTime();
        }

        public DateTime AsUnspecifiedType(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Unspecified);
        }

        public DateTime ConvertTargetToUtc(DateTime date)
        {
            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById(LocalisationSettings.TargetTimeZoneId));
        }
    }
}
