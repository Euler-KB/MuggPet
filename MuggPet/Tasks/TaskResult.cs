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

namespace MuggPet.Tasks
{
    /// <summary>
    /// Represents a task result
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    public class TaskResult<T>
    {
        bool _success;
        T _result;
        string _errorMessage;

        public bool Succeeded
        {
            get
            {
                return _success;
            }
        }

        public TaskResult(T result)
        {
            _success = result != null;
            _result = result;
        }

        public TaskResult(bool success, T result)
        {
            _success = success;
            _result = result;
        }

        public TaskResult(string errorMessage)
        {
            _success = false;
            _errorMessage = errorMessage;
        }

        public T Result
        {
            get
            {
                return _result;
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
    }
}