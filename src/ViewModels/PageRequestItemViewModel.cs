using System.Threading.Tasks;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class PageRequestItemViewModel : ItemViewModel
    {
        public TaskCompletionSource<bool> TaskCompletionSource { get; set; }
    }
}