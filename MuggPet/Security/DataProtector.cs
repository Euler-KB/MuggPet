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
    /// Represents an internal handler for data protections
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
        public DataProtector(byte[] secretKey, byte[] salt, bool adjustSalt = false, IDataProtectorHandler handler = null)
        {
            //  assign protection handler
            _protectionHandler = handler ?? new DefaultHandler();

            //
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

        public byte[] EncryptRaw(byte[] dataBytes)
        {
            using (var provider = new AesCryptoServiceProvider())
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                //
                provider.GenerateIV();
                provider.Mode = CipherMode.ECB;
                provider.Padding = PaddingMode.PKCS7;

                provider.Key = binarySecretKey;
                bw.Write(provider.IV.Length);
                bw.Write(provider.IV);

                using (var encryptor = provider.CreateEncryptor())
                    bw.Write(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));

                ms.Flush();
                return ms.ToArray();
            }
        }

        public string Encrypt(string raw)
        {
            return Encrypt(raw, Encoding.UTF8);
        }

        public string Encrypt(string raw, Encoding encoding)
        {
            return _protectionHandler.ConvertToString(EncryptRaw(encoding.GetBytes(raw)));
        }

        public byte[] DecryptRaw(byte[] blob)
        {
            using (var provider = new AesCryptoServiceProvider())
            using (var rd = new BinaryReader(new MemoryStream(blob)))
            {
                provider.Mode = CipherMode.ECB;
                provider.Padding = PaddingMode.PKCS7;

                using (var decryptor = provider.CreateDecryptor(binarySecretKey, rd.ReadBytes(rd.ReadInt32())))
                {
                    byte[] content = rd.ReadBytes((int)(rd.BaseStream.Length - rd.BaseStream.Position));
                    return decryptor.TransformFinalBlock(content, 0, content.Length);
                }
            }
        }

        public string Decrypt(string payload)
        {
            return Decrypt(payload, Encoding.UTF8);
        }

        public string Decrypt(string payload, Encoding encoding)
        {
            return encoding.GetString(DecryptRaw(_protectionHandler.ConvertToBlob(payload)));
        }

    }
}