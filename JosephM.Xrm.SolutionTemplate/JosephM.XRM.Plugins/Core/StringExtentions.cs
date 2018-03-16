using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace $safeprojectname$.Core
{
    public static class StringExtentions
    {
        public static string Left(this string text, int length)
        {
            if (!string.IsNullOrEmpty(text) && text.Length > length)
                return text.Substring(0, length);
            else
                return text;
        }

        public static string JoinGrammarOr(this IEnumerable<string> strings)
        {
            var stringBuilder = new StringBuilder();
            var n = strings.Count();
            for (var i = 0; i < n; i++)
            {
                if (i == n - 1)
                    stringBuilder.Append(strings.ElementAt(i));
                else if (i < n - 2)
                    stringBuilder.Append(strings.ElementAt(i) + ", ");
                else
                    stringBuilder.Append(strings.ElementAt(i) + " or ");
            }
            return stringBuilder.ToString();
        }

        public static string JoinGrammarAnd(this IEnumerable<string> strings)
        {
            var stringBuilder = new StringBuilder();
            var n = strings.Count();
            for (var i = 0; i < n; i++)
            {
                if (i == n - 1)
                    stringBuilder.Append(strings.ElementAt(i));
                else if (i < n - 2)
                    stringBuilder.Append(strings.ElementAt(i) + ", ");
                else
                    stringBuilder.Append(strings.ElementAt(i) + " and ");
            }
            return stringBuilder.ToString();
        }

        public static string ReplicateString(this string theString, int times)
        {
            var stringBuilder = new StringBuilder();
            if (times >= 0)
            {
                for (var i = 0; i < times; i++)
                    stringBuilder.Append(theString);
            }
            return stringBuilder.ToString();
        }
    }
}