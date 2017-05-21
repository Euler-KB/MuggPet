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
using Android.Graphics;
using MuggPet.App;
using Android.Util;

namespace MuggPet.Utils
{
    /// <summary>
    /// Provides efficient mechanism loading fonts
    /// </summary>
    public static class FontAssetsManager
    {
        static IDictionary<string, Typeface> _store;

        static string _baseDirectory = "fonts";

        static FontAssetsManager()
        {
            _store = new Dictionary<string, Typeface>();
        }

        /// <summary>
        /// Gets or sets the base directory for fonts assets. The default value is 'Fonts'
        /// </summary>
        public static string BaseDirectory
        {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        /// <summary>
        /// Resets the cache
        /// </summary>
        public static void Reset()
        {
            _store.Clear();
        }

        /// <summary>
        /// Retreives a typeface from assets given the base directory
        /// </summary>
        /// <param name="name">The name of the font. The name is appended to the base directory for retrieval</param>
        public static Typeface Get(string name)
        {
            string path = $"{_baseDirectory}/{name}";
            Typeface value;
            if (!_store.TryGetValue(path, out value))
            {
                try
                {
                    value = Typeface.CreateFromAsset(Application.Context.Assets, path);
                    _store[path] = value;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(FontAssetsManager), ex.Message);
                }
            }

            return value;
        }
    }
}