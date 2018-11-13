using System.Threading.Tasks;
using System.Windows.Input;

namespace CaliburnMicroMessageNavigator.Mvvm
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}