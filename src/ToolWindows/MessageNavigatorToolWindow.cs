using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace CaliburnMicroMessageNavigator.ToolWindows
{
    [Guid(WindowGuidString)]
    public sealed class MessageNavigatorToolWindow : ToolWindowPane
    {
        public const string WindowGuidString = "fb386f22-7169-4f35-8d77-750aa5c7ab6f";
        public const string Title = "Caliburn.Micro.MessageNavigator";

        // "state" parameter is the object returned from MyPackage.InitializeToolWindowAsync
        public MessageNavigatorToolWindow(MessageNavigatorToolWindowState state)
        {
            Caption = Title;
            BitmapResourceID = 400;
            BitmapIndex = 1;

            Content = new MessageNavigatorToolWindowControl(state);
        }
    }
}
