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

[assembly: MuggPet.App.Lifecycle(typeof(MuggPet.App.BaseApplication), "OnResumeActivity", MuggPet.App.LifecycleScope.ActivityResumed)]
namespace MuggPet.App
{
    /// <summary>
    /// Represents the base application class
    /// </summary>
    public abstract class BaseApplication : Application
    {
        #region Life cycle listener

        /// <summary>
        /// Invoked when an activity is resumed
        /// </summary>
        /// <param name="activity">The resumed activity</param>
        static void OnResumeActivity(Android.App.Activity activity)
        {
            CurrentActivity = activity;
        }

        #endregion

        /// <summary>
        /// Gets the current activity
        /// </summary>
        public static Android.App.Activity CurrentActivity { get; private set; }


        private IDictionary<string, object> _properties;

        /// <summary>
        /// Exposes an in-memory key-value pair of properties for storing temporary data
        /// </summary>
        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Retreives a property with specified key
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the property</param>
        public T GetProperty<T>(string key)
        {
            if (_properties.ContainsKey(key))
                return (T)_properties[key];

            return default(T);
        }

        protected BaseApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            //  Load components
            OnLoadComponents();
        }

        /// <summary>
        /// Loads application dependent components immediately after application is created.
        /// </summary>
        protected virtual void OnLoadComponents()
        {
            //  Load all application components here...

            //  Initialize application properties 
            _properties = new Dictionary<string, object>();

            //  Initialize life cycle manager
            LifeCycleManager.Initialize(typeof(BaseApplication).Assembly, GetType().Assembly);

            //  TODO: Override and implement custom component initialization

        }
    }
}