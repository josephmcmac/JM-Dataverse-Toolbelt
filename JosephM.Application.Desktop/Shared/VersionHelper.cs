using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Shared
{
    public static class VersionHelper
    {
        public static bool IsNewerVersion(string latestVersionString, string thisVersionString)
        {
            var isNewer = false;
            if (thisVersionString != null && latestVersionString != null)
            {
                var latestNumbers = ParseVersionNumbers(latestVersionString);
                var thisNumbers = ParseVersionNumbers(thisVersionString);
                isNewer = IsNewerVersion(latestNumbers, thisNumbers);
            }

            return isNewer;
        }

        private static bool IsNewerVersion(IEnumerable<int> latestNumbers, IEnumerable<int> thisNumbers)
        {
            if (!latestNumbers.Any())
            {
                return false;
            }
            else if (!thisNumbers.Any())
            {
                if (latestNumbers.All(i => i == 0))
                    return false;
                else
                    return true;
            }
            else if (thisNumbers.First() > latestNumbers.First())
                return false;
            else if (latestNumbers.First() > thisNumbers.First())
                return true;
            else
                return IsNewerVersion(latestNumbers.Skip(1).ToArray(), thisNumbers.Skip(1).ToArray());
        }

        private static IEnumerable<int> ParseVersionNumbers(string versionString)
        {
            var splitVersion = versionString.Split('.');

            var splitLatestInts = new List<int>();
            if (versionString != null)
            {
                foreach (var item in versionString.Split('.'))
                {
                    int parsed = 0;
                    if (!int.TryParse(item, out parsed))
                    {
                        throw new Exception($"Error parsing version numbers. The version/release string was '{versionString}'");
                    }
                    splitLatestInts.Add(parsed);
                }
            }
            return splitLatestInts;
        }
    }
}
