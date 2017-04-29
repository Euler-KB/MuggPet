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
using Android.Net;

namespace MuggPet.Utils.Connectivity
{
    /// <summary>
    /// Manages the network connectivity state.
    /// Note: Usage requires persmissions
    /// </summary>
    public static class ConnectivityState
    {
        /// <summary>
        /// Dispatches changes only when application is foreground
        /// </summary>
        public static bool ForegroundOnly { get; set; }

        [BroadcastReceiver]
        [IntentFilter(new string[] { ConnectivityManager.ConnectivityAction })]
        internal class ConnectivityReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                if (ForegroundOnly && Processes.IsCurrentForeground || !ForegroundOnly)
                {
                    Changed?.Invoke(this, ConnectivityManager.FromContext(context));
                }
            }
        }

        static ConnectivityReceiver receiver = new ConnectivityReceiver();

        /// <summary>
        /// Initialize connectivity changes listener when first accessed
        /// </summary>
        static ConnectivityState()
        {
            if (App.BaseApplication.Current == null)
            {
                throw new Exception("Cannot start connectivity changes manager. Please ensure your application class derives from MuggPet.App.BaseApplication!");
            }

            //  Register connectivity changes listener
            App.BaseApplication.Current.RegisterReceiver(receiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        /// <summary>
        /// Invoked when connectivity state changes
        /// </summary>
        public static event EventHandler<ConnectivityManager> Changed;

        /// <summary>
        /// Determines whether the active network is connected
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                return ConnectivityManager.FromContext(Application.Context).ActiveNetworkInfo?.IsConnected == true;
            }
        }
    }
}