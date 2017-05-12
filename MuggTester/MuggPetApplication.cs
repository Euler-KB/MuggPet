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
using MuggPet.App;

namespace MuggTester
{
    [Application]
    public class MuggPetApplication : BaseApplication
    {
        protected MuggPetApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        protected override void OnLoadComponents()
        {
            //  
            base.OnLoadComponents();

            //  load application settings
            AppSettings.Initialize();
            
        }

    }
}