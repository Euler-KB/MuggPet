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

namespace MuggPet.App
{
    /// <summary>
    /// Represents the base application class
    /// </summary>
    public abstract class BaseApplication : Application, Application.IActivityLifecycleCallbacks, IDispatchLifecycleEvents
    {
        #region Life cycle listener

        public void OnActivityCreated(Android.App.Activity activity, Bundle savedInstanceState)
        {
            ActivityCreated?.Invoke(activity, savedInstanceState);
        }

        public void OnActivityDestroyed(Android.App.Activity activity)
        {
            ActivityDestroyed?.Invoke(activity);
        }

        public void OnActivityPaused(Android.App.Activity activity)
        {
            ActivityPaused?.Invoke(activity);
        }

        public void OnActivityResumed(Android.App.Activity activity)
        {
            //  Assign current activity on resumne
            CurrentActivity = activity;

            ActivityResumed?.Invoke(activity);
        }

        public void OnActivitySaveInstanceState(Android.App.Activity activity, Bundle outState)
        {
            ActivitySaveInstanceState?.Invoke(activity, outState);
        }

        public void OnActivityStarted(Android.App.Activity activity)
        {
            ActivityStarted?.Invoke(activity);
        }

        public void OnActivityStopped(Android.App.Activity activity)
        {
            ActivityStopped?.Invoke(activity);
        }


        void SetupLifecycleCallback()
        {
            //  Ensure we're running only compatible platforms
            if ((int)Build.VERSION.SdkInt >= 14)
            {
                //  register life cycle callback
                RegisterActivityLifecycleCallbacks(this);
            }
        }

        #endregion

        /// <summary>
        /// Gets the current activity
        /// </summary>
        public static Android.App.Activity CurrentActivity { get; private set; }

        /// <summary>
        /// Gets the current application instance
        /// </summary>
        public static BaseApplication Current { get; private set; }

        private IDictionary<string, object> _properties;

        public event ActivityLifecycleDelegate ActivityStarted;

        public event ActivityLifecycleDelegate ActivityStopped;

        public event ActivityLifecycleDelegate ActivityDestroyed;

        public event ActivityLifecycleDelegate ActivityResumed;

        public event ActivityLifecycleDelegate ActivityPaused;

        public event ActivityLifecycleDelegate ActivityRestarted;

        public event ActivityLifecycleDelegate<Bundle> ActivityCreated;

        public event ActivityLifecycleDelegate<Bundle> ActivitySaveInstanceState;

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
            //  assign current activity
            Current = this;

            //  Load components

            //  Initialize application properties 
            _properties = new Dictionary<string, object>();

            //  Listen for activity lifecycle messages
            SetupLifecycleCallback();

            //  TODO: Override and implement custom component initialization

        }


    }
}