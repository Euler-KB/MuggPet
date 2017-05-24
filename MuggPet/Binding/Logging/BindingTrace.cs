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
using Android.Util;

namespace MuggPet.Binding.Logging
{
    internal static class BindingTrace
    {
        const string Tag = "MuggPet.Binding";

        //  Formats the name for a binding operation
        static string FormatModeName(BindingMode mode)
        {
            switch (mode)
            {
                case BindingMode.Attach:
                    return "View Attachment";
                case BindingMode.Command:
                    return "Command Binding";
                case BindingMode.ObjectToView:
                    return "Object-To-View Binding";
                case BindingMode.Resource:
                    return "Resource Binding";
                case BindingMode.ViewContent:
                    return "View-To-Object Binding";
            }

            return "Unknwon Operation";
        }

        //  Traces a failure in binding
        public static void TraceFail(BindingMode mode, string message)
        {
            Log.Warn(Tag, $"Operation: [[ {FormatModeName(mode)} ]] - {message}");
        }

        //  Traces a complete binding operation
        public static void TraceComplete(string message)
        {
            Log.Info(Tag, $"[[ Completed ]] - {message}");
        }

    }
}