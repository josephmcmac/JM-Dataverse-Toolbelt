using System;
using JosephM.Core.Security;

namespace JosephM.Core.FieldType
{
    [Serializable]
    public class Password
    {
        protected bool Equals(Password other)
        {
            return string.Equals(_password, other._password) && EncryptPassword.Equals(other.EncryptPassword);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_password != null ? _password.GetHashCode() : 0)*397) ^ EncryptPassword.GetHashCode();
            }
        }

        public static bool operator ==(Password left, Password right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Password left, Password right)
        {
            return !Equals(left, right);
        }

        private readonly string _password;
        public bool EncryptPassword { get; set; }

        public Password(string encryptedPassword)
            : this(encryptedPassword, true, true)
        {
        }

        public Password(string password, bool isEncrypted, bool encrypt)
        {
            EncryptPassword = encrypt;
            _password = EncryptPassword && !isEncrypted ? StringEncryptor.Encrypt(password) : password;
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Password;
            return other != null && Equals(other);
        }

        public static Password CreateFromRawPassword(string rawPassword)
        {
            return new Password(StringEncryptor.Encrypt(rawPassword));
        }

        /// <summary>
        ///     Note this is used when saving to the encypted password to config
        /// </summary>
        public override string ToString()
        {
            return _password;
        }

        public string GetRawPassword()
        {
            return EncryptPassword ? StringEncryptor.Decrypt(_password) : _password;
        }
    }
}