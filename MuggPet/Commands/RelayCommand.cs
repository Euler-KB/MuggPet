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
    /// Defines a command that relays control to delegates for operation. 
    /// </summary>
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Func<object, bool> canExecute;

        private Func<object, Task> execute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            //
            this.canExecute = canExecute;
            this.execute = (arg) =>
            {
                execute(arg);
                return Task.FromResult(0);
            };
        }

        public RelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            //
            this.canExecute = canExecute;
            this.execute = (arg) => execute(arg);
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public Task Execute(object parameter)
        {
            return execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}