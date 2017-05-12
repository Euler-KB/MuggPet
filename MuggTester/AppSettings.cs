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
using MuggPet.App.Settings;
using MuggPet.App.Settings.Attributes;
using Android.Preferences;

namespace MuggTester
{
    public static class AppSettings
    {
        /// <summary>
        /// This is where you place all your setting properties
        /// </summary>
        public class DefaultSettings : SettingsBase
        {
            bool enableToasts = true;
            bool enableNotifications = false;
            bool autoReplyMessages = true;
            string accountId;
            CarState carState;

            [Protect]
            public CarState CurrentCarState
            {
                get { return carState; }
                set { Set(ref carState, value); }
            }

            public bool EnableToasts
            {
                get { return enableToasts; }
                set { Set(ref enableToasts, value); }
            }

            public bool EnableNotifications
            {
                get { return enableNotifications; }
                set { Set(ref enableNotifications, value); }
            }

            public bool AutoReplyMessages
            {
                get { return autoReplyMessages; }
                set { Set(ref autoReplyMessages, value); }
            }

            public string AccountId
            {
                get { return accountId; }
                set { Set(ref accountId, value); }
            }

        }

        static SettingsManager<DefaultSettings> settingsManager;

        static AppSettings()
        {
            //
            string key = "er3423fdfaDS~@234-Key",  salt = "-~+2X-salt";

            //  create a new settings manager
            settingsManager = new SettingsManager<DefaultSettings>(Encoding.UTF8.GetBytes(key) , Encoding.UTF8.GetBytes(salt));
        }

        public static void Initialize()
        {
            settingsManager.Load();
        }

        /// <summary>
        /// Exposes an instance of application settings
        /// </summary>
        public static DefaultSettings Default
        {
            get { return settingsManager.Default; }
        }

        /// <summary>
        /// Gets the settings manager for the application
        /// </summary>
        public static SettingsManager<DefaultSettings> Manager
        {
            get { return settingsManager; }
        }

    }
}