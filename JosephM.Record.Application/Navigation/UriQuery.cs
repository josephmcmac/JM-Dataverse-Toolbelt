#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

#endregion

namespace JosephM.Record.Application.Navigation
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
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, objectValue);
                var jsonString = Encoding.Default.GetString(stream.ToArray());
                Add(key, jsonString);
            }
        }
    }
}