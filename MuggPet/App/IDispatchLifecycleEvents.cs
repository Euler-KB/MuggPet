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

namespace MuggPet.App
{
    #region Delegates

    public delegate void ActivityLifecycleDelegate(Android.App.Activity activity);

    public delegate void ActivityLifecycleDelegate<T>(Android.App.Activity activity, T e);

    #endregion

    /// <summary>
    /// Describes an activity that dispatches its lifecycle events
    /// </summary>
    public interface IDispatchLifecycleEvents
    {
        /// <summary>
        /// Dispatched when activity's OnStart() is called 
        /// </summary>
        event ActivityLifecycleDelegate ActivityStarted;

        /// <summary>
        /// Dispatched when activity's OnStop() is called 
        /// </summary>
        event ActivityLifecycleDelegate ActivityStopped;

        /// <summary>
        /// Dispatched when activity's OnDestroy() is called 
        /// </summary>
        event ActivityLifecycleDelegate ActivityDestroyed;

        /// <summary>
        /// Dispatched when activity's OnResume() is called 
        /// </summary>
        event ActivityLifecycleDelegate ActivityResumed;

        /// <summary>
        /// Dispatched when activity's OnPause() is called 
        /// </summary>
        event ActivityLifecycleDelegate ActivityPaused;

        /// <summary>
        /// Dispatched when activity's OnRestart() is called 
        /// </summary>
        event ActivityLifecycleDelegate ActivityRestarted;

        /// <summary>
        /// Dispatched when activity's OnCreate() is called 
        /// </summary>
        event ActivityLifecycleDelegate<Bundle> ActivityCreated;

        /// <summary>
        /// Dispathed when activity's OnSaveInstanceState is called
        /// </summary>
        event ActivityLifecycleDelegate<Bundle> ActivitySaveInstanceState;
    }
}