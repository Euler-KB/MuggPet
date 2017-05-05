using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using MuggPet.App.Activity;

namespace MuggPet.Tasks.Native
{
    public class ContactPickTask : TaskBase<Android.Net.Uri>
    {
        public ContactPickTask() { }

        public ContactPickTask(IStartActivityAsync host) : base(host)
        {

        }

        protected override Intent OnGetIntent(object state)
        {
            return new Intent(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
        }

        protected override Task<Android.Net.Uri> OnResult(object state, ActivityResultState result)
        {
            if (result.ResultCode == Result.Ok)
                return Task.FromResult(result.Data.Data);

            return null;
        }
    }
}