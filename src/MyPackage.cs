using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CaliburnMicroMessageNavigator.Commands;
using CaliburnMicroMessageNavigator.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CaliburnMicroMessageNavigator
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Caliburn.Micro.MessageNavigator", "Displays all publications and handlers of 'Caliburn.Micro' message types treated according to the search performed", "1.0", IconResourceID = 400)]
    [ProvideToolWindow(typeof(MessageNavigatorToolWindow), Style = VsDockStyle.Linked, DockedWidth = 300, Window = "DocumentWell", Orientation = ToolWindowOrientation.Bottom)]
    [Guid("7a437e08-57e9-4a52-a069-6299e23e6e7e")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class MyPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            await ShowToolWindow.InitializeAsync(this);
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(Guid.Parse(MessageNavigatorToolWindow.WindowGuidString)) ? this : null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            return toolWindowType == typeof(MessageNavigatorToolWindow) ? MessageNavigatorToolWindow.Title : base.GetToolWindowTitle(toolWindowType, id);
        }

        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            // Perform as much work as possible in this method which is being run on a background thread.
            // The object returned from this method is passed into the constructor of the SampleToolWindow 
            var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            return new MessageNavigatorToolWindowState
            {
                DTE = dte
            };
        }
    }
}
