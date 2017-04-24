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

namespace MuggTester
{
    [Activity(Label = "Settings", Theme = "@style/AppTheme")]
    public class MuggPreferenceScreen : MuggPet.Preferences.BasePrefrenceActivity
    {
        public MuggPreferenceScreen() : base(Resource.Xml.mugg_preferences)
        {

        }

        public override void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            AppSettings.Manager.UpdateProperty(key, sharedPreferences);
        }
    }
}