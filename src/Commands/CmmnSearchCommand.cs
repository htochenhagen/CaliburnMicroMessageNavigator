﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Caliburn.Micro;
using CaliburnMicroMessageNavigator.Events;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace CaliburnMicroMessageNavigator.Commands
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class CmmnSearchCommand
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f76f9f33-e602-4869-b9af-97c77e8e4a6a");

        /// <summary>
        ///     VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CmmnSearchCommand" /> class.
        ///     Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CmmnSearchCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static CmmnSearchCommand Instance { get; private set; }

        /// <summary>
        ///     Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => _package;

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CMMNSearchCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CmmnSearchCommand(package, commandService);
        }

        /// <summary>
        ///     This function is the callback used to execute the command when the menu item is clicked.
        ///     See the constructor to see how the menu item is associated with this function using
        ///     OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (ServiceProvider is IServiceProvider serviceProvider)
            {
                var text = GetTextFromSource(serviceProvider);

                MyPackage.CmmnEventAggregator.PublishOnCurrentThread(new SourceCodeTextEventMessage(text));
            }
        }

        private string GetTextFromSource(IServiceProvider serviceProvider1)
        {
            var result = string.Empty;
            var service = serviceProvider1.GetService(typeof(SVsTextManager));
            if (service is IVsTextManager2 textManager)
            {
                textManager.GetActiveView2(1, null, (uint) _VIEWFRAMETYPE.vftCodeWindow, out var view);

                var textSpanArray = new[] {new TextSpan()};
                // Get current source code selection
                var selectionSpanResult = view.GetSelectionSpan(textSpanArray);
                var textSpan = textSpanArray[0];
                Trace.WriteLine(
                    $"{nameof(CmmnSearchCommand)}.{nameof(Execute)}():\t\tSelection start line = {textSpan.iStartLine}\t\tstart column = {textSpan.iStartIndex}\t\tend line = {textSpan.iEndLine}\t\tend column = {textSpan.iEndIndex}");

                if (selectionSpanResult != 0 && textSpan.iStartLine == textSpan.iEndLine &&
                    textSpan.iStartIndex == textSpan.iEndIndex)
                {
                    // Determine word under the cursor
                    textSpanArray = new[] {new TextSpan()};
                    view.GetWordExtent(textSpan.iStartLine, textSpan.iStartIndex, 0, textSpanArray);
                    textSpan = textSpanArray[0];
                }

                Trace.WriteLine(
                    $"{nameof(CmmnSearchCommand)}.{nameof(Execute)}():\t\tFinal text span start line = {textSpan.iStartLine}\t\tstart column = {textSpan.iStartIndex}\t\tend line = {textSpan.iEndLine}\t\tend column = {textSpan.iEndIndex}");

                // Determine text of selection or word
                view.GetTextStream(textSpan.iStartLine, textSpan.iStartIndex, textSpan.iEndLine, textSpan.iEndIndex,
                    out result);
                Trace.WriteLine($"{nameof(CmmnSearchCommand)}.{nameof(Execute)}():\t\tWord under cursor = {result}");
            }

            return result;
        }
    }
}