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

namespace MuggPet.Utils
{
    /// <summary>
    /// Provides various file utilities
    /// </summary>
    public static class FileUtils
    {
        public static string Prefix;

        public static string PostFix = ".temp";

        static FileUtils()
        {
            Prefix = Application.Context.PackageName;
        }

        /// <summary>
        /// Gets a temp file within internal storage
        /// </summary>
        public static Java.IO.File InternalTempFile
        {
            get
            {
                return Java.IO.File.CreateTempFile(Prefix, PostFix, Application.Context.CacheDir);
            }
        }

        /// <summary>
        /// Gets a temp file within external storage
        /// </summary>
        public static Java.IO.File ExtermalTempFile
        {
            get
            {
                return Java.IO.File.CreateTempFile(Prefix, PostFix, Application.Context.ExternalCacheDir);
            }
        }

    }
}