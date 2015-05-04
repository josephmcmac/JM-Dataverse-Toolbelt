using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel.Description;
using System.Xml.Serialization;

namespace JosephM.Xrm
{
    /// <summary>
    ///     Management utility for the Device Id
    /// </summary>
    internal static class DeviceIdManager
    {
        #region Fields

        public const int MaxDeviceNameLength = 24;
        public const int MaxDevicePasswordLength = 24;
        private static readonly Random RandomInstance = new Random();

        #endregion

        #region Constructor

        static DeviceIdManager()
        {
            PersistToFile = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates whether the registered device credentials should be persisted to the database
        /// </summary>
        public static bool PersistToFile { get; set; }

        /// <summary>
        ///     Indicates that the credentials should be persisted to the disk if registration fails with DeviceAlreadyExists.
        /// </summary>
        /// <remarks>
        ///     If the device already exists, there is a possibility that the credentials are the same as the current credentials
        ///     that
        ///     are being registered. This is especially true in automated environments where the same credentials are used
        ///     continually (to avoid
        ///     registering spurious device credentials.
        /// </remarks>
        public static bool PersistIfDeviceAlreadyExists { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Loads the device credentials (if they exist).
        /// </summary>
        /// <returns></returns>
        public static ClientCredentials LoadOrRegisterDevice()
        {
            return LoadOrRegisterDevice(null);
        }

        /// <summary>
        ///     Loads the device credentials (if they exist).
        /// </summary>
        /// <param name="deviceName">Device name that should be registered</param>
        /// <param name="devicePassword">Device password that should be registered</param>
        public static ClientCredentials LoadOrRegisterDevice(string deviceName, string devicePassword)
        {
            return LoadOrRegisterDevice(null, deviceName, devicePassword);
        }

        /// <summary>
        ///     Loads the device credentials (if they exist).
        /// </summary>
        /// <param name="issuerUri">URL for the current token issuer</param>
        /// <remarks>
        ///     The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
        /// </remarks>
        public static ClientCredentials LoadOrRegisterDevice(Uri issuerUri)
        {
            return LoadOrRegisterDevice(issuerUri, null, null);
        }

        /// <summary>
        ///     Loads the device credentials (if they exist).
        /// </summary>
        /// <param name="issuerUri">URL for the current token issuer</param>
        /// <param name="deviceName">Device name that should be registered</param>
        /// <param name="devicePassword">Device password that should be registered</param>
        /// <remarks>
        ///     The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
        /// </remarks>
        public static ClientCredentials LoadOrRegisterDevice(Uri issuerUri, string deviceName, string devicePassword)
        {
            var credentials = LoadDeviceCredentials(issuerUri);
            if (null == credentials)
            {
                credentials = RegisterDevice(Guid.NewGuid(), issuerUri, deviceName, devicePassword);
            }

            return credentials;
        }

        /// <summary>
        ///     Registers the given device with Live ID with a random application ID
        /// </summary>
        /// <returns>ClientCredentials that were registered</returns>
        public static ClientCredentials RegisterDevice()
        {
            return RegisterDevice(Guid.NewGuid());
        }

        /// <summary>
        ///     Registers the given device with Live ID
        /// </summary>
        /// <param name="applicationId">ID for the application</param>
        /// <returns>ClientCredentials that were registered</returns>
        public static ClientCredentials RegisterDevice(Guid applicationId)
        {
            return RegisterDevice(applicationId, null);
        }

        /// <summary>
        ///     Registers the given device with Live ID
        /// </summary>
        /// <param name="applicationId">ID for the application</param>
        /// <param name="issuerUri">URL for the current token issuer</param>
        /// <returns>ClientCredentials that were registered</returns>
        /// <remarks>
        ///     The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
        /// </remarks>
        public static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri)
        {
            return RegisterDevice(applicationId, issuerUri, null, null);
        }

        /// <summary>
        ///     Registers the given device with Live ID
        /// </summary>
        /// <param name="applicationId">ID for the application</param>
        /// <param name="deviceName">Device name that should be registered</param>
        /// <param name="devicePassword">Device password that should be registered</param>
        /// <returns>ClientCredentials that were registered</returns>
        public static ClientCredentials RegisterDevice(Guid applicationId, string deviceName, string devicePassword)
        {
            return RegisterDevice(applicationId, null, deviceName, devicePassword);
        }

        /// <summary>
        ///     Registers the given device with Live ID
        /// </summary>
        /// <param name="applicationId">ID for the application</param>
        /// <param name="issuerUri">URL for the current token issuer</param>
        /// <param name="deviceName">Device name that should be registered</param>
        /// <param name="devicePassword">Device password that should be registered</param>
        /// <returns>ClientCredentials that were registered</returns>
        /// <remarks>
        ///     The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
        /// </remarks>
        public static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri, string deviceName,
            string devicePassword)
        {
            if (string.IsNullOrEmpty(deviceName) && !PersistToFile)
            {
                throw new ArgumentNullException("deviceName",
                    "If PersistToFile is false, then deviceName must be specified.");
            }
            else if (string.IsNullOrEmpty(deviceName) != string.IsNullOrEmpty(devicePassword))
            {
                throw new ArgumentNullException("deviceName",
                    "Either deviceName/devicePassword should both be specified or they should be null.");
            }

            var device = GenerateDevice(deviceName, devicePassword);
            return RegisterDevice(applicationId, issuerUri, device);
        }

        /// <summary>
        ///     Loads the device's credentials from the file system
        /// </summary>
        /// <returns>Device Credentials (if set) or null</returns>
        public static ClientCredentials LoadDeviceCredentials()
        {
            return LoadDeviceCredentials(null);
        }

        /// <summary>
        ///     Loads the device's credentials from the file system
        /// </summary>
        /// <param name="issuerUri">URL for the current token issuer</param>
        /// <returns>Device Credentials (if set) or null</returns>
        /// <remarks>
        ///     The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
        /// </remarks>
        public static ClientCredentials LoadDeviceCredentials(Uri issuerUri)
        {
            //If the credentials should not be persisted to a file, then they won't be present on the disk.
            if (!PersistToFile)
            {
                return null;
            }

            var environment = DiscoverEnvironmentInternal(issuerUri);

            var device = ReadExistingDevice(environment);
            if (null == device || null == device.User)
            {
                return null;
            }

            return device.User.ToClientCredentials();
        }

        /// <summary>
        ///     Discovers the Windows Live environment based on the Token Issuer
        /// </summary>
        public static string DiscoverEnvironment(Uri issuerUri)
        {
            return DiscoverEnvironmentInternal(issuerUri).Environment;
        }

        #endregion

        #region Private Methods

        private static EnvironmentConfiguration DiscoverEnvironmentInternal(Uri issuerUri)
        {
            if (null == issuerUri)
            {
                return new EnvironmentConfiguration(EnvironmentType.LiveDeviceID, "login.live.com", null);
            }

            var searchList = new Dictionary<EnvironmentType, string>();
            searchList.Add(EnvironmentType.LiveDeviceID, "login.live");
            searchList.Add(EnvironmentType.OrgDeviceID, "login.microsoftonline");

            foreach (var searchPair in searchList)
            {
                if (issuerUri.Host.Length > searchPair.Value.Length &&
                    issuerUri.Host.StartsWith(searchPair.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var environment = issuerUri.Host.Substring(searchPair.Value.Length);

                    //Parse out the environment
                    if ('-' == environment[0])
                    {
                        var separatorIndex = environment.IndexOf('.', 1);
                        if (-1 != separatorIndex)
                        {
                            environment = environment.Substring(1, separatorIndex - 1);
                        }
                        else
                        {
                            environment = null;
                        }
                    }
                    else
                    {
                        environment = null;
                    }

                    return new EnvironmentConfiguration(searchPair.Key, issuerUri.Host, environment);
                }
            }

            //In all other cases the environment is either not applicable or it is a production system
            return new EnvironmentConfiguration(EnvironmentType.LiveDeviceID, issuerUri.Host, null);
        }

        private static void Serialize<T>(Stream stream, T value)
        {
            var serializer = new XmlSerializer(typeof (T), string.Empty);

            var xmlNamespaces = new XmlSerializerNamespaces();
            xmlNamespaces.Add(string.Empty, string.Empty);

            serializer.Serialize(stream, value, xmlNamespaces);
        }

        private static T Deserialize<T>(string operationName, Stream stream)
        {
            //Read the XML into memory so that the data can be used in an exception if necessary
            using (var reader = new StreamReader(stream))
            {
                return Deserialize<T>(operationName, reader.ReadToEnd());
            }
        }

        private static T Deserialize<T>(string operationName, string xml)
        {
            //Attempt to deserialize the data. If deserialization fails, include the XML in the exception that is thrown for further
            //investigation
            using (var reader = new StringReader(xml))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof (T), string.Empty);
                    return (T) serializer.Deserialize(reader);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        "Unable to Deserialize XML (Operation = {0}):{1}{2}",
                        operationName, Environment.NewLine, xml), ex);
                }
            }
        }

        private static FileInfo GetDeviceFile(EnvironmentConfiguration environment)
        {
            return new FileInfo(string.Format(CultureInfo.InvariantCulture, LiveIdConstants.FileNameFormat,
                environment.Type,
                string.IsNullOrEmpty(environment.Environment)
                    ? null
                    : "-" + environment.Environment.ToUpperInvariant()));
        }

        private static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri, LiveDevice device)
        {
            var environment = DiscoverEnvironmentInternal(issuerUri);

            var request = new DeviceRegistrationRequest(applicationId, device);

            var url = string.Format(CultureInfo.InvariantCulture, LiveIdConstants.RegistrationEndpointUriFormat,
                environment.HostName);

            var response = ExecuteRegistrationRequest(url, request);
            if (!response.IsSuccess)
            {
                var throwException = true;
                if (DeviceRegistrationErrorCode.DeviceAlreadyExists == response.Error.RegistrationErrorCode)
                {
                    if (!PersistToFile)
                    {
                        //If the file is not persisted, the registration will always occur (since the credentials are not
                        //persisted to the disk. However, the credentials may already exist. To avoid an exception being continually
                        //processed by the calling user, DeviceAlreadyExists will be ignored if the credentials are not persisted to the disk.
                        return device.User.ToClientCredentials();
                    }
                    else if (PersistIfDeviceAlreadyExists)
                    {
                        // This flag indicates that the 
                        throwException = false;
                    }
                }

                if (throwException)
                {
                    throw new DeviceRegistrationFailedException(response.Error.RegistrationErrorCode,
                        response.ErrorSubCode);
                }
            }

            if (PersistToFile || PersistIfDeviceAlreadyExists)
            {
                WriteDevice(environment, device);
            }

            return device.User.ToClientCredentials();
        }

        private static LiveDevice GenerateDevice(string deviceName, string devicePassword)
        {
            // If the deviceName hasn't been specified, it should be generated using random characters.
            DeviceUserName userNameCredentials;
            if (string.IsNullOrEmpty(deviceName))
            {
                userNameCredentials = GenerateDeviceUserName();
            }
            else
            {
                userNameCredentials = new DeviceUserName {DeviceName = deviceName, DecryptedPassword = devicePassword};
            }

            return new LiveDevice {User = userNameCredentials, Version = 1};
        }

        private static LiveDevice ReadExistingDevice(EnvironmentConfiguration environment)
        {
            //Retrieve the file info
            var file = GetDeviceFile(environment);
            if (!file.Exists)
            {
                return null;
            }

            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Deserialize<LiveDevice>("Loading Device Credentials from Disk", stream);
            }
        }

        private static void WriteDevice(EnvironmentConfiguration environment, LiveDevice device)
        {
            var file = GetDeviceFile(environment);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var stream = file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                Serialize(stream, device);
            }
        }

        private static DeviceRegistrationResponse ExecuteRegistrationRequest(string url,
            DeviceRegistrationRequest
                registrationRequest)
        {
            //Create the request that will submit the request to the server
            var request = WebRequest.Create(url);
            request.ContentType = "application/soap+xml; charset=UTF-8";
            request.Method = "POST";
            request.Timeout = 180000;

            //Write the envelope to the RequestStream
            using (var stream = request.GetRequestStream())
            {
                Serialize(stream, registrationRequest);
            }

            // Read the response into an XmlDocument and return that doc
            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        return Deserialize<DeviceRegistrationResponse>("Deserializing Registration Response", stream);
                    }
                }
            }
            catch (WebException ex)
            {
                Trace.TraceError("Live ID Device Registration Failed (HTTP Code: {0}): {1}",
                    ex.Status, ex.Message);

                if (null != ex.Response)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    {
                        return Deserialize<DeviceRegistrationResponse>("Deserializing Failed Registration Response",
                            stream);
                    }
                }

                throw;
            }
        }

        private static DeviceUserName GenerateDeviceUserName()
        {
            var userName = new DeviceUserName();
            userName.DeviceName = GenerateRandomString(LiveIdConstants.ValidDeviceNameCharacters, MaxDeviceNameLength);
            userName.DecryptedPassword = GenerateRandomString(LiveIdConstants.ValidDevicePasswordCharacters,
                MaxDevicePasswordLength);

            return userName;
        }

        private static string GenerateRandomString(string characterSet, int count)
        {
            //Create an array of the characters that will hold the final list of random characters
            var value = new char[count];

            //Convert the character set to an array that can be randomly accessed
            var set = characterSet.ToCharArray();

            lock (RandomInstance)
            {
                //Populate the array with random characters from the character set
                for (var i = 0; i < count; i++)
                {
                    value[i] = set[RandomInstance.Next(0, set.Length)];
                }
            }

            return new string(value);
        }

        #endregion

        #region Private Classes

        #region Nested type: EnvironmentConfiguration

        private sealed class EnvironmentConfiguration
        {
            public EnvironmentConfiguration(EnvironmentType type, string hostName, string environment)
            {
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    throw new ArgumentNullException("hostName");
                }

                Type = type;
                HostName = hostName;
                Environment = environment;
            }

            #region Properties

            public EnvironmentType Type { get; private set; }

            public string HostName { get; private set; }

            public string Environment { get; private set; }

            #endregion
        }

        #endregion

        #region Nested type: EnvironmentType

        private enum EnvironmentType
        {
            LiveDeviceID,
            OrgDeviceID
        }

        #endregion

        #region Nested type: LiveIdConstants

        private static class LiveIdConstants
        {
            public const string RegistrationEndpointUriFormat = @"https://{0}/ppsecure/DeviceAddCredential.srf";

            public const string ValidDeviceNameCharacters = "0123456789abcdefghijklmnopqrstuvqxyz";

            //Consists of the list of characters specified in the documentation
            public const string ValidDevicePasswordCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^*()-_=+;,./?`~";

            public static readonly string FileNameFormat = Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LiveDeviceID"),
                "{0}{1}.xml");
        }

        #endregion

        #endregion
    }
}