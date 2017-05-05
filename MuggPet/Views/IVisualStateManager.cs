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

namespace MuggPet.Views
{
    /// <summary>
    /// Represents the interface for all hosts that support the use of a visual state manager
    /// </summary>
    public interface IVisualStateManager
    {
        /// <summary>
        /// Gets the visual state manager for this instance
        /// </summary>
        VisualState.VisualStateManager VisualState { get; }
    }
}