using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace MuggPet.Security
{
    /// <summary>
    /// Represents an internal handler for symmetric data protections
    /// </summary>
    public interface IDataProtectorHandler
    {
        /// <summary>
        /// Adjusts input salt
        /// </summary>
        /// <param name="salt">The salt to be adjusted</param>
        /// <returns>The adjusted salt</returns>
        byte[] AdjustSalt(byte[] salt);

        /// <summary>
        /// Converts binary blob to string.
        /// </summary>
        /// <param name="blob">The blob to be converted</param>
        /// <returns>A string representation of the supplied blob</returns>
        string ConvertToString(byte[] blob);

        /// <summary>
        /// Converts a string(payload) to blob
        /// </summary>
        /// <param name="payload">The payload of to be converted to binary form</param>
        /// <returns>The binary representation of the payload</returns>
        byte[] ConvertToBlob(string payload);

        /// <summary>
        /// Protects the given binary data with the given symmetric key
        /// </summary>
        /// <param name="data">The data to protect</param>
        /// <param name="key">The symmetric key for protection</param>
        byte[] Protect(byte[] data, byte[] key);

        /// <summary>
        /// Deciphers the given binary data with the given key
        /// </summary>
        /// <param name="data">The protected data</param>
        /// <param name="key">The symmetric key</param>
        byte[] UnProtect(byte[] data, byte[] key);
    }

    /// <summary>
    /// Protects data using symmetric encryption algorithm
    /// </summary>
    public sealed class DataProtector
    {
        /// <summary>
        /// The default data protection handler
        /// </summary>
        internal sealed class DefaultHandler : IDataProtectorHandler
        {
            public byte[] AdjustSalt(byte[] salt)
            {
                int csp = salt[0] / 2;
                for (int i = 0; i < salt.Length; i++)
                {
                    int v = (salt[i]) + csp;
                    if (v < 50)
                        v = (v * 2) - (v / 3);

                    //
                    salt[i] = (byte)Math.MathHelpers.Clamp(v, 0, byte.MaxValue);
                }

                return salt;
            }

            public byte[] ConvertToBlob(string payload)
            {
                return Convert.FromBase64String(payload);
            }

            public string ConvertToString(byte[] blob)
            {
                return Convert.ToBase64String(blob);
            }

            public byte[] Protect(byte[] data, byte[] key)
            {
                using (var provider = new AesCryptoServiceProvider())
                using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
                {
                    //
                    provider.GenerateIV();
                    provider.Mode = CipherMode.ECB;
                    provider.Padding = PaddingMode.PKCS7;

                    provider.Key = key;
                    bw.Write(provider.IV.Length);
                    bw.Write(provider.IV);

                    using (var encryptor = provider.CreateEncryptor())
                        bw.Write(encryptor.TransformFinalBlock(data, 0, data.Length));

                    ms.Flush();
                    return ms.ToArray();
                }
            }

            public byte[] UnProtect(byte[] data, byte[] key)
            {
                using (var provider = new AesCryptoServiceProvider())
                using (var rd = new BinaryReader(new MemoryStream(data)))
                {
                    provider.Mode = CipherMode.ECB;
                    provider.Padding = PaddingMode.PKCS7;

                    using (var decryptor = provider.CreateDecryptor(key, rd.ReadBytes(rd.ReadInt32())))
                    {
                        byte[] content = rd.ReadBytes((int)(rd.BaseStream.Length - rd.BaseStream.Position));
                        return decryptor.TransformFinalBlock(content, 0, content.Length);
                    }
                }
            }
        }

        //  The final secret key
        private byte[] binarySecretKey;

        //  The proctection handler instance
        private IDataProtectorHandler _protectionHandler;

        /// <summary>
        /// Creates a new instance with specified key and salt
        /// </summary>
        /// <param name="secretKey">The secret key for encrypting data</param>
        /// <param name="salt">Additional salt for hashing secret key</param>
        /// <param name="adjustSalt">Modifies the salt</param>
        /// <param name="handler">Represents an inner </param>
        public DataProtector(byte[] secretKey, byte[] salt, bool adjustSalt = false, IDataProtectorHandler handler = null)
        {
            //  assign protection handler
            _protectionHandler = handler ?? new DefaultHandler();
            Initialize(secretKey, salt, adjustSalt);
        }


        void Initialize(byte[] secretKey, byte[] salt, bool adjustSalt)
        {
            if (adjustSalt)
            {
                //  adjust salt
                salt = _protectionHandler.AdjustSalt(salt);
            }

            using (var hasher = new HMACSHA256(salt))
                binarySecretKey = hasher.ComputeHash(secretKey);
        }

        /// <summary>
        /// Protects the given binary data
        /// </summary>
        /// <param name="dataBytes">The data to undergo protection</param>
        /// <returns>The protected blob</returns>
        public byte[] EncryptRaw(byte[] dataBytes)
        {
            return _protectionHandler.Protect(dataBytes, binarySecretKey);
        }

        /// <summary>
        /// Protects the given payload with a UTF8 encoding
        /// </summary>
        /// <param name="payload">The payload to undergo protection</param>
        /// <returns>The protected payload</returns>
        public string Encrypt(string payload)
        {
            return Encrypt(payload, Encoding.UTF8);
        }

        /// <summary>
        /// Protects the given payload with specified encoding
        /// </summary>
        /// <param name="payload">The payload to undergo protection</param>
        /// <param name="encoding">The encoding for the payload</param>
        /// <returns>The unprotected payload</returns>
        public string Encrypt(string payload, Encoding encoding)
        {
            return _protectionHandler.ConvertToString(EncryptRaw(encoding.GetBytes(payload)));
        }

        /// <summary>
        /// Unprotects the given binary data
        /// </summary>
        /// <param name="dataBytes">The data to be unprotected</param>
        /// <returns>The unprotected blob</returns>
        public byte[] DecryptRaw(byte[] blob)
        {
            return _protectionHandler.UnProtect(blob, binarySecretKey);
        }

        /// <summary>
        /// Unprotects the given payload with a UTF8 encoding
        /// </summary>
        /// <param name="payload">The payload to undergo unprotection</param>
        /// <returns>The protected payload</returns>
        public string Decrypt(string payload)
        {
            return Decrypt(payload, Encoding.UTF8);
        }

        /// <summary>
        /// Unprotects the given payload with specified encoding
        /// </summary>
        /// <param name="payload">The payload to undergo unprotection</param>
        /// <param name="encoding">The encoding for the payload</param>
        /// <returns>The unprotected payload</returns>
        public string Decrypt(string payload, Encoding encoding)
        {
            return encoding.GetString(DecryptRaw(_protectionHandler.ConvertToBlob(payload)));
        }

    }
}