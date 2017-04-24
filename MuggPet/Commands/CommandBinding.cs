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

namespace MuggPet.Commands
{
    internal static class CommandBinding
    {
        /// <summary>
        /// Binds a given command to a view
        /// </summary>
        /// <param name="command">The command to bind</param>
        /// <param name="view">The view to bind the command</param>
        /// <param name="parameter">Additional parameter for the command</param>
        public static void BindCommand(ICommand command, View view, object parameter)
        {
            if (command == null)
                throw new Exception("Command cannot be null upon binding!");

            if (view == null)
                throw new Exception("The view cannot be null upon binding!");


            command.CanExecuteChanged += (s, e) =>
            {
                view.Enabled = command.CanExecute(parameter);
            };

            view.Click += (s, e) =>
            {
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            };

            view.Enabled = command.CanExecute(parameter);
        }


    }
}