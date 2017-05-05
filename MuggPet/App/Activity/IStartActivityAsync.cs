using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;

namespace MuggPet.App.Activity
{
    /// <summary>
    /// Represents the interface for an activity that can start other activities asynchronously
    /// </summary>
    public interface IStartActivityAsync
    {
        /// <summary>
        /// Starts an activity for result asynchronously
        /// </summary>
        /// <param name="activityType">The type of the activity to start</param>
        Task<ActivityResultState> StartActivityForResultAsync(Type activityType);

        /// <summary>
        /// Starts an activity for result asynchronously
        /// </summary>
        /// <param name="intent">The intent to start the activity</param>
        Task<ActivityResultState> StartActivityForResultAsync(Intent intent);

        /// <summary>
        /// Starts an activit for result asynchronously
        /// </summary>
        /// <param name="intent">The intent to start the activity</param>
        /// <param name="options">Extra options for the intent</param>
        Task<ActivityResultState> StartActivityForResultAsync(Intent intent, Bundle options);

    }
}