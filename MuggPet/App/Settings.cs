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
using System.Reflection;
using Android.Preferences;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using MuggPet.Security;
using MuggPet.App.Settings.Attributes;

namespace MuggPet.App.Settings
{
    namespace Attributes
    {
        /// <summary>
        /// Marks the specified property as protected. Thus, the value is encrypted and decrypted before exporting and importing respectively
        /// </summary>
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        public class ProtectAttribute : Attribute
        {
            /// <summary>
            /// Determines whether to apply protection to the adorned property
            /// </summary>
            public bool Enabled { get; set; } = true;
        }
    }

    /// <summary>
    /// The base class for setting
    /// </summary>
    public abstract class SettingsBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName]string property = null)
        {
            if (!field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    /// <summary>
    /// Manages application settings to preferences
    /// </summary>
    /// <typeparam name="TSettings">The setting class</typeparam>
    public class SettingsManager<TSettings> where TSettings : INotifyPropertyChanged
    {
        //  Data protector instance
        DataProtector _dataProtector;

        //  Loaded settings yet?
        private bool _isLoaded;

        //  Do we need to save changes
        private bool _settingsDirty;

        //  Determines whether to resist changes to settings update
        private bool _resistChanges;

        //  The settings object instance
        private TSettings instance;

        //  Holds the names of changed properties
        private HashSet<string> _changesMap = new HashSet<string>();

        /// <summary>
        /// The default setting object instance
        /// </summary>
        public TSettings Default
        {
            get { return instance; }
        }

        /// <summary>
        /// Initializes a new settings manager with required keys for data protection
        /// </summary>
        /// <param name="secretkey">The secret key used in protecting values</param>
        /// <param name="salt">The salt to use in conjunction with the secret key</param>
        /// <param name="handler">An optional handler for managing data protection. If null, a default handler will be used</param>
        public SettingsManager(byte[] secretkey, byte[] salt, IDataProtectorHandler handler = null) : this()
        {
            //  initialize data protector
            _dataProtector = new DataProtector(secretkey, salt, true, handler);
        }

        public SettingsManager()
        {
            //  make new setting instance
            instance = Activator.CreateInstance<TSettings>();
            instance.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_resistChanges)
            {
                //  keep track of changed property
                _changesMap.Add(e.PropertyName);

                //  we have some properties changed....yah!
                if (!_settingsDirty)
                    _settingsDirty = true;
            }
        }

        /// <summary>
        /// Updates a setting property that was updated via preference screen or any source without direct interaction with the setting instance 
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <param name="prefMgr">The preference manager to use in persisting the setting value</param>
        /// <returns>True if succeeded else otherwise</returns>
        public bool UpdateProperty(string propertyName, ISharedPreferences prefMgr = null)
        {
            var prop = typeof(TSettings).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                _resistChanges = true;
                if (InternalUpdateProperty(prefMgr ?? PreferenceManager.GetDefaultSharedPreferences(Application.Context), prop, instance))
                {
                    _resistChanges = false;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Loads the settings from preference
        /// </summary>
        public void Load()
        {
            if (!_isLoaded)
            {
                InternalLoadSettings();
                _isLoaded = true;
            }
        }

        /// <summary>
        /// Loads the settings from preference asynchronously
        /// </summary>
        public Task LoadAsync()
        {
            return Task.Run(() => Load());
        }

        /// <summary>
        /// Causes settings to reload
        /// </summary>
        public void Refresh()
        {
            InternalLoadSettings();

            //  clear dirty flags
            if (_settingsDirty)
                _settingsDirty = false;
        }

        /// <summary>
        /// Causes settings to reload asynchronously
        /// </summary>
        public Task RefreshAsync()
        {
            return Task.Run(() => Refresh());
        }

        /// <summary>
        /// Saves settings to preference asynchronously
        /// </summary>
        /// <returns>True if succeeded else otherwise</returns>
        public Task<bool> SaveAsync()
        {
            return Task.Run(() => Save());
        }

        /// <summary>
        /// Saves settings to the preference
        /// </summary>
        /// <returns>True if succeeded else otherwise</returns>
        public bool Save()
        {
            try
            {
                //  check dirty?
                if (!_settingsDirty)
                    return false;

                //
                var prefMgr = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                var editor = prefMgr.Edit();

                //
                foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => _changesMap.Contains(x.Name)))
                    InternalPutValue(editor, prop, prop.GetValue(this));

                //  commit changes
                editor.Commit();

                //  clear dirty
                _settingsDirty = false;
                _changesMap.Clear();

                return true;
            }
            catch
            {
                //  failed to save settings (:-
            }

            return false;
        }

        void InternalLoadSettings()
        {
            var prefMgr = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            foreach (var prop in typeof(TSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                InternalUpdateProperty(prefMgr, prop, instance);
            }
        }

        #region Transfer

        protected virtual bool InternalUpdateProperty(ISharedPreferences prefMgr, PropertyInfo prop, object target)
        {
            //
            bool isProtected = prop.GetCustomAttribute<ProtectAttribute>()?.Enabled == true;
            object value = null;
            if (prop.PropertyType == typeof(bool))
            {
                value = prefMgr.GetBoolean(prop.Name, (bool)prop.GetValue(target));
            }
            else if (prop.PropertyType == typeof(int))
            {
                value = prefMgr.GetInt(prop.Name, (int)prop.GetValue(target));
            }

            else if (prop.PropertyType == typeof(long))
            {
                value = prefMgr.GetLong(prop.Name, (long)prop.GetValue(target));
            }
            else if (prop.PropertyType == typeof(float))
            {
                value = prefMgr.GetFloat(prop.Name, (float)prop.GetValue(target));
            }
            else if (prop.PropertyType == typeof(string))
            {
                value = prop.GetValue(target);
                value = (value == null ? null : (prefMgr.Contains(prop.Name) ? DeserializeValue(prefMgr.GetString(prop.Name, string.Empty), isProtected) : value));
            }
            else if (!prop.PropertyType.IsPrimitive)
            {
                //  we have a custom data type (class)
                value = prop.GetValue(target);
                value = (value == null ? null : (prefMgr.Contains(prop.Name) ? JsonConvert.DeserializeObject(DeserializeValue(prefMgr.GetString(prop.Name, string.Empty), isProtected)) : value));
            }

            if (!Equals(prop.GetValue(target), value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serializes string values and protects it if necessary
        /// </summary>
        /// <param name="value">The string to protect or serialize</param>
        /// <param name="protect">True to protect the value else no protection is ensured</param>
        protected virtual string SerializeValue(string value, bool protect)
        {
            if (protect && _dataProtector != null)
            {
                value = _dataProtector.Encrypt(value);
            }

            return value;
        }

        /// <summary>
        /// Deserializes string values and un protects its if it was protected earlier
        /// </summary>
        /// <param name="value">The string to deserialize or unprotect</param>
        /// <param name="isProtected">Is the string protected</param>
        protected virtual string DeserializeValue(string value, bool isProtected)
        {
            if (isProtected && _dataProtector != null)
            {
                value = _dataProtector.Decrypt(value);
            }

            return value;
        }

        protected virtual void InternalPutValue(ISharedPreferencesEditor editor, PropertyInfo prop, object value)
        {
            bool isProtected = prop.GetCustomAttribute<ProtectAttribute>()?.Enabled == true;
            if (prop.PropertyType == typeof(bool))
                editor.PutBoolean(prop.Name, (bool)value);
            else if (prop.PropertyType == typeof(int))
                editor.PutInt(prop.Name, (int)value);
            else if (prop.PropertyType == typeof(long))
                editor.PutLong(prop.Name, (long)value);
            else if (prop.PropertyType == typeof(float))
                editor.PutFloat(prop.Name, (float)value);
            else if (prop.PropertyType == typeof(string))
                editor.PutString(prop.Name, SerializeValue((string)value, isProtected));
            else
            {
                //  do we have a custom data type ??
                if (!prop.PropertyType.IsPrimitive)
                {
                    editor.PutString(prop.Name, SerializeValue(JsonConvert.SerializeObject(value), isProtected));
                }
            }
        }

        #endregion

    }
}