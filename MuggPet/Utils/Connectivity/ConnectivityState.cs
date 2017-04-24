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
    /// Manages the network connectivity state
    /// </summary>
    public static class ConnectivityState
    {
        [BroadcastReceiver]
        [IntentFilter(new string[] { ConnectivityManager.ConnectivityAction })]
        internal class ConnectivityReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                Changed?.Invoke(this, ConnectivityManager.FromContext(context));
            }
        }

        static ConnectivityReceiver receiver = new ConnectivityReceiver();

        /// <summary>
        /// Initialize connectivity changes listener when first accessed
        /// </summary>
        static ConnectivityState()
        {
            Application.Context.RegisterReceiver(receiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        /// <summary>
        /// Invoked when connectivity state changes
        /// </summary>
        public static event EventHandler<ConnectivityManager> Changed;

        public static bool IsConnected
        {
            get
            {
                return ConnectivityManager.FromContext(Application.Context).ActiveNetworkInfo?.IsConnected == true;
            }
        }
    }
}