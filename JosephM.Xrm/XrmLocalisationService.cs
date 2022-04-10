using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;
using System.Linq;

namespace JosephM.Xrm
{
    public class XrmLocalisationService
    {
        public XrmLocalisationService(XrmService xrmService, Guid userId)
        {
            XrmService = xrmService;
            CurrentUserId = userId;
        }

        private bool _userSettingsLoaded = false;
        private Entity _userSettings;
        private Entity UserSettings
        {
            get
            {
                if (!_userSettingsLoaded)
                {
                    _userSettings = XrmService.GetFirst(Entities.usersettings, Fields.usersettings_.systemuserid, CurrentUserId, new[] { Fields.usersettings_.timezonecode, Fields.usersettings_.dateformatstring, Fields.usersettings_.timeformatstring, Fields.usersettings_.currencysymbol, Fields.usersettings_.currencydecimalprecision, Fields.usersettings_.numberseparator, Fields.usersettings_.decimalsymbol, Fields.usersettings_.negativecurrencyformatcode, Fields.usersettings_.currencyformatcode, Fields.usersettings_.negativeformatcode, Fields.usersettings_.numbergroupformat
                    });
                    _userSettingsLoaded = true;
                }
                return _userSettings;
            }
        }

        public DateTime ConvertUtcToLocalTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcDateTime, TargetTimeZoneId);
        }

        private Entity _organisation;
        private Entity Organisation
        {
            get
            {
                if (_organisation == null)
                {
                    var queryExpression = new QueryExpression(Entities.organization);
                    queryExpression.ColumnSet = new ColumnSet(Fields.organization_.dateformatstring, Fields.organization_.timeformatstring, Fields.organization_.currencysymbol, Fields.organization_.currencydecimalprecision, Fields.organization_.numberseparator, Fields.organization_.decimalsymbol, Fields.organization_.negativecurrencyformatcode, Fields.organization_.currencyformatcode, Fields.organization_.negativeformatcode, Fields.organization_.numbergroupformat);
                    _organisation = XrmService.RetrieveFirst(queryExpression);
                }
                return _organisation;
            }
        }

        private NumberFormatInfo _numberFormatInfo = null;
        public NumberFormatInfo NumberFormatInfo
        {
            get
            {
                if (_numberFormatInfo == null)
                {
                    var numberFormatInfo = new NumberFormatInfo
                    {
                        CurrencySymbol = UserSettings.GetStringField(Fields.usersettings_.currencysymbol)
                        ?? Organisation.GetStringField(Fields.organization_.currencysymbol) ?? NumberFormatInfo.InvariantInfo.CurrencySymbol,
                        CurrencyDecimalDigits = (int?)UserSettings.GetField(Fields.usersettings_.currencydecimalprecision) ?? (int?)Organisation.GetField(Fields.organization_.currencydecimalprecision) ?? NumberFormatInfo.InvariantInfo.CurrencyDecimalDigits,
                        CurrencyGroupSeparator = UserSettings.GetStringField(Fields.usersettings_.numberseparator) ?? Organisation.GetStringField(Fields.organization_.numberseparator) ?? NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator,
                        CurrencyDecimalSeparator = UserSettings.GetStringField(Fields.usersettings_.decimalsymbol) ?? Organisation.GetStringField(Fields.organization_.decimalsymbol) ?? NumberFormatInfo.InvariantInfo.CurrencyDecimalSeparator,
                        CurrencyNegativePattern = (int?)UserSettings.GetField(Fields.usersettings_.negativecurrencyformatcode) ?? (int?)Organisation.GetField(Fields.organization_.negativecurrencyformatcode) ?? NumberFormatInfo.InvariantInfo.CurrencyNegativePattern,
                        CurrencyPositivePattern = (int?)UserSettings.GetField(Fields.usersettings_.currencyformatcode) ?? (int?)Organisation.GetField(Fields.organization_.currencyformatcode) ?? NumberFormatInfo.InvariantInfo.CurrencyPositivePattern,

                        NumberGroupSeparator = UserSettings.GetStringField(Fields.usersettings_.numberseparator) ?? Organisation.GetStringField(Fields.organization_.numberseparator) ?? NumberFormatInfo.InvariantInfo.NumberGroupSeparator,
                        NumberDecimalSeparator = DecimalSeparator,
                        NumberNegativePattern = (int?)UserSettings.GetField(Fields.usersettings_.negativeformatcode) ?? (int?)Organisation.GetField(Fields.organization_.negativeformatcode) ?? NumberFormatInfo.InvariantInfo.NumberNegativePattern,
                    };
                    var numberGroupFormat = UserSettings.GetStringField(Fields.usersettings_.numbergroupformat) ?? UserSettings.GetStringField(Fields.organization_.numbergroupformat);
                    if (!string.IsNullOrWhiteSpace(numberGroupFormat))
                    {
                        try
                        {
                            var splitToInts = numberGroupFormat
                                .Split(',')
                                .Select(s => int.Parse(s))
                                .ToArray();
                            numberFormatInfo.CurrencyGroupSizes = splitToInts;
                            numberFormatInfo.NumberGroupSizes = splitToInts;
                        }
                        catch (Exception)
                        {
                            numberFormatInfo.CurrencyGroupSizes = NumberFormatInfo.InvariantInfo.CurrencyGroupSizes;
                            numberFormatInfo.NumberGroupSizes = NumberFormatInfo.InvariantInfo.NumberGroupSizes;
                        }
                    }
                    _numberFormatInfo = numberFormatInfo;
                }
                return _numberFormatInfo;
            }
        }

        public string DecimalSeparator
        {
            get
            {
                return UserSettings.GetStringField(Fields.usersettings_.decimalsymbol) ?? Organisation.GetStringField(Fields.organization_.decimalsymbol) ?? NumberFormatInfo.InvariantInfo.NumberDecimalSeparator;
            }
        }

        public string DateFormatString
        {
            get
            {
                var dateFormat = UserSettings.GetStringField(Fields.usersettings_.dateformatstring) ?? Organisation.GetStringField(Fields.organization_.dateformatstring) ?? "dd/MM/yyyy";
                return dateFormat;
            }
        }

        public string TimeFormatString
        {
            get
            {
                var dateFormat = UserSettings.GetStringField(Fields.usersettings_.timeformatstring) ?? Organisation.GetStringField(Fields.organization_.timeformatstring) ?? "h:mm tt";
                return dateFormat;
            }
        }

        private int UserTimeZoneCode
        {
            get
            {
                var userTimeZoneCode = UserSettings.GetField(Fields.usersettings_.timezonecode);
                if (userTimeZoneCode == null)
                {
                    throw new NullReferenceException("Error timezonecode is empty in the usersettings record");
                }
                return (int)userTimeZoneCode;
            }
        }

        private Entity _timeZone;
        private Entity TimeZone
        {
            get
            {
                if (_timeZone == null)
                {
                    _timeZone = XrmService.GetFirst(Entities.timezonedefinition, Fields.timezonedefinition_.timezonecode, UserTimeZoneCode, new[] { Fields.timezonedefinition_.standardname });
                }
                return _timeZone;
            }
        }

        public XrmService XrmService { get; }
        public Guid CurrentUserId { get; }

        public string TargetTimeZoneId => TimeZone.GetStringField(Fields.timezonedefinition_.standardname);

        public DateTime TargetTodayUniversal
        {
            get
            {
                var localNow = DateTime.Now;
                var targetNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(localNow,
                    TargetTimeZoneId);
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

        public string DateTimeFormatString
        {
            get
            {
                return $"{DateFormatString} {TimeFormatString}";
            }
        }

        public DateTime ConvertToTargetTime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TargetTimeZoneId);
        }

        public DateTime ConvertToTargetDayUtc(DateTime day)
        {
            var sourceDate = day.Date;
            var targetDayTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(day,
                TargetTimeZoneId);
            var difference = sourceDate - targetDayTime;
            return sourceDate.Add(difference).ToUniversalTime();
        }

        public DateTime AsUnspecifiedType(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Unspecified);
        }

        public DateTime ConvertTargetToUtc(DateTime date)
        {
            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById(TargetTimeZoneId));
        }

        public string ToDateDisplayString(DateTime dateTime)
        {
            return dateTime.ToString(DateFormatString);
        }

        public string ToDateTimeDisplayString(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormatString);
        }
    }
}
