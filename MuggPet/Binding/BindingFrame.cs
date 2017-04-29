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
    /// Represents a frame for an object binding
    /// </summary>
    public class BindingFrame
    {
        /// <summary>
        /// Gets object to view bindings
        /// </summary>
        public IList<BindState> ObjectViewBindings { get; }

        /// <summary>
        /// Gets view attachment bindings
        /// </summary>
        public IList<BindState> ViewAttachment { get; }

        /// <summary>
        /// Gets resource bindings
        /// </summary>
        public IList<BindState> ResourceBindings { get; }

        /// <summary>
        /// Gets command bindings
        /// </summary>
        public IList<BindState> CommandBindings { get; }

        /// <summary>
        /// Gets view to object bindings
        /// </summary>
        public IList<BindState> ViewContentBindings { get; set; }

        public BindingFrame()
        {
            ObjectViewBindings = new List<BindState>();
            ViewAttachment = new List<BindState>();
            ResourceBindings = new List<BindState>();
            CommandBindings = new List<BindState>();
            ViewContentBindings = new List<BindState>();
        }
    }
}