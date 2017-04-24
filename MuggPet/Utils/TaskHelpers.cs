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
using System.Threading.Tasks;

namespace MuggPet.Utils
{
    /// <summary>
    /// Provides delayed executing of actions on both background and main thread
    /// </summary>
    public static class Tasks
    {
        /// <summary>
        /// Executes the action after specified delay on the background thread
        /// </summary>
        /// <param name="delay">The time to delay (in millisecs)</param>
        /// <param name="execute">The action to excute</param>
        public static Task Execute(int delay, Action execute)
        {
            return Task.Delay(delay).ContinueWith(t =>
            {
                execute();
            });
        }

        /// <summary>
        /// Executes the specified action of the ui thread after the given delay
        /// </summary>
        /// <param name="activity">The activity in context</param>
        /// <param name="delay">The time(in millisecs) to delay</param>
        /// <param name="execute">The action to execute</param>
        public static Task ExecuteDelayedAsync(this Android.App.Activity activity, int delay, Action execute)
        {
            return Execute(delay, () =>
            {
                activity.RunOnUiThread(execute);
            });
        }

        /// <summary>
        /// Executes the specified action of the ui thread after the given delay
        /// </summary>
        /// <param name="activity">The activity in context</param>
        /// <param name="delay">The time(in millisecs) to delay</param>
        /// <param name="execute">The action to execute</param>
        public static async void ExecuteDelayed(this Android.App.Activity activity, int delay, Action execute)
        {
            await ExecuteDelayedAsync(activity, delay, execute);
        }

    }
}