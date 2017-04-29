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
    /// Represents a binding exception
    /// </summary>
    [Serializable]
    public class BindingException : Exception
    {
        public BindingException() { }

        public BindingException(string message) : base(message) { }
    }

}