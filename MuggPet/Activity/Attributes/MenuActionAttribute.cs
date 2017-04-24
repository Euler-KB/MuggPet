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

namespace MuggPet.Activity.Attributes
{
    /// <summary>
    /// Represents a menu action handler attribute
    /// </summary>
    public interface IMenuActionAttribute
    {
        /// <summary>
        /// The item id of the menu
        /// </summary>
        int ID { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class MenuActionAttribute : Attribute, IMenuActionAttribute
    {
        public int ID { get; private set; }

        public MenuActionAttribute(int menuItemId)
        {
            this.ID = menuItemId;
        }
    }
}