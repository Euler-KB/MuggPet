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

namespace MuggPet.Activity
{
    /// <summary>
    /// Represent the results from an activity that was started for result
    /// </summary>
    public class ActivityResultState
    {
        /// <summary>
        /// The result code
        /// </summary>
        public Result ResultCode { get; }

        /// <summary>
        /// The associated data
        /// </summary>
        public Intent Data { get; }

        public ActivityResultState(Result resultCode, Intent data)
        {
            ResultCode = resultCode;
            Data = data;
        }
    }

}