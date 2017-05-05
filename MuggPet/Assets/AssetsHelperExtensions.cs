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
using Android.Content.Res;
using System.IO;
using System.Threading.Tasks;

namespace MuggPet.Assets
{
    /// <summary>
    /// Provides various extensions for fetching assets
    /// </summary>
    public static class AssetsHelperExtensions
    {
        /// <summary>
        /// Reads the content of the asset as a string
        /// </summary>
        public static string ReadString(this AssetManager mgr, string name)
        {
            using (var sr = new StreamReader(mgr.Open(name)))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Reads the content of the asset as a string  asynchronously
        /// </summary>
        public static Task<string> ReadStringAsync(this AssetManager mgr,  string name)
        {
            return Task.Run(() => ReadString(mgr, name));
        }

        /// <summary>
        /// Reads the content of an asset as a byte array
        /// </summary>
        public static byte[] Read(this AssetManager mgr, string name)
        {
            using (var ms = new MemoryStream())
            using (var stream = mgr.Open(name))
            {
                stream.CopyTo(ms);
                ms.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Reads the content of an asset as a byte array asynchronously
        /// </summary>
        public static Task<byte[]> ReadAsync(this AssetManager mgr ,string name)
        {
            return Task.Run(() => Read(mgr, name));
        }
    }
}