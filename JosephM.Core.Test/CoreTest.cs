using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JosephM.Core.Service;
using System.Reflection;
using System.Linq;

namespace JosephM.Core.Test
{
    public class CoreTest
    {
        public CoreTest()
        {
            FileUtility.DeleteFiles(TestingFolder);
            FileUtility.DeleteSubFolders(TestingFolder);
            Controller = new LogController();
            Controller.AddUi(new DebugUserInterface());
        }

        public static DirectoryInfo GetSolutionRootFolder()
        {
            var rootFolderName = "XRM-Developer-Tool";
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
            var directory = fileInfo.Directory;
            while (directory.Name != rootFolderName)
            {
                directory = directory.Parent;
                if (directory == null)
                    throw new NullReferenceException("Could not find solution root folder of name '" + rootFolderName + "' in " + fileInfo.FullName);
            }
            return directory;
        }

        protected LogController Controller { get; private set; }

        public static string TestingFolder
        {
            get
            {
                if (!Directory.Exists(TestConstants.TestFolder))
                    Directory.CreateDirectory(TestConstants.TestFolder);
                return TestConstants.TestFolder;
            }
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

        public static void PopulateObject(object instance)
        {
            var type = instance.GetType();
            foreach (var property in type.GetWritableProperties())
            {
                var value = InstantiatePopulated(property.PropertyType);
                var setMethod = property.GetSetMethod();
                if (setMethod != null)
                {
                    var parameters = setMethod.GetParameters();
                    if (parameters.Count() == 1)
                    {
                        property.GetSetMethod().Invoke(instance, new object[] { value });
                    }
                }
            }
        }

        private static object InstantiatePopulated(Type type)
        {
            if (type.Name == "Nullable`1")
                type = type.GetGenericArguments()[0];

            object value = null;
            if (type == typeof(string))
                value = Guid.NewGuid().ToString(); //required when mapping to Guid
            else if (type == typeof(Guid))
                value = Guid.NewGuid();
            else if (type == typeof(bool))
                value = true;
            else if (type == typeof(RecordType))
                value = new RecordType("type", "type");
            else if (type == typeof(RecordField))
                value = new RecordField("field", "field");
            else if (type == typeof(Folder))
                value = new Folder(TestingFolder);
            else if (type == typeof(int))
                value = 1;
            else if (type == typeof(Password))
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
            {
                value = type.CreateFromParameterlessConstructor();
                PopulateObject(value);
            }
            else if (type == typeof(ExtensionDataObject))
                value = null;
            else
                throw new Exception(string.Format("Type {0} Not Matched For Initialising Property",
                    type));
            return value;
        }

        public static string ReplicateString(string stringToReplicate, int times)
        {
            var stringer = new StringBuilder();
            for (var i = 0; i < times; i++)
                stringer.Append(stringToReplicate);
            return stringer.ToString();
        }

        public void WaitTillTrue(Func<bool> assertInTime, int seconds)
        {
            var secondsWaited = 0;
            while (!assertInTime())
            {
                secondsWaited++;
                if (secondsWaited > seconds)
                    Assert.Fail("Waited Too Long Without Meeting Test Criteria");
                Thread.Sleep(1000);
            }
        }

        public ServiceRequestController CreateServiceRequestController()
        {
            return new ServiceRequestController(Controller);
        }
    }
}