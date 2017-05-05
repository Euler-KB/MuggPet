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
    public delegate void ReceiverReceivedDelegate(BroadcastReceiver receiver, Context context, Intent intent);

    /// <summary>
    /// Implements a broadcast receiver that relays received callbacks to an event
    /// </summary>
    public class ReceiverBase : BroadcastReceiver
    {
        public event ReceiverReceivedDelegate Received;

        public override void OnReceive(Context context, Intent intent)
        {
            Received?.Invoke(this, context, intent);
        }
    }
}