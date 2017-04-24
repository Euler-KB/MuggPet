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
    /// Provides vast helpers with respect to numeric calculations
    /// </summary>
    public static class Numerics
    {
        /// <summary>
        /// Generates a sequence of number from @param 'from' to @param 'to' inclusive
        /// </summary>
        public static int[] GenerateRange(int from, int to)
        {
            List<int> items = new List<int>();
            for (; from <= to; from++)
                items.Add(from);

            return items.ToArray();
        }
    }
}