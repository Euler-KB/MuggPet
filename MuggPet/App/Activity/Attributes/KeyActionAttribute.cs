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

namespace MuggPet.App.Activity.Attributes
{
    /// <summary>
    /// Invokes a key action on the adorned member
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class KeyActionAttribute : Attribute
    {
        public Keycode Key { get; }

        /// <summary>
        /// Gets or sets the action for triggering the key action
        /// </summary>
        public KeyEventActions Action { get; set; } = KeyEventActions.Down;

        /// <summary>
        /// Initializes a key pressed action invoker on 
        /// </summary>
        /// <param name="key">The action key</param>
        public KeyActionAttribute(Keycode key)
        {
            Key = key;
        }
    }
}