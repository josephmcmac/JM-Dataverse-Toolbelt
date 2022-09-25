﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace JosephM.Core.Security
{
    public class StringEncryptor
    {
        private static readonly byte[] entropy = Encoding.Unicode.GetBytes("MAC");

        public static string Encrypt(string rawString)
        {
            return EncryptStringFromSecure(ToSecureString(rawString));
        }

        public static string Decrypt(string encryptedString)
        {
            return ToInsecureString(DecryptStringToSecure(encryptedString));
        }

        private static string EncryptStringFromSecure(SecureString input)
        {
            var encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        private static SecureString DecryptStringToSecure(string encryptedData)
        {
            try
            {
                var decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    DataProtectionScope.CurrentUser);
                return ToSecureString(Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        private static SecureString ToSecureString(string input)
        {
            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        private static string ToInsecureString(SecureString input)
        {
            var returnValue = string.Empty;
            var ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        public static string HashString(string stringToHashIrreversibly)
        {
            var crypter = new SHA1CryptoServiceProvider();
            var textWithSaltBytes = Encoding.UTF8.GetBytes(string.Concat(stringToHashIrreversibly, "MAC"));
            var hashedBytes = crypter.ComputeHash(textWithSaltBytes);
            return Convert.ToBase64String(hashedBytes);
        }
    }
}