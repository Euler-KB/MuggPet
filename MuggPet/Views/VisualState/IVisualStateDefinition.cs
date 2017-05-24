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

namespace MuggPet.Views.VisualState
{
    public interface IVisualStateDefinition
    {
        /// <summary>
        /// Activates new state for the view
        /// </summary>
        void OnActivateState(View view, VisualState.AnimationState animationState);

        /// <summary>
        /// Reverts the state of the view to initial value
        /// </summary>
        void OnRevertState(View view, object initialState, VisualState.AnimationState animationState);

        /// <summary>
        /// Gets the state for the view
        /// </summary>
        object OnGetState(View view);
    }
}