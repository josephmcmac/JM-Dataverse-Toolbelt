using JosephM.Record.Attributes;
using JosephM.Record.Metadata;

namespace JosephM.Record.Query
{
    public enum ConditionType
    {
        Equal,
        NotEqual,
        Null,
        NotNull,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        GreaterThan,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        LessThan,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        GreaterEqual,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        LessEqual,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        BeginsWith,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        DoesNotBeginWith,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        EndsWith,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        DoesNotEndWith,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        Like,
        [ValidForFieldTypes(RecordFieldType.Owner)]
        EqualUserId,
        [ValidForFieldTypes(RecordFieldType.Owner)]
        NotEqualUserId,
        [ValidForFieldTypes(RecordFieldType.Owner)]
        EqualUserTeams,
        [ValidForFieldTypes(RecordFieldType.Owner)]
        EqualUserOrUserTeams,
        [ValidForFieldTypes(RecordFieldType.Date)]
        Yesterday,
        [ValidForFieldTypes(RecordFieldType.Date)]
        Today,
        [ValidForFieldTypes(RecordFieldType.Date)]
        Tomorrow,
        [ValidForFieldTypes(RecordFieldType.Date)]
        Last7Days,
        [ValidForFieldTypes(RecordFieldType.Date)]
        Next7Days,
        [ValidForFieldTypes(RecordFieldType.Date)]
        LastWeek,
        [ValidForFieldTypes(RecordFieldType.Date)]
        ThisWeek,
        [ValidForFieldTypes(RecordFieldType.Date)]
        NextWeek,
        [ValidForFieldTypes(RecordFieldType.Date)]
        LastMonth,
        [ValidForFieldTypes(RecordFieldType.Date)]
        ThisMonth,
        [ValidForFieldTypes(RecordFieldType.Date)]
        NextMonth,
        [ValidForFieldTypes(RecordFieldType.Date)]
        On,
        [ValidForFieldTypes(RecordFieldType.Date)]
        OnOrBefore,
        [ValidForFieldTypes(RecordFieldType.Date)]
        OnOrAfter,
        [ValidForFieldTypes(RecordFieldType.Date)]
        LastYear,
        [ValidForFieldTypes(RecordFieldType.Date)]
        ThisYear,
        [ValidForFieldTypes(RecordFieldType.Date)]
        NextYear,
        [ValidForFieldTypes(RecordFieldType.Date)]
        NotOn,
        [ValidForFieldTypes(RecordFieldType.Date)]
        ThisFiscalYear,
        [ValidForFieldTypes(RecordFieldType.Date)]
        ThisFiscalPeriod,
        [ValidForFieldTypes(RecordFieldType.Date)]
        NextFiscalYear,
        [ValidForFieldTypes(RecordFieldType.Date)]
        NextFiscalPeriod,
        [ValidForFieldTypes(RecordFieldType.Date)]
        LastFiscalYear,
        [ValidForFieldTypes(RecordFieldType.Date)]
        LastFiscalPeriod,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        NotLike,
        //this actually just a fake to prevent it displaying in UI at all
        //as haven't implemented multiselect
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        In,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NotIn,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        Between,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NotBetween,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXHours,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXHours,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXDays,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXDays,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXWeeks,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXWeeks,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXMonths,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXMonths,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXYears,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXYears,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        EqualBusinessId,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NotEqualBusinessId,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        ChildOf,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        Mask,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NotMask,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        MasksSelect,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        Contains,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        DoesNotContain,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        EqualUserLanguage,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        OlderThanXMonths,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXFiscalYears,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXFiscalYears,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        LastXFiscalPeriods,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        NextXFiscalPeriods,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        InFiscalYear,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        InFiscalPeriod,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        InFiscalPeriodAndYear,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        InOrBeforeFiscalPeriodAndYear,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        InOrAfterFiscalPeriodAndYear,
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        UnmatchedConditionType = 999999,
    }
}