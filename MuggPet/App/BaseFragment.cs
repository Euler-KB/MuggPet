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

namespace MuggPet.App
{
    /// <summary>
    /// Represents the base fragment for all activity fragments
    /// </summary>
    public abstract class BaseFragment : Android.Support.V4.App.Fragment
    {
        private int layoutID;

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
            OnLoaded();
        }

        protected virtual Task OnBind()
        {
            //  Attach view to fragment
            Binding.BindingManager.AttachViews(this);

            //
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