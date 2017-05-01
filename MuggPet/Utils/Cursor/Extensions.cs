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
using Android.Database;
using System.Collections;

namespace MuggPet.Utils.Cursor
{
    public static class CursorExtensions
    {
        /// <summary>
        /// Provides a enumerable instance of the cursor. This can be used with the  'for each' statement making data access tidy and effective
        /// </summary>
        /// <param name="cursor">The cursor to be enumerated</param>
        /// <returns></returns>
        public static IEnumerable AsEnumerable(this ICursor cursor)
        {
            return new CursorIterator(cursor);
        }

        /// <summary>
        /// Iterates each record within the cursor
        /// </summary>
        /// <param name="cursor">The cursor to be iterated</param>
        /// <param name="iterate">A callback for each record</param>
        public static void Iterate(this ICursor cursor, Action<ICursor> iterate)
        {
            if (cursor == null)
                return;

            try
            {
                if (cursor.MoveToFirst())
                {
                    do
                    {
                        iterate.Invoke(cursor);

                    } while (cursor.MoveToNext());
                }
            }
            finally
            {
                cursor.Close();
            }

        }
    }
}