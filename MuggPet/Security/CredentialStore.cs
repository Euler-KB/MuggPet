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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace MuggPet.Security
{
    /// <summary>
    /// Represents a internal credential store handler. 
    /// </summary>
    public interface ICredentialStoreHandler
    {
        /// <summary>
        /// Generates a new key for identifying credential records.
        /// </summary>
        string GenerateUserKey(UserCredentials credential);

        /// <summary>
        /// Writes the entire binary representation of the store.
        /// </summary>
        void Write(byte[] payload);

        /// <summary>
        /// Reads the entire binary representation of the store.
        /// </summary>
        byte[] Read();

        /// <summary>
        /// Determines whether the handler has created a store location so it can proceed to read.
        /// </summary>
        bool StoreExists();

        /// <summary>
        /// Destroys the store for the credentials
        /// </summary>
        void Destroy();
    }

    namespace Handlers
    {
        /// <summary>
        /// Implements the default file credentials store handler
        /// </summary>
        public class DefaultStoreHandler : ICredentialStoreHandler
        {
            //  The default credential storage file
            static string DefaultFilePath = "{0}/_app_cred_blob.dat";

            //  The path of the file
            private string _actualPath;

            /// <summary>
            /// Initializes a new store handler with default path.
            /// </summary>
            public DefaultStoreHandler()
            {
                _actualPath = string.Format(DefaultFilePath, System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
            }

            /// <summary>
            /// Initializes a new credential handler with a custom credential file store path.
            /// </summary>
            /// <param name="path">The path for credentials storage</param>
            public DefaultStoreHandler(string path)
            {
                _actualPath = path;
            }

            public string GenerateUserKey(UserCredentials credential)
            {
                return $"{string.Concat(Guid.NewGuid().ToString().Take(12))}_{System.Environment.TickCount}";
            }

            public void Write(byte[] blob)
            {
                File.WriteAllBytes(_actualPath, blob);
            }

            public byte[] Read()
            {
                return File.ReadAllBytes(_actualPath);
            }

            public bool StoreExists()
            {
                return File.Exists(_actualPath);
            }

            public void Destroy()
            {
                File.Delete(_actualPath);
            }
        }

    }

    /// <summary>
    /// Implements a secure credentials storage
    /// </summary>
    public static class CredentialStore
    {
        //  The protector instance
        private static DataProtector _dataProtector;

        //  Determines whether the store is initialized
        private static bool _isInitialized;

        //  Holds temporarily credentials fetched from source
        private static List<CredentialInfo> _credentialsInfo;

        //  The credential store handler
        static ICredentialStoreHandler _storeHandler;

        /// <summary>
        /// Describes a UserCredentail with a key attached
        /// </summary>
        class CredentialInfo
        {
            public string Key { get; set; }

            public UserCredentials Credentials { get; set; }
        }

        static async Task<List<CredentialInfo>> InternalGetCredentials()
        {
            if (_credentialsInfo == null)
            {
                if (!_storeHandler.StoreExists())
                {
                    //  create in-memory
                    _credentialsInfo = new List<CredentialInfo>();
                }
                else
                {
                    var rawContent = _storeHandler.Read();
                    var deciphered = _dataProtector.DecryptRaw(rawContent);
                    string payload = Encoding.UTF8.GetString(deciphered);
                    _credentialsInfo = await Task.Run(() => JsonConvert.DeserializeObject<List<CredentialInfo>>(payload));
                }
            }

            return _credentialsInfo;

        }

        static async Task<bool> InternalSaveCredentials()
        {
            if (_credentialsInfo == null)
                throw new Exception("Ensure storage loaded first!");

            try
            {
                //  serialize here
                string payload = JsonConvert.SerializeObject(_credentialsInfo);
                var cipherBlock = _dataProtector.EncryptRaw(Encoding.UTF8.GetBytes(payload));
                await Task.Run(() => _storeHandler.Write(cipherBlock));
                return true;
            }
            catch
            {
                //  failed (:-
            }

            return false;
        }

        /// <summary>
        /// Returns the credentials belonging to the specified key. If not found null is returned
        /// </summary>
        /// <param name="key">The key for the credentials</param>
        public static async Task<UserCredentials> Retrieve(string key)
        {
            CheckInitialized();

            var cStore = await InternalGetCredentials();
            return cStore.Find(x => x.Key.Equals(key))?.Credentials;
        }

        /// <summary>
        /// Clears all stored credentials. 
        /// </summary>
        public static async Task Reset()
        {
            CheckInitialized();

            if (_storeHandler.StoreExists())
            {
                await Task.Run(() => _storeHandler.Destroy());

                if (_credentialsInfo == null)
                    _credentialsInfo = new List<CredentialInfo>();
                else
                    _credentialsInfo.Clear();
            }
        }

        /// <summary>
        /// Destroys the associated credentials with the give key
        /// </summary>
        /// <param name="key">The key for the credentials</param>
        /// <returns>True if operation was successfull else otherwise</returns>
        public static async Task<bool> Destroy(string key)
        {
            CheckInitialized();

            var cStore = await InternalGetCredentials();
            var instance = cStore.Find(x => x.Key.Equals(key));
            if (instance != null)
            {
                bool removed = cStore.Remove(instance);
                if (await InternalSaveCredentials())
                    return removed;
            }

            return false;
        }

        /// <summary>
        /// Stores user credentials and returns its access key
        /// </summary>
        /// <param name="credentials">The user credentials</param>
        /// <returns>The newly generated key for the credentials</returns>
        public static async Task<string> Persist(UserCredentials credentials)
        {
            CheckInitialized();

            //
            string genKey = _storeHandler.GenerateUserKey(credentials);
            var cStore = await InternalGetCredentials();
            cStore.Add(new CredentialInfo()
            {
                Key = genKey,
                Credentials = credentials
            });

            //
            if (await InternalSaveCredentials())
                return genKey;

            //  force collect
            GC.Collect();

            return null;
        }

        /// <summary>
        /// Updates the credential with the specified key
        /// </summary>
        /// <param name="key">The key representing the credentials in the store</param>
        /// <param name="credentials">The credential</param>
        /// <returns>True if succeeded else otherwise</returns>
        public static async Task<bool> Update(string key, UserCredentials credentials)
        {
            CheckInitialized();

            var cInfo = _credentialsInfo.Find(x => x.Key == key);
            if (cInfo != null)
            {
                //  update
                cInfo.Credentials = credentials;

                //  save
                return await InternalSaveCredentials();

            }

            return false;
        }

        static void CheckInitialized()
        {
            if (!_isInitialized)
                throw new Exception("Store not yet initialize. Please ensure to call 'Initialize()' method first before attempting to use any operation on the store!");
        }

        /// <summary>
        /// Initializes the store with required keys
        /// </summary>
        /// <param name="securityKey">The security key for encrypting the data</param>
        /// <param name="salt">The salt used in conjuction with the security key</param>
        /// <param name="storeHandler">The handler for handling protection requests. If null a default handler will be used.</param>
        public static bool Initialize(byte[] securityKey, byte[] salt, ICredentialStoreHandler storeHandler)
        {
            if (!_isInitialized)
            {
                try
                {
                    //  initialize a new data protector
                    _dataProtector = new DataProtector(securityKey, salt, true);

                    //  assign the current store handler
                    _storeHandler = storeHandler ?? new Handlers.DefaultStoreHandler();
                }
                catch
                {
                    return false;
                }

                _isInitialized = true;
            }

            return true;
        }

        /// <summary>
        /// Initializes the store with required keys
        /// </summary>
        /// <param name="securityKey">The security key for encrypting the data</param>
        /// <param name="salt">The salt used in conjuction with the security key</param>
        /// <param name="storagePath">The path for credentials storage</param>
        public static bool Initialize(byte[] securityKey, byte[] salt, string storagePath)
        {
            if (!_isInitialized)
            {
                try
                {
                    //  initialize a new data protector
                    _dataProtector = new DataProtector(securityKey, salt, true);

                    //  assign the current store handler
                    _storeHandler = new Handlers.DefaultStoreHandler(storagePath);
                }
                catch
                {
                    return false;
                }

                _isInitialized = true;
            }

            return true;
        }

    }
}