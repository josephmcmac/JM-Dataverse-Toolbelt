using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;

namespace JosephM.Core.Test
{
    public class CoreTest
    {
        public CoreTest()
        {
            Controller = new LogController();
        }

        protected LogController Controller { get; private set; }

        public virtual string TestingFolder
        {
            get { return TestConstants.TestFolder; }
        }

        public virtual string TestingString
        {
            get { return TestConstants.TestingString; }
        }

        public static string Replicate(char character, int times)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < times; i++)
                stringBuilder.Append(character);
            return stringBuilder.ToString();
        }

        public static string CreateRandomAlphaNumericString(int length)
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = new char[length];
            var rd = new Random();
            for (var i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            return new string(chars);
        }

        public bool IsPrimitiveType(object value)
        {
            return value is string || value is Enum || value is bool;
        }

        public void PopulateObject(object instance)
        {
            var type = instance.GetType();
            foreach (var property in type.GetProperties())
            {
                var value = InstantiatePopulated(property.PropertyType);

                property.GetSetMethod().Invoke(instance, new object[] {value});
            }
        }

        private object InstantiatePopulated(Type type)
        {
            object value = null;
            if (type == typeof (string))
                value = Guid.NewGuid().ToString(); //required when mapping to Guid
            else if (type == typeof (Guid))
                value = Guid.NewGuid();
            else if (type == typeof (bool))
                value = true;
            else if (type == typeof (RecordType))
                value = new RecordType("type", "type");
            else if (type == typeof (RecordField))
                value = new RecordField("field", "field");
            else if (type == typeof (Password))
                value = new Password("FakePassword", false, true);
            else if (type.IsEnum)
            {
                var options = type.GetEnumValues();
                foreach (var option in options)
                {
                    value = option;
                    break;
                }
            }
            else if (type.IsIEnumerableOfT())
            {
                var tType = type.GetGenericArguments()[0];
                var objectList = new List<object>();
                {
                    for (var i = 0; i < 3; i++)
                    {
                        objectList.Add(InstantiatePopulated(tType));
                    }
                }
                value = tType.ToNewTypedEnumerable(objectList);
            }
            else if (type.HasParameterlessConstructor())
                value = type.CreateFromParameterlessConstructor();
            else if (type == typeof (ExtensionDataObject))
                value = null;
            else
                throw new Exception(string.Format("Type {0} Not Matched For Initialising Property",
                    type));
            return value;
        }
    }
}