using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CaliburnMicroMessageNavigator.Mvvm
{
    public abstract class AsyncCommandBase : IAsyncCommand
    {
        public abstract bool CanExecute(object parameter);
        public abstract Task ExecuteAsync(object parameter);

#pragma warning disable VSTHRD100 // Avoid async void methods
        public async void Execute(object parameter)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ExecuteAsync(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}