using System;

namespace JosephM.Core.Utility
{
    public class TaskEstimator
    {
        public TaskEstimator(int toDo)
        {
            ToDo = toDo;
        }

        private int ToDo { get; set; }

        private DateTime _start = DateTime.UtcNow;

        private TimeSpan GetEstimateRemaining(int done)
        {
            var timeSpent = DateTime.UtcNow - _start;
            var seconds = timeSpent.TotalSeconds;
            var averagePer = seconds / done;
            var remaining = ToDo - done;
            var estimateRemaining = averagePer * remaining;
            return TimeSpan.FromSeconds(estimateRemaining);
        }

        private string GetEstimateRemainingDisplayString(int done)
        {
            if (done == 0)
                return "Unknown";
            var t = GetEstimateRemaining(done);
            if (t.TotalMinutes <= 1)
            {
                return $"{t.Seconds} second{AppendPlural(t.Seconds)}";
            }
            if (t.TotalMinutes <= 10)
            {
                var secondsPart = t.Seconds > 0 ? $" {t.Seconds} second{AppendPlural(t.Seconds)}" : null;
                return $"{ t.Minutes} minute{AppendPlural(t.Minutes)}{secondsPart}";
            }
            if (t.TotalHours <= 1)
            {
                return $"{t.Minutes} minute{AppendPlural(t.Minutes)}";
            }
            var minutesPart = t.Minutes > 0 ? $" {t.Minutes} minute{AppendPlural(t.Minutes)}" : null;
            var hours = t.TotalHours - (t.TotalHours % 1);
            return $"{hours} hour{AppendPlural(Convert.ToInt32(hours))}{minutesPart}";
        }

        private string AppendPlural(int number)
        {
            return number == 1 ? "" : "s";
        }

        public string GetProgressString(int countUpdated, string taskName = null)
        {
            var estimateRemainingString = countUpdated == 0 ? "" : $" - Estimated Time {GetEstimateRemainingDisplayString(countUpdated)}";
            return $"{(string.IsNullOrWhiteSpace(taskName) ? null : (taskName + " - "))}{ToDo - countUpdated} Remaining{estimateRemainingString}";
        }
    }
}
