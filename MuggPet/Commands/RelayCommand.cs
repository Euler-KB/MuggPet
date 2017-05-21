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
    /// Represents a relay command handler
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Dispatched when command's executable state has changed
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Called to determine whether the command can be executed
        /// </summary>
        private Func<object, bool> canExecute;

        /// <summary>
        /// The execute method
        /// </summary>
        private Action<object> execute;

        /// <summary>
        /// Initializes a new relay command with an execute callback which runs on the caller thread and an optional callback for determining command's execution state
        /// </summary>
        /// <param name="execute">This callback is invoked to execute the command</param>
        /// <param name="canExecute">Indicates whether the command can be executed</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            //
            this.canExecute = canExecute;
            this.execute = execute;
        }

        /// <summary>
        /// Determines whether the command is executable
        /// </summary>
        /// <param name="parameter">An optional state argument</param>
        /// <returns>True if executable else otherwise</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">An optional parameter for the command</param>
        public Task Execute(object parameter)
        {
            execute(parameter);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Notifies command's executable state has changed
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}