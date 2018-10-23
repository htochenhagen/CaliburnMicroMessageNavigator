using CaliburnMicroMessageNavigator.ToolWindows;
using CaliburnMicroMessageNavigator.ViewModels;

namespace CaliburnMicroMessageNavigator.Views
{
    public partial class MessageNavigatorToolWindowControl
    {
        public MessageNavigatorToolWindowControl(MessageNavigatorToolWindowState state)
        {
            DataContext = new MessageNavigatorToolWindowControlViewModel(state);
            InitializeComponent();
        }
    }
}