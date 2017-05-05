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

namespace MuggPet.Binding
{
    /// <summary>
    /// Represents an execption generated upon binding
    /// </summary>
    [Serializable]
    public class BindingException : Exception
    {
        /// <summary>
        /// Initializes a new empty binding exception
        /// </summary>
        public BindingException() { }

        /// <summary>
        /// Initializes a new binding execption with a message for the exception
        /// </summary>
        /// <param name="message">The error message for the exception</param>
        public BindingException(string message) : base(message) { }
    }

}