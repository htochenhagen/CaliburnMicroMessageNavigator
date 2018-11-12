using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace CaliburnMicroMessageNavigator
{
    public class SolutionEventsListener : IVsSolutionEvents, IDisposable
    {
        private IVsSolution _solution;
        private uint _solutionEventsCookie;

        public SolutionEventsListener(IServiceProvider serviceProvider)
        {
            _solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            _solution?.AdviseSolutionEvents(this, out _solutionEventsCookie);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        #region IDisposable Members 

        public void Dispose()
        {
            if (_solution != null && _solutionEventsCookie != 0)
            {
                GC.SuppressFinalize(this);
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                _solution.UnadviseSolutionEvents(_solutionEventsCookie);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
                AfterSolutionLoaded = null;
                BeforeSolutionClosed = null;
                _solutionEventsCookie = 0;
                _solution = null;
            }
        }

        #endregion

        public event EventHandler AfterSolutionLoaded;
        public event EventHandler QueryCloseSolution;
        public event EventHandler BeforeSolutionClosed;
        public event EventHandler AfterCloseSolution;
        public event EventHandler AfterLoadProject;
        public event EventHandler AfterOpenProject;
        public event EventHandler QueryUnloadProject;
        public event EventHandler BeforeUnloadProject;
        public event EventHandler QueryCloseProject;
        public event EventHandler BeforeCloseProject;

        #region IVsSolutionEvents Members 

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            AfterSolutionLoaded?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            QueryCloseSolution?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            BeforeSolutionClosed?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            AfterCloseSolution?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            AfterLoadProject?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            AfterOpenProject?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            QueryUnloadProject?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            BeforeUnloadProject?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            QueryCloseProject?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            BeforeCloseProject?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        #endregion
    }
}