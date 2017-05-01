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
using System.Collections;
using Android.Database;

namespace MuggPet.Utils.Cursor
{
    /// <summary>
    /// Enumerates the content of a cursor
    /// </summary>
    internal class CursorEnumerator : IEnumerator, IEnumerator<ICursor>
    {
        public ICursor cursor;

        public CursorEnumerator(ICursor cursor)
        {
            this.cursor = cursor;
        }

        public object Current
        {
            get
            {
                return cursor;
            }
        }

        ICursor IEnumerator<ICursor>.Current
        {
            get
            {
                return cursor;
            }
        }

        public void Dispose()
        {
            cursor.Close();
        }

        public bool MoveNext()
        {
            return cursor.MoveToNext();
        }

        public void Reset()
        {
            cursor.MoveToFirst();
        }
    }

    /// <summary>
    /// Indicates an enumerable wrapper for a cursor
    /// </summary>
    public class CursorIterator : IEnumerable, IEnumerable<ICursor>
    {
        private CursorEnumerator enumerable;
        public CursorIterator(ICursor cursor)
        {
            enumerable = new CursorEnumerator(cursor);
        }

        public IEnumerator GetEnumerator()
        {
            return enumerable;
        }

        IEnumerator<ICursor> IEnumerable<ICursor>.GetEnumerator()
        {
            return enumerable;
        }

    }
}