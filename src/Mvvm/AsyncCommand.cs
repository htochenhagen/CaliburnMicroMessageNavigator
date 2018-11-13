using System;
using System.Threading.Tasks;

namespace CaliburnMicroMessageNavigator.Mvvm
{
    public class AsyncCommand : AsyncCommandBase
    {

        public AsyncCommand(Func<Task> executeCallback) : this(null, executeCallback)
        {
        }

        public AsyncCommand(Func<bool> canExecuteCallback, Func<Task> executeCallback)
        {
            ExecuteCallback = executeCallback;
            CanExecuteCallback = canExecuteCallback;
        }

        /// <summary>
        /// Gets or sets the predicate to handle the ICommand.CanExecute method.
        /// If unset, ICommand.CanExecute will always return true if ExecuteCallback is set.
        /// </summary>
        public Func<bool> CanExecuteCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the action to handle the ICommand.Execute method.
        /// If unset, ICommand.CanExecute will always return false.
        /// </summary>
        public Func<Task> ExecuteCallback
        {
            get;
            set;
        }

        public override bool CanExecute(object parameter)
        {
            if (ExecuteCallback == null)
            {
                return false;
            }

            if (CanExecuteCallback == null)
            {
                return true;
            }

            return CanExecuteCallback();
        }

        public override Task ExecuteAsync(object parameter)
        {
            return ExecuteCallback();
        }
    }
}