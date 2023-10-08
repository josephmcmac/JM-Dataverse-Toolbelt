using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Schema;
using System.Globalization;
using System;
using $safeprojectname$.Xrm;
using System.Linq;

namespace $safeprojectname$.Localisation
{
    public class UserLocalisationSettings : ILocalisationSettings
    {
        public UserLocalisationSettings(XrmService xrmService, Guid userId)
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
                    var userSettingsQuery = new QueryExpression(Entities.usersettings);
                    userSettingsQuery.ColumnSet = new ColumnSet(Fields.usersettings_.timezonecode, Fields.usersettings_.dateformatstring, Fields.usersettings_.timeformatstring, Fields.usersettings_.currencysymbol, Fields.usersettings_.currencydecimalprecision, Fields.usersettings_.numberseparator, Fields.usersettings_.decimalsymbol, Fields.usersettings_.negativecurrencyformatcode, Fields.usersettings_.currencyformatcode, Fields.usersettings_.negativeformatcode, Fields.usersettings_.numbergroupformat);
                    userSettingsQuery.Criteria.AddCondition(Fields.usersettings_.systemuserid, ConditionOperator.Equal, CurrentUserId);
                    var timeZoneDefinitionLink = userSettingsQuery.AddLink(Entities.timezonedefinition, Fields.usersettings_.timezonecode, Fields.timezonedefinition_.timezonecode, JoinOperator.LeftOuter);
                    timeZoneDefinitionLink.EntityAlias = "TZ";
                    timeZoneDefinitionLink.Columns = new ColumnSet(Fields.timezonedefinition_.standardname);
                    _userSettings = XrmService.RetrieveFirst(userSettingsQuery);
                    _userSettingsLoaded = true;
                }
                return _userSettings;
            }
        }
        private Entity _organisation;
        private Entity Organisation
        {
            get
            {
                if (_organisation == null)
                {
                    var organisationQuery = new QueryExpression(Entities.organization);
                    organisationQuery.ColumnSet = new ColumnSet(Fields.organization_.dateformatstring, Fields.organization_.timeformatstring, Fields.organization_.currencysymbol, Fields.organization_.currencydecimalprecision, Fields.organization_.numberseparator, Fields.organization_.decimalsymbol, Fields.organization_.negativecurrencyformatcode, Fields.organization_.currencyformatcode, Fields.organization_.negativeformatcode, Fields.organization_.numbergroupformat, Fields.organization_.utcconversiontimezonecode);
                    var timeZoneDefinitionLink = organisationQuery.AddLink(Entities.timezonedefinition, Fields.organization_.utcconversiontimezonecode, Fields.timezonedefinition_.timezonecode, JoinOperator.LeftOuter);
                    timeZoneDefinitionLink.EntityAlias = "TZ";
                    timeZoneDefinitionLink.Columns = new ColumnSet(Fields.timezonedefinition_.standardname);
                    _organisation = XrmService.RetrieveFirst(organisationQuery);
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
                        NumberGroupSeparator = UserSettings.GetStringField(Fields.usersettings_.numberseparator) ?? Organisation.GetStringField(Fields.organization_.numberseparator) ?? NumberFormatInfo.InvariantInfo.NumberGroupSeparator,
                        NumberDecimalSeparator = DecimalSeparator,
                    };
                    var currencyFormatCode = UserSettings.GetField(Fields.usersettings_.currencyformatcode) ?? Organisation.GetField(Fields.organization_.currencyformatcode);
                    if (currencyFormatCode is OptionSetValue osvCFC)
                    {
                        numberFormatInfo.CurrencyPositivePattern = osvCFC.Value;
                    }
                    else if (currencyFormatCode is int intCFC)
                    {
                        numberFormatInfo.CurrencyPositivePattern = intCFC;
                    }
                    else
                    {
                        numberFormatInfo.CurrencyPositivePattern = NumberFormatInfo.InvariantInfo.CurrencyPositivePattern;
                    }

                    var negativeFormatCode = UserSettings.GetField(Fields.usersettings_.negativeformatcode) ?? Organisation.GetField(Fields.organization_.negativeformatcode);
                    if (negativeFormatCode is OptionSetValue osvNFC)
                    {
                        numberFormatInfo.NumberNegativePattern = osvNFC.Value;
                    }
                    else if (negativeFormatCode is int intNFC)
                    {
                        numberFormatInfo.NumberNegativePattern = intNFC;
                    }
                    else
                    {
                        numberFormatInfo.NumberNegativePattern = NumberFormatInfo.InvariantInfo.NumberNegativePattern;
                    }
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

        public XrmService XrmService { get; }
        public Guid CurrentUserId { get; }

        public string TargetTimeZoneId
        {
            get
            {
                //(GMT) Coordinated Universal Time
                var timeZoneName = UserSettings.GetStringField($"TZ.{Fields.timezonedefinition_.standardname}");
                if (string.IsNullOrWhiteSpace(timeZoneName))
                {
                    timeZoneName = Organisation.GetStringField($"TZ.{Fields.timezonedefinition_.standardname}");
                }
                if (string.IsNullOrWhiteSpace(timeZoneName))
                {
                    timeZoneName = "UTC";
                }
                return timeZoneName;
            }
        }
    }
}