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
using System.Threading;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using System.Runtime.CompilerServices;

namespace MuggPet.Dialogs
{
    /// <summary>
    /// Represents an accept dialog result. Accepted is true if user taps on the positive button and canceled if otherwise.
    /// </summary>
    public class AcceptDialogResult
    {
        private bool? result;

        public AcceptDialogResult(bool? value)
        {
            this.result = value;
        }

        /// <summary>
        /// Indicates accepted state
        /// </summary>
        public bool Accepted
        {
            get { return result == true; }
        }

        /// <summary>
        /// Indicates that the dialog was canceled
        /// </summary>
        public bool Canceled
        {
            get { return result == null; }
        }

        public static implicit operator bool (AcceptDialogResult result)
        {
            return result.Accepted;
        }
    }

    /// <summary>
    /// Facilitates traditional dialogs with async and await pattern
    /// </summary>
    public static class CommonDialogs
    {
        #region Debug

        /// <summary>
        /// Breaks execution if a debugger is attached and traces supplied exception to the output window
        /// </summary>
        /// <param name="context">This parameter is the make the method available within all classes that inherit from context</param>
        /// <param name="ex">The exception to inspect</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void BreakException(this Context context, Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.Message);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //  TODO: Inspect exception object here
                System.Diagnostics.Debugger.Break();
            }

        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void ShowDebugException(this Context context, Exception ex, string title = "An exception occurred!", [CallerMemberName] string methodName = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Method: {methodName}").AppendLine();
            sb.AppendLine(ex.Message);

            //
            Android.Util.Log.Error($"Exception : {methodName}() ", ex.Message);

            //
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //break
                System.Diagnostics.Debugger.Break();
            }

            AlertDialog.Builder dlg = new AlertDialog.Builder(context);
            dlg.SetTitle(title);
            dlg.SetMessage(sb.ToString());
            dlg.SetPositiveButton("Cool", delegate { });
            dlg.Show();
        }

        #endregion

        #region Common Dialogs

        /// <summary>
        /// Show a dialog with a custom specified content view
        /// </summary>
        /// <param name="context">The context object for creating and showing the dialog</param>
        /// <param name="content">The content view of the dialog</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="positiveText">The text message for the positive button. When set to null, the button will be invisible</param>
        /// <param name="negativeText">The text message for the negative button. When set to null, the button will be invisible</param>
        /// <param name="cancellable">Determines whether the dialog can be cancelled</param>
        /// <returns>True and False if positive and negative button is clicked respectively and Null when the dialog is canceled</returns>
        public static async Task<bool?> ShowContentDialog(this Context context, View content, string title = null, string positiveText = null, string negativeText = null, bool cancellable = false)
        {
            bool? dlgResult = null;

            AlertDialog.Builder builder = new AlertDialog.Builder(context);

            if (title != null)
                builder.SetTitle(title);

            if (negativeText != null)
                builder.SetNegativeButton(negativeText, delegate { dlgResult = false; });

            if (positiveText != null)
                builder.SetPositiveButton(positiveText, delegate { dlgResult = true; });

            //  set custom content
            builder.SetView(content);

            await builder.SetTitle(title)
                .SetCancelable(cancellable)
                .Create()
                .ShowAsync();

            return dlgResult;
        }

        /// <summary>
        /// Shows a selection dialog for specified items
        /// </summary>
        /// <param name="context">The context object for creating and showing the dialog</param>
        /// <param name="titleID">The title of the dialog</param>
        /// <param name="itemsID">The items to select from</param>
        /// <param name="cancelTextID">The text for the cancel button. When set to null, will be invisible</param>
        /// <param name="cancelable">Determines whether the dialog is cancelable</param>
        /// <returns>Returns the index of the selected item and null if nothing was selected</returns>
        public static Task<int?> ShowSelectItem(this Context context, int titleID, int itemsID, int? cancelTextID = null, bool cancelable = true)
        {
            return ShowSelectItem(context, context.GetString(titleID), context.Resources.GetStringArray(itemsID), cancelTextID == null ? null : context.GetString(cancelTextID.Value));
        }

        /// <summary>
        /// Shows a selection dialog for specified items
        /// </summary>
        /// <param name="context">The context object for creating and showing the dialog</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="items">The items to select from</param>
        /// <param name="cancelButtonText">The text for the cancel button. When set to null, will be invisible</param>
        /// <param name="cancelable">Determines whether the dialog is cancelable</param>
        /// <returns>Returns the index of the selected item and null if nothing was selected</returns>
        public static async Task<int?> ShowSelectItem(this Context context, string title, string[] items, string cancelButtonText = null, bool cancelable = true)
        {
            int? selectedIndex = null;
            AlertDialog.Builder dlg = new AlertDialog.Builder(context);

            if (cancelButtonText != null)
                dlg.SetNegativeButton(cancelButtonText, delegate { });

            await dlg.SetTitle(title)
                .SetItems(items, (s, e) => { selectedIndex = e.Which; })
                .SetCancelable(cancelable)
                .Create()
                .ShowAsync();

            return selectedIndex;
        }

        /// <summary>
        /// Shows a dialog which accepts a positive or negative input from the user. This is useful for requesting for user choices.
        /// </summary>
        /// <param name="context">The context object for creating and showing the dialog</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="messageID">The resouce id of the message to show</param>
        /// <param name="posButtonResID">The resource id of the text on the positive button. If null, its ignored</param>
        /// <param name="negButtonResID">The resource id of the text on the negative button. If null, its ignored</param>
        /// <param name="cancelable">Determines whether the dialog is cancellable</param>
        /// <returns>A result indicating the users selection</returns>
        public static Task<AcceptDialogResult> ShowAcceptDialog(this Context context, int titleID, int messageID, int? posButtonResID = null, int? negButtonResID = null, bool cancellable = true)
        {
            return ShowAcceptDialog(context, context.GetString(titleID), context.GetString(messageID),
                posButtonResID == null ? null : context.GetString(posButtonResID.Value),
                negButtonResID == null ? null : context.GetString(negButtonResID.Value),
                cancellable);
        }

        /// <summary>
        /// Shows a dialog which accepts a positive or negative input from the user. This is useful for requesting for user choices.
        /// </summary>
        /// <param name="context">The context object for creating and showing the dialog</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message to show</param>
        /// <param name="positiveButtonText">The text on the positive button</param>
        /// <param name="negativeButtonText">The text on the negative button. If null its</param>
        /// <param name="cancelable">Determines whether the dialog is cancellable</param>
        /// <returns>A result indicating the users selection</returns>
        public static async Task<AcceptDialogResult> ShowAcceptDialog(this Context context, string title, string message, string positiveButtonText, string negativeButtonText = "Cancel", bool cancelable = true)
        {
            bool? accept = null;

            AlertDialog.Builder builder = new AlertDialog.Builder(context);

            if (negativeButtonText != null)
                builder.SetNegativeButton(negativeButtonText, delegate { accept = false; });

            if (positiveButtonText != null)
                builder.SetPositiveButton(positiveButtonText, delegate { accept = true; });

            await builder.SetTitle(title)
                .SetCancelable(cancelable)
                .SetMessage(message)
                .Create()
                .ShowAsync();

            return new AcceptDialogResult(accept);
        }

        /// <summary>
        /// Shows an alert dialog with specified title and message
        /// </summary>
        /// <param name="context">The context object</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="positiveText">The text on the positive button. If null, will be invisible</param>
        /// <param name="message">The message to display</param>
        /// <param name="cancelable">Determines whether the dialog is cancellable</param>
        public static Task ShowMessage(this Context context, string title, string message, string positiveText = "OK", bool cancelable = true)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);

            if (positiveText != null)
                builder.SetPositiveButton(positiveText, delegate { });

            return builder.SetTitle(title)
                    .SetMessage(message)
                    .SetCancelable(cancelable)
                    .Create()
                    .ShowAsync();
        }

        #endregion


        /// <summary>
        /// Shows dialog and blocks execution control till dismissed.
        /// This is the core of showing dialogs in an asynchronous fashion.
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        public static Task ShowAsync(this Dialog dialog)
        {
            ManualResetEvent hEvent = new ManualResetEvent(false);

            dialog.DismissEvent += (s, e) =>
            {
                hEvent.Set();
            };

            dialog.Show();

            return Task.Run(() => hEvent.WaitOne());

        }
    }
}