#region

using System;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace JosephM.Core.Extentions
{
    public static class DateTimeExtentions
    {
        public static int GetAge(this DateTime dateOfBirth, DateTime asAt)
        {
            var dateOfBirthLocal = dateOfBirth.ToLocalTime();
            var asAtLocal = asAt.ToLocalTime();
            if (dateOfBirthLocal.Month > asAtLocal.Month ||
                (dateOfBirthLocal.Month == asAtLocal.Month && dateOfBirthLocal.Day > asAtLocal.Day))
                return asAtLocal.Year - dateOfBirthLocal.Year - 1;
            return asAtLocal.Year - dateOfBirthLocal.Year;
        }

        public static int GetYearsDuration(this DateTime startDate, DateTime endDate)
        {
            var duration = endDate.Year - startDate.Year;
            if (startDate.Month == 1 && startDate.Day == 1 && endDate.Month == 12 &&
                endDate.Day == 31)
                duration ++;
            return duration;
        }
    }
}