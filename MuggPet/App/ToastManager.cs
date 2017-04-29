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
    /// <summary>
    /// Provides a sequential mechanism for showing toasts
    /// </summary>
    public class ToastManager
    {
        private IDictionary<string, Toast> _toastMap = new Dictionary<string, Toast>();

        private Context context;

        /// <summary>
        /// Initializes a new toast manager with the given context
        /// </summary>
        /// <param name="context">The context object used in displaying the toast</param>
        public ToastManager(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// Shows the specified toast with the associated key in a sequential manner. Thus canceling all active toasts within its group before activation
        /// </summary>
        /// <param name="key">The key that represents the group to which the toast belongs</param>
        /// <param name="toast">The toast to show</param>
        public void ShowToast(string key, Toast toast)
        {
            if (IsAttached(key))
            {
                _toastMap[key].Cancel();
                _toastMap[key] = toast;
            }

            //
            toast.Show();
        }

        /// <summary>
        /// Toasts message with specified key
        /// </summary>
        /// <param name="key">The key that represents the group to which the toast belongs</param>
        /// <param name="message">The message to display</param>
        /// <param name="length">The duration of the toast</param>
        /// <param name="gravity">Gravity flags for adjusting the toast's position on screen. If null, no gravity is applied</param>
        public void Show(string key, string message, ToastLength length = ToastLength.Short, GravityFlags? gravity = null)
        {
            var toast = Toast.MakeText(context, message, length);
            if (gravity != null)
                toast.SetGravity(gravity.Value, 0, 0);

            ShowToast(key, toast);
        }

        /// <summary>
        /// Removes a toast assigned to the given key
        /// </summary>
        /// <param name="key">The key identifying the toast group</param>
        /// <returns>Returns the removed toast</returns>
        public Toast Detach(string key)
        {
            Toast value;
            if (_toastMap.TryGetValue(key, out value))
            {
                _toastMap.Remove(key);
            }

            return value;
        }

        /// <summary>
        /// Resets the keys and all active toasts
        /// </summary>
        public void Reset()
        {
            //
            foreach (var item in _toastMap)
                item.Value.Cancel();

            _toastMap.Clear();
        }

        /// <summary>
        /// Determines whether there is any registered to
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsAttached(string key)
        {
            return _toastMap.ContainsKey(key);
        }
    }
}