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
using MuggPet.Binding;

namespace MuggTester.Extensibility
{
    public class CommandArgs
    {
        public object Parameter { get; set; }

        public float Progress { get; set; }
    }

    public class BindCommandEx : BindCommand
    {
        public BindCommandEx(int id) : base(id)
        {

        }

        protected override bool IsSupportedView(View view)
        {
            return base.IsSupportedView(view) || view is SeekBar;
        }

        protected override void OnBindView(View view)
        {
            if (view is SeekBar)
                ((SeekBar)view).ProgressChanged += OnProgressChanged;
            else
                base.OnBindView(view);
        }

        protected override void OnUnBindView(View view)
        {
            if (view is SeekBar)
                ((SeekBar)view).ProgressChanged -= OnProgressChanged;
            else
                base.OnUnBindView(view);
        }

        private void OnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            var args = new CommandArgs() { Parameter = Parameter, Progress = e.Progress };
            if (Command.CanExecute(args))
                Command.Execute(args);
        }
    }
}