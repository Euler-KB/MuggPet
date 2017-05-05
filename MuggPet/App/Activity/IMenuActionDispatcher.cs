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

namespace MuggPet.App.Activity
{
    /// <summary>
    /// Represents the interface for activities that support dispatching selected menu items to methods
    /// </summary>
    public interface IMenuActionDispatcher
    {
        /// <summary>
        /// Dispatches menu item selection event
        /// </summary>
        /// <param name="itemID">The id of the selected menu item</param>
        bool DispatchSelected(int itemID);
    }
}