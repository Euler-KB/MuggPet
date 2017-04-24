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
        }

        static SettingsManager<DefaultSettings> settingsManager;

        static AppSettings()
        {
            //  create a new settings manager
            settingsManager = new SettingsManager<DefaultSettings>();
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