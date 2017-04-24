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

namespace MuggPet.Commands
{
    /// <summary>
    /// Represents a the interface for defining commands
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Invoked when the command has its execution state changed
        /// </summary>
        event EventHandler CanExecuteChanged;

        /// <summary>
        /// Invoked to determine whether the command can be fired
        /// </summary>
        /// <param name="parameter">An additional input to the command</param>
        /// <returns>True can execute command else false</returns>
        bool CanExecute(object parameter);

        /// <summary>
        /// Executes the logic of the command
        /// </summary>
        /// <param name="parameter">An additional input to the command</param>
        Task Execute(object parameter);
    }
}