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
using ActionMode = Android.Support.V7.View.ActionMode;
using AppActivity = Android.Support.V7.App.AppCompatActivity;
using MuggPet.App.Activity;

namespace MuggPet.Utils
{
    /// <summary>
    /// Provides a smarter way of managing action mode
    /// </summary>
    public class SmartActionMode : Java.Lang.Object, ActionMode.ICallback
    {
        private bool _isActive;

        /// <summary>
        /// Determines whether action mode is active
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int menuResID = -1;

        private AppActivity hostActivity;

        /// <summary>
        /// Gets the host activity
        /// </summary>
        public AppActivity Host => hostActivity;

        /// The active action mode reference
        private ActionMode actionMode;


        /// <summary>
        /// Invoked when menu item is selected
        /// </summary>
        public event EventHandler<IMenuItem> MenuItemSelected;

        /// <summary>
        /// Invoked when action mode state changes
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Instantiates a new smart action with a host activity and a menu
        /// </summary>
        /// <param name="host">The host activity for starting the action mode</param>
        /// <param name="menu">The resource id of the menu to load for the action mode</param>
        public SmartActionMode(AppActivity host, int menu)
        {
            this.hostActivity = host;
            this.menuResID = menu;
        }

        /// <summary>
        /// Instantiates a new smart action without menu
        /// </summary>
        /// <param name="host">The host activity for starting the action mode</param>
        public SmartActionMode(AppActivity host)
        {
            this.hostActivity = host;
        }

        /// <summary>
        /// Starts to show the action mode if only not already shown
        /// </summary>
        /// <returns>True if shown successfully else otherwise</returns>
        public bool Start()
        {
            if (!IsActive)
            {
                actionMode = hostActivity.StartSupportActionMode(this);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finishes the action mode if already active
        /// </summary>
        /// <returns>True if shown successfully else otherwise</returns>
        public bool Cancel()
        {
            if (IsActive && actionMode != null)
            {
                actionMode.Finish();
                return true;
            }

            return false;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            MenuItemSelected?.Invoke(this, item);

            if (hostActivity is IMenuActionDispatcher)
                return ((IMenuActionDispatcher)hostActivity).DispatchSelected(item.ItemId);

            return true;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            if (menuResID != -1)
            {
                hostActivity.MenuInflater.Inflate(menuResID, menu);
            }

            IsActive = true;
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            IsActive = false;
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }
    }
}