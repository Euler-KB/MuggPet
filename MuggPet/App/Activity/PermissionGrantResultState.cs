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
using Android.Content.PM;

namespace MuggPet.App.Activity
{
    /// <summary>
    /// Holds result for a permission request
    /// </summary>
    public class PermissionGrantResultState
    {
        public string [] Permissions { get; set; }

        public Permission[] GrantResults { get; set; }
    }
}