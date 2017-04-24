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
    public enum RegisterBehavior
    {
        /// <summary>
        /// Starts a
        /// </summary>
        None = 0,

        /// <summary>
        /// Enables resuming of receiver when activity is restored
        /// </summary>
        EnableResume = 1,

        /// <summary>
        /// Enables pausing of receiver when activity is no longer foreground
        /// </summary>
        PauseOnInactive = 2
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RegisterReceiverAttribute : Attribute
    {
        public RegisterBehavior Behavior { get; }

        public RegisterReceiverAttribute(RegisterBehavior behaviour = RegisterBehavior.EnableResume)
        {
            Behavior = behaviour;
        }
    }
}