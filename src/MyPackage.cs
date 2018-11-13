using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using CaliburnMicroMessageNavigator.Commands;
using CaliburnMicroMessageNavigator.Events;
using CaliburnMicroMessageNavigator.ToolWindows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = EnvDTE.Constants;
using Task = System.Threading.Tasks.Task;

namespace CaliburnMicroMessageNavigator
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Caliburn.Micro.MessageNavigator",
        "Displays all publications and handlers of 'Caliburn.Micro' message types treated according to the search performed",
        "1.0", IconResourceID = 400)]
    [ProvideToolWindow(typeof(MessageNavigatorToolWindow), Style = VsDockStyle.Tabbed, DockedWidth = 300,
        Window = Constants.vsWindowKindTaskList, Orientation = ToolWindowOrientation.Left)]
    [Guid("bcbed547-34b3-4e0d-a0cc-740878b4f9fd")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class MyPackage : AsyncPackage, IHandleWithTask<FocusToolWindowEventMessage>
    {
        public MyPackage()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            CmmnEventAggregator.Subscribe(this);
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var path = Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(path);

            if (args.Name.ToLower().Contains("tomstoolbox.core"))
            {
                path = Path.Combine(path ?? throw new InvalidOperationException(), "tomstoolbox.core.dll");
                var ret = Assembly.LoadFrom(path);
                return ret;
            }
            if (args.Name.ToLower().Contains("tomstoolbox.desktop"))
            {
                path = Path.Combine(path ?? throw new InvalidOperationException(), "tomstoolbox.desktop.dll");
                var ret = Assembly.LoadFrom(path);
                return ret;
            }
            if (args.Name.ToLower().Contains("tomstoolbox.observablecollections"))
            {
                path = Path.Combine(path ?? throw new InvalidOperationException(), "tomstoolbox.observablecollections.dll");
                var ret = Assembly.LoadFrom(path);
                return ret;
            }
            if (args.Name.ToLower().Contains("tomstoolbox.wpf"))
            {
                path = Path.Combine(path ?? throw new InvalidOperationException(), "tomstoolbox.wpf.dll");
                var ret = Assembly.LoadFrom(path);
                return ret;
            }

            return null;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ShowToolWindowCommand.InitializeAsync(this);
            await CmmnSearchCommand.InitializeAsync(this);
        }

        public static EventAggregator CmmnEventAggregator = new EventAggregator();

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(Guid.Parse(MessageNavigatorToolWindow.WindowGuidString)) ? this : null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            return toolWindowType == typeof(MessageNavigatorToolWindow)
                ? MessageNavigatorToolWindow.Title
                : base.GetToolWindowTitle(toolWindowType, id);
        }

        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id,
            CancellationToken cancellationToken)
        {
            // Perform as much work as possible in this method which is being run on a background thread.
            // The object returned from this method is passed into the constructor of the SampleToolWindow 
            var dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            return new MessageNavigatorToolWindowState
            {
                DTE = dte
            };
        }

        public async Task Handle(FocusToolWindowEventMessage message)
        {
            var pane = await FindToolWindowAsync(typeof(MessageNavigatorToolWindow), 0, true, DisposalToken);
            if (pane?.Frame is IVsWindowFrame frame)
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                ErrorHandler.ThrowOnFailure(frame.Show());
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

    }
}