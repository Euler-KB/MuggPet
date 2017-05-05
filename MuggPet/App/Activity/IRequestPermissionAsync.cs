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
using System.Threading.Tasks;

namespace MuggPet.App.Activity
{
    /// <summary>
    /// Represents an interface that supports requesting of permissions asynchronously
    /// </summary>
    public interface IRequestPermissionAsync
    {
        /// <summary>
        /// Requests permission asynchronously
        /// </summary>
        Task<PermissionGrantResultState> RequestPermissionAsync(string[] permissions);
    }
}