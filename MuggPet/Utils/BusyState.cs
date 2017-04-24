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
    /// Complements the execution of the actions within scopes defined with the using keyword
    /// </summary>
    public static class BusyState
    {
        class DisposableAction : IDisposable
        {
            Action enter, exit;
            public DisposableAction(Action enter, Action exit)
            {
                this.enter = enter;
                this.exit = exit;

                //
                enter?.Invoke();
            }

            public void Dispose()
            {
                exit?.Invoke();
            }
        }

        /// <summary>
        /// Runs the 'enter' action at the beginning of the scope and always ensures 'exit' action is invoked at the end of the scope when using the 'using' keyword
        /// </summary>
        /// <param name="enter">The action to call at the beginning of the scope</param>
        /// <param name="exit">The must always run action to call at the end of the scope</param>
        public static IDisposable Begin(Action enter, Action exit)
        {
            return new DisposableAction(enter, exit);
        }

    }
}