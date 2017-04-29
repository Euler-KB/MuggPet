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
    /// Represents an interface that supports binding
    /// </summary>
    public interface ISupportBinding
    {
        /// <summary>
        /// Gets the binding handler for the current object
        /// </summary>
        IBindingHandler BindingHandler { get; }

    }
}