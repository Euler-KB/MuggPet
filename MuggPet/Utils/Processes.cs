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

namespace MuggPet.Utils
{
    /// <summary>
    /// Provides various methods for accessing your application process
    /// </summary>
    public static class Processes
    {
        /// <summary>
        /// Returns the name of the current process
        /// </summary>
        public static string CurrentProcessName
        {
            get { return Application.Context.PackageName; }
        }

        /// <summary>
        /// Determines whether the current process is running
        /// Note: This is useful in services which requires the attached processes running state
        /// </summary>
        public static bool IsCurrentRunning
        {
            get
            {
                var activityManager = ActivityManager.FromContext(Application.Context);
                var processes = activityManager.RunningAppProcesses;
                if (processes == null)
                    return false;

                string process = CurrentProcessName;
                return processes.Any(x => x.ProcessName.Equals(process));
            }
        }

        /// <summary>
        /// Determines whether the current process if foreground
        /// Note: This is useful in services which requires the attached processes to be foreground
        /// </summary>
        public static bool IsCurrentForeground
        {
            get
            {
                var activityManager = ActivityManager.FromContext(Application.Context);
                var processes = activityManager.RunningAppProcesses;
                if (processes == null)
                    return false;

                string process = CurrentProcessName;
                foreach (var app in processes)
                {
                    if (app.Importance == Importance.Foreground && app.ProcessName.Equals(process) )
                        return true;
                }

                return false;
            }
        }
    }

}