using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JosephM.Core.Serialisation
{
    /// <summary>
    ///     General Use Utility Methods For IRecords
    /// </summary>
    public static class JsonHelper
    {

        public static string ObjectToJsonString<T>(T objectValue)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, objectValue);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static object JsonStringToObject(string jsonString, Type type)
        {
            object theObject;
            var serializer = new DataContractJsonSerializer(type);
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                theObject = serializer.ReadObject(stream);
            }
            return theObject;
        }
    }
}