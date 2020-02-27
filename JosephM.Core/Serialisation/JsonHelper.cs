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
        public static string ObjectAsTypeToJsonString(object objectValue, bool format = false)
        {
            if (format)
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        stream, Encoding.UTF8, true, true, "  "))
                    {
                        var serializer = new DataContractJsonSerializer(objectValue.GetType());
                        serializer.WriteObject(writer, objectValue);
                        writer.Flush();
                    }
                    return Encoding.Default.GetString(stream.ToArray());
                }
            }
            else
            {
                var serializer = new DataContractJsonSerializer(objectValue.GetType());
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, objectValue);
                    return Encoding.Default.GetString(stream.ToArray());
                }
            }
        }

        public static string ObjectToJsonString<T>(T objectValue, bool format = false)
        {
            if (format)
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        stream, Encoding.UTF8, true, true, "  "))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        serializer.WriteObject(writer, objectValue);
                        writer.Flush();
                    }
                    return Encoding.Default.GetString(stream.ToArray());
                }
            }
            else
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, objectValue);
                    return Encoding.Default.GetString(stream.ToArray());
                }
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