using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MuggPet.Activity;
using MuggPet.Activity.VisualState;
using MuggPet.Binding;

namespace MuggPet.App
{
    /// <summary>
    /// Represents the base fragment
    /// </summary>
    public abstract class BaseFragment : Android.Support.V4.App.Fragment , ISupportBinding  , IVisualStateManager
    {
        private int layoutID;

        IBindingHandler bindingHandler;

        /// <summary>
        /// Gets the binding handler instance for this fragment
        /// </summary>
        public IBindingHandler BindingHandler
        {
            get
            {
                return bindingHandler ?? (bindingHandler = new BindingHandler());
            }
        }

        VisualStateManager visualStateManager;

        /// <summary>
        /// Gets the visual state manager for this fragment
        /// </summary>
        public VisualStateManager VisualState
        {
            get
            {
                return visualStateManager ?? (visualStateManager = new VisualStateManager((ViewGroup)View));
            }
        }

        /// <summary>
        /// Initializes a new fragment with specified layout to be loaded
        /// </summary>
        /// <param name="layoutID">The id of the layout for the fragment</param>
        public BaseFragment(int layoutID)
        {
            this.layoutID = layoutID;
        }

        public override async void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            //  Attach views 
            await OnBind();

            //
            OnHandleVisualStates();

            //
            OnLoaded();
        }

        protected virtual void OnHandleVisualStates()
        {
            VisualState.BeginStateDefinition();

            OnDefineVisualStates();

            VisualState.FinalizeStateDefinition();
        }

        protected virtual void OnDefineVisualStates()
        {

        }

        protected virtual Task OnBind()
        {
            //  Attach view to fragment
            this.AttachViews();

            return Task.FromResult(0);
        }

        protected virtual void OnLoaded()
        {
            //  TODO: Overide and implement loaded logic


        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(layoutID, container, false);
        }


    }
}