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

namespace MuggPet.Dialogs
{
    /// <summary>
    /// Provides various handling dialogs
    /// </summary>
    public static class DialogUtils
    {
        #region 

        /// <summary>
        /// Shows a dialog fragment with specified arguments 
        /// </summary>
        /// <typeparam name="T">The type of the dialog fragment</typeparam>
        /// <param name="fragmentManager">The support fragment manager</param>
        /// <param name="tag">An associated tag for the fragment</param>
        /// <param name="arguments">Represents arguments to be passed to dialog upon creating. If null, no arguments are passed</param>
        public static T Activate<T>(Android.Support.V4.App.FragmentManager fragmentManager, string tag, Bundle arguments = null) where T : Android.Support.V4.App.DialogFragment
        {
            var instance = Activator.CreateInstance<T>();
            if (arguments != null)
                instance.Arguments = arguments;

            instance.Show(fragmentManager, tag);

            return instance;
        }

        #endregion
    }
}