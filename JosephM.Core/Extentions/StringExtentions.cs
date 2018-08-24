#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

#endregion

namespace JosephM.Core.Extentions
{
    public static class StringExtentions
    {
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

        public static T ParseEnum<T>(this string enumString)
        {
            return (T) Enum.Parse(typeof (T), enumString);
        }

        public static string ReplicateString(this string aString, int numberOfTimes)
        {
            if (aString == null)
                return null;
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < numberOfTimes; i++)
                stringBuilder.Append(aString);
            return stringBuilder.ToString();
        }

        public static string SplitCamelCase(this string stringToSplit)
        {
            var result = stringToSplit;
            //probably a one statement regular expression but just do this way

            if (result != null && result.Length > 1)
            {
                var stringBuilder = new StringBuilder();
                foreach (var char_ in result)
                {
                    if (char_ >= 'A' && char_ <= 'Z' && stringBuilder.Length != 0 &&
                        stringBuilder[stringBuilder.Length - 1] != ' ')
                        stringBuilder.Append(" " + char_.ToString().ToUpper());
                    else
                        stringBuilder.Append(char_);
                }
                result = stringBuilder.ToString();
            }
            return result;
        }

        public static bool IsNullOrWhiteSpace(this string stringToCheck)
        {
            return String.IsNullOrWhiteSpace(stringToCheck);
        }

        public static string Left(this string text, int length)
        {
            if (!string.IsNullOrEmpty(text) && text.Length > length)
                return text.Substring(0, length);
            else
                return text;
        }

        public static bool Like(this string stringToMatch, string likePattern)
        {
            var regexPattern = Regex.Replace(
                likePattern,
                @"[%_]|\[[^]]*\]|[^%_[]+",
                match =>
                {
                    if (match.Value == "%")
                    {
                        return ".*";
                    }
                    if (match.Value == "_")
                    {
                        return ".";
                    }
                    if (match.Value.StartsWith("[") && match.Value.EndsWith("]"))
                    {
                        return match.Value;
                    }
                    return Regex.Escape(match.Value);
                });
            return stringToMatch != null && new Regex(regexPattern).IsMatch(stringToMatch);
        }

        public static string StripXmlTags(this string xmlString)
        {
            return Regex.Replace(xmlString, "<[^>]+>", string.Empty);
        }

        public static string StripHtml(this string source)
        {
            if (source == null)
                return null;
            try
            {
                var result = source.Replace("\r", " ");
                result = result.Replace("</p>", "</p>");
                result = result.Replace("</li>", "</li>" + Environment.NewLine);
                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                
                // Replace line breaks with space
                // because browsers inserts space
                //result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = Regex.Replace(result,
                    @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = Regex.Replace(result,
                    @"<( )*head([^>])*>", "<head>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<( )*(/)( )*head( )*>)", "</head>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(<head>).*(</head>)", string.Empty,
                    RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = Regex.Replace(result,
                    @"<( )*script([^>])*>", "<script>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<( )*(/)( )*script( )*>)", "</script>",
                    RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<script>).*(</script>)", string.Empty,
                    RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = Regex.Replace(result,
                    @"<( )*style([^>])*>", "<style>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<( )*(/)( )*style( )*>)", "</style>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(<style>).*(</style>)", string.Empty,
                    RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = Regex.Replace(result,
                    @"<( )*td([^>])*>", "\t",
                    RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = Regex.Replace(result,
                    @"<( )*br( )*>", "\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"<( )*li( )*>", "\r",
                    RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = Regex.Replace(result,
                    @"<( )*div([^>])*>", "\r\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"<( )*tr([^>])*>", "\r\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"<( )*p([^>])*>", "\r\r",
                    RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = Regex.Replace(result,
                    @"<[^>]*>", string.Empty,
                    RegexOptions.IgnoreCase);

                // replace special characters:
                result = Regex.Replace(result,
                    @" ", " ",
                    RegexOptions.IgnoreCase);

                result = Regex.Replace(result,
                    @"&bull;", " * ",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&lsaquo;", "<",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&rsaquo;", ">",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&trade;", "(tm)",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&frasl;", "/",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&lt;", "<",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&gt;", ">",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&copy;", "(c)",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&reg;", "(r)",
                    RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = Regex.Replace(result,
                    @"&(.{2,6});", string.Empty,
                    RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = Regex.Replace(result,
                    "(\r)( )+(\r)", "\r\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(\t)( )+(\t)", "\t\t",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(\t)( )+(\r)", "\t\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(\r)( )+(\t)", "\r\t",
                    RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = Regex.Replace(result,
                    "(\r)(\t)+(\r)", "\r\r",
                    RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = Regex.Replace(result,
                    "(\r)(\t)+", "\r\t",
                    RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                var breaks = "\r\r\r";
                // Initial replacement target string for tabs
                var tabs = "\t\t\t\t\t";
                for (var index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                result = result.Replace("\r", Environment.NewLine);

                // That's it.
                return result;
            }
            catch (Exception ex)
            {
                return "Error Passing Html: " + ex.DisplayString();
            }
        }

        public static IEnumerable<int> ParseRanges(this string rangeString)
        {

            if (rangeString.IsNullOrWhiteSpace())
                return new int[0];
            var validCharacters = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', ',', ' ' };
            if (rangeString.Any(c => !validCharacters.Contains(c)))
                throw new Exception("String Must Be Numeric With - and , separators");
            var result = new List<int>();
            while (true)
            {
                var index = rangeString.IndexOf(",");
                if (index != -1)
                {
                    var part = rangeString.Substring(0, index);
                    if (part.Contains("-"))
                    {
                        var split = part.Split('-');
                        try
                        {
                            var start = int.Parse(split[0].Trim());
                            var end = int.Parse(split[1].Trim());
                            if (start > end)
                                throw new Exception(string.Format("Range {0} To {1} Is Invalid. End Must Be Greater Than Start", start, end));
                            for (var i = start; i <= end; i++)
                                result.Add(i);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Invalid Range Value Of {0}", part), ex);
                        }
                    }
                    else
                        result.Add(int.Parse(part.Trim()));
                    rangeString = rangeString.Substring(index + 1);
                }
                else
                {
                    var part = rangeString;
                    if (part.Contains("-"))
                    {
                        var split = part.Split('-');
                        try
                        {
                            var start = int.Parse(split[0].Trim());
                            var end = int.Parse(split[1].Trim());
                            if (start > end)
                                throw new Exception(string.Format("Range {0} To {1} Is Invalid. End Must Be Greater Than Start", start, end));
                            for (var i = start; i <= end; i++)
                                result.Add(i);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Invalid Range Value Of {0}", part), ex);
                        }
                    }
                    else
                    {
                        result.Add(int.Parse(part.Trim()));
                    }
                    break;
                }
            }
            return result.Distinct();
        }

        public static string ToTitleCase(this string severalWords)
        {
            if (severalWords == null)
                return null;
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(severalWords.ToLower());
        }
    }
}