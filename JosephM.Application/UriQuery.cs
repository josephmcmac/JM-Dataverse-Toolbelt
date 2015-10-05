#region

using System.Collections.Generic;
using JosephM.Core.Serialisation;

#endregion

namespace JosephM.Application
{
    public class UriQuery
    {
        private readonly SortedDictionary<string, string> _arguments = new SortedDictionary<string, string>();

        public void Add(string key, string value)
        {
            _arguments.Add(key, value);
        }

        public IDictionary<string, string> Arguments
        {
            get { return _arguments; }
        }

        public void AddObject<T>(string key, T objectValue)
        {
            Add(key, JsonHelper.ObjectToJsonString<T>(objectValue));
        }
    }
}