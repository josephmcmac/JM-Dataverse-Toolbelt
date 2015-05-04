#region

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xrm.Sdk.Client;

#endregion

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// located in the SDK\bin folder of the SDK download.

namespace JosephM.Xrm
{
    public class XrmConfiguration : IXrmConfiguration
    {
        #region IXrmConfiguration Members

        public AuthenticationProviderType AuthenticationProviderType { get; set; }
        public string DiscoveryServiceAddress { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        #endregion
    }

    #region Public Classes & Enums

    /// <summary>
    ///     Indicates an error during registration
    /// </summary>
    public enum DeviceRegistrationErrorCode
    {
        /// <summary>
        ///     Unspecified or Unknown Error occurred
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Interface Disabled
        /// </summary>
        InterfaceDisabled = 1,

        /// <summary>
        ///     Invalid Request Format
        /// </summary>
        InvalidRequestFormat = 3,

        /// <summary>
        ///     Unknown Client Version
        /// </summary>
        UnknownClientVersion = 4,

        /// <summary>
        ///     Blank Password
        /// </summary>
        BlankPassword = 6,

        /// <summary>
        ///     Missing Device User Name or Password
        /// </summary>
        MissingDeviceUserNameOrPassword = 7,

        /// <summary>
        ///     Invalid Parameter Syntax
        /// </summary>
        InvalidParameterSyntax = 8,

        /// <summary>
        ///     Invalid Characters are used in the device credentials.
        /// </summary>
        InvalidCharactersInCredentials = 9,

        /// <summary>
        ///     Internal Error
        /// </summary>
        InternalError = 11,

        /// <summary>
        ///     Device Already Exists
        /// </summary>
        DeviceAlreadyExists = 13
    }

    /// <summary>
    ///     Indicates that Device Registration failed
    /// </summary>
    [Serializable]
    public sealed class DeviceRegistrationFailedException : Exception
    {
        /// <summary>
        ///     Construct an instance of the DeviceRegistrationFailedException class
        /// </summary>
        public DeviceRegistrationFailedException()
        {
        }

        /// <summary>
        ///     Construct an instance of the DeviceRegistrationFailedException class
        /// </summary>
        /// <param name="message">Message to pass</param>
        public DeviceRegistrationFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Construct an instance of the DeviceRegistrationFailedException class
        /// </summary>
        /// <param name="message">Message to pass</param>
        /// <param name="innerException">Exception to include</param>
        public DeviceRegistrationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Construct an instance of the DeviceRegistrationFailedException class
        /// </summary>
        /// <param name="code">Error code that occurred</param>
        /// <param name="subCode">Subcode that occurred</param>
        public DeviceRegistrationFailedException(DeviceRegistrationErrorCode code, string subCode)
            : this(code, subCode, null)
        {
        }

        /// <summary>
        ///     Construct an instance of the DeviceRegistrationFailedException class
        /// </summary>
        /// <param name="code">Error code that occurred</param>
        /// <param name="subCode">Subcode that occurred</param>
        /// <param name="innerException">Inner exception</param>
        public DeviceRegistrationFailedException(DeviceRegistrationErrorCode code, string subCode,
            Exception innerException)
            : base(string.Concat(code.ToString(), ": ", subCode), innerException)
        {
            RegistrationErrorCode = code;
        }

        /// <summary>
        ///     Construct an instance of the DeviceRegistrationFailedException class
        /// </summary>
        /// <param name="si"></param>
        /// <param name="sc"></param>
        private DeviceRegistrationFailedException(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }

        #region Properties

        /// <summary>
        ///     Error code that occurred during registration
        /// </summary>
        public DeviceRegistrationErrorCode RegistrationErrorCode { get; private set; }

        #endregion
    }

    #region Serialization Classes

    #region DeviceRegistrationRequest Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("DeviceAddRequest")]
    public sealed class DeviceRegistrationRequest
    {
        #region Constructors

        public DeviceRegistrationRequest()
        {
        }

        public DeviceRegistrationRequest(Guid applicationId, LiveDevice device)
            : this()
        {
            if (null == device)
            {
                throw new ArgumentNullException("device");
            }

            ClientInfo = new DeviceRegistrationClientInfo {ApplicationId = applicationId, Version = "1.0"};
            Authentication = new DeviceRegistrationAuthentication
            {
                MemberName = device.User.DeviceId,
                Password = device.User.DecryptedPassword
            };
        }

        #endregion

        #region Properties

        [XmlElement("ClientInfo")]
        public DeviceRegistrationClientInfo ClientInfo { get; set; }

        [XmlElement("Authentication")]
        public DeviceRegistrationAuthentication Authentication { get; set; }

        #endregion
    }

    #endregion

    #region DeviceRegistrationClientInfo Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("ClientInfo")]
    public sealed class DeviceRegistrationClientInfo
    {
        #region Properties

        [XmlAttribute("name")]
        public Guid ApplicationId { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        #endregion
    }

    #endregion

    #region DeviceRegistrationAuthentication Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("Authentication")]
    public sealed class DeviceRegistrationAuthentication
    {
        #region Properties

        [XmlElement("Membername")]
        public string MemberName { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }

        #endregion
    }

    #endregion

    #region DeviceRegistrationResponse Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("DeviceAddResponse")]
    public sealed class DeviceRegistrationResponse
    {
        #region Properties

        [XmlElement("success")]
        public bool IsSuccess { get; set; }

        [XmlElement("puid")]
        public string Puid { get; set; }

        [XmlElement("Error")]
        public DeviceRegistrationResponseError Error { get; set; }

        [XmlElement("ErrorSubcode")]
        public string ErrorSubCode { get; set; }

        #endregion
    }

    #endregion

    #region DeviceRegistrationResponse Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("Error")]
    public sealed class DeviceRegistrationResponseError
    {
        private string _code;

        #region Properties

        [XmlAttribute("Code")]
        public string Code
        {
            get { return _code; }

            set
            {
                _code = value;

                //Parse the error code
                if (!string.IsNullOrEmpty(value))
                {
                    //Parse the error code
                    if (value.StartsWith("dc", StringComparison.Ordinal))
                    {
                        int code;
                        if (int.TryParse(value.Substring(2), NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out code) &&
                            Enum.IsDefined(typeof (DeviceRegistrationErrorCode), code))
                        {
                            RegistrationErrorCode = (DeviceRegistrationErrorCode) Enum.ToObject(
                                typeof (DeviceRegistrationErrorCode), code);
                        }
                    }
                }
            }
        }

        [XmlIgnore]
        public DeviceRegistrationErrorCode RegistrationErrorCode { get; private set; }

        #endregion
    }

    #endregion

    #region LiveDevice Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("Data")]
    public sealed class LiveDevice
    {
        #region Properties

        [XmlAttribute("version")]
        public int Version { get; set; }

        [XmlElement("User")]
        public DeviceUserName User { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes",
            MessageId = "System.Xml.XmlNode", Justification = "This is required for proper XML Serialization")]
        [XmlElement("Token")]
        public XmlNode Token { get; set; }

        [XmlElement("Expiry")]
        public string Expiry { get; set; }

        [XmlElement("ClockSkew")]
        public string ClockSkew { get; set; }

        #endregion
    }

    #endregion

    #region DeviceUserName Class

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DeviceUserName
    {
        private string _decryptedPassword;
        private string _encryptedPassword;
        private bool _encryptedValueIsUpdated;

        #region Constants

        private const string UserNamePrefix = "11";

        #endregion

        #region Constructors

        public DeviceUserName()
        {
            UserNameType = "Logical";
        }

        #endregion

        #region Properties

        [XmlAttribute("username")]
        public string DeviceName { get; set; }

        [XmlAttribute("type")]
        public string UserNameType { get; set; }

        [XmlElement("Pwd")]
        public string EncryptedPassword
        {
            get
            {
                ThrowIfNoEncryption();

                if (!_encryptedValueIsUpdated)
                {
                    _encryptedPassword = Encrypt(_decryptedPassword);
                    _encryptedValueIsUpdated = true;
                }

                return _encryptedPassword;
            }

            set
            {
                ThrowIfNoEncryption();
                UpdateCredentials(value, null);
            }
        }

        public string DeviceId
        {
            get { return UserNamePrefix + DeviceName; }
        }

        [XmlIgnore]
        public string DecryptedPassword
        {
            get { return _decryptedPassword; }

            set { UpdateCredentials(null, value); }
        }

        private bool IsEncryptionEnabled
        {
            get
            {
                //If the object is not going to be persisted to a file, then the value does not need to be encrypted. This is extra
                //overhead and will not function in partial trust.
                return DeviceIdManager.PersistToFile;
            }
        }

        #endregion

        #region Methods

        public ClientCredentials ToClientCredentials()
        {
            var credentials = new ClientCredentials();
            credentials.UserName.UserName = DeviceId;
            credentials.UserName.Password = DecryptedPassword;

            return credentials;
        }

        private void ThrowIfNoEncryption()
        {
            if (!IsEncryptionEnabled)
            {
                throw new NotSupportedException("Not supported when DeviceIdManager.UseEncryptionApis is false.");
            }
        }

        private void UpdateCredentials(string encryptedValue, string decryptedValue)
        {
            bool isValueUpdated;
            if (string.IsNullOrEmpty(encryptedValue) && string.IsNullOrEmpty(decryptedValue))
            {
                isValueUpdated = true;
            }
            else if (string.IsNullOrEmpty(encryptedValue))
            {
                if (IsEncryptionEnabled)
                {
                    encryptedValue = Encrypt(decryptedValue);
                    isValueUpdated = true;
                }
                else
                {
                    encryptedValue = null;
                    isValueUpdated = false;
                }
            }
            else
            {
                ThrowIfNoEncryption();

                decryptedValue = Decrypt(encryptedValue);
                isValueUpdated = true;
            }

            _encryptedPassword = encryptedValue;
            _decryptedPassword = decryptedValue;
            _encryptedValueIsUpdated = isValueUpdated;
        }

        private string Encrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        private string Decrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var decryptedBytes = ProtectedData.Unprotect(Convert.FromBase64String(value), null,
                DataProtectionScope.CurrentUser);
            if (0 == decryptedBytes.Length)
            {
                return null;
            }

            return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
        }

        #endregion
    }

    #endregion

    #endregion

    #endregion
}