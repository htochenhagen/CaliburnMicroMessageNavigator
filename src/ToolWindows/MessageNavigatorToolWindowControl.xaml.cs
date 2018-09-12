using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace CaliburnMicroMessageNavigator.ToolWindows
{
    public partial class MessageNavigatorToolWindowControl
    {
        public MessageNavigatorToolWindowState State { get; }
        private CancellationTokenSource _cancellationTokenSource;

        public MessageNavigatorToolWindowControl(MessageNavigatorToolWindowState state)
        {
            State = state;
            InitializeComponent();

            StatusLabel.Content = "Ready";
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteSearchAsync();
        }
        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task ExecuteSearchAsync()
        {
            try
            {
                SearchButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Visible;
                ListViewHandlersResults.Items.Clear();

                var cancellationToken = ResetCancellationToken();
                
                int publicationsCount = await ExecutePublicationsSearchAsync(cancellationToken);
                int handlersCount = await ExecuteHandlersSearchAsync(cancellationToken);

                SetStatusMessage($"{publicationsCount} publications and {handlersCount} handlers found.");
            }
            finally
            {
                SearchButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<int> ExecutePublicationsSearchAsync(CancellationToken cancellationToken)
        {
            //var searchExpression = SearchTextBox.Text;
            var searchExpression =
                $"AllNodes.OfType<InvocationExpressionSyntax>().Where(ie => System.Text.RegularExpressions.Regex.Match(ie.Expression.ToString(), \"publish.*(thread)?\", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success && System.Text.RegularExpressions.Regex.Match(ie.ArgumentList.Arguments.First().Expression.ToString(), \"{SearchTextBox.Text}\", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success)";
            try
            {
                var scriptResult = await ScriptRunner.RunScriptAsync(searchExpression, new ScriptGlobals(cancellationToken), cancellationToken);

                return await DisplayPublicationsResultAsync(scriptResult, cancellationToken);
            }
            catch (CompilationErrorException cex)
            {
                DisplayDiagnosticsForPublications(cex.Diagnostics);
            }
            catch (Exception ex)
            {
                AddToListViewPublications(ex);
            }

            return 0;
        }

        private async Task<int> ExecuteHandlersSearchAsync(CancellationToken cancellationToken)
        {
            //var searchExpression = SearchTextBox.Text;
            var searchExpression =
                $"AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == \"Handle\" && System.Text.RegularExpressions.Regex.Match(md.ParameterList.Parameters.Last().Type.ToString(), \"{SearchTextBox.Text}\", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success)";
            try
            {
                var scriptResult = await ScriptRunner.RunScriptAsync(searchExpression, new ScriptGlobals(cancellationToken), cancellationToken);

                return await DisplayHandlersResultAsync(scriptResult, cancellationToken);
            }
            catch (CompilationErrorException cex)
            {
                DisplayDiagnosticsForHandlers(cex.Diagnostics);
            }
            catch (Exception ex)
            {
                AddToListViewHandlers(ex);
            }

            return 0;
        }

        private async Task<int> DisplayPublicationsResultAsync(object scriptResult, CancellationToken cancellationToken)
        {
            ListViewPublicationsResults.Items.Clear();

            return await IterateItems(scriptResult, cancellationToken, AddToListViewPublications, WaitForNextPublicationsPageRequestAsync);
        }

        private async Task<int> DisplayHandlersResultAsync(object scriptResult, CancellationToken cancellationToken)
        {
            ListViewHandlersResults.Items.Clear();

            return await IterateItems(scriptResult, cancellationToken, AddToListViewHandlers, WaitForNextHandlersPageRequestAsync);
        }

        private Task WaitForNextPublicationsPageRequestAsync(int pageLimit)
        {
            var moreItemsTaskCompletionSource = new TaskCompletionSource<bool>();
            var moreItemsListViewItem = new ListViewItem()
            {
                Content = $"More than {pageLimit } publications found, click to fetch more"
            };
            moreItemsListViewItem.MouseUp += (sender, args) =>
            {
                ListViewPublicationsResults.Items.Remove(moreItemsListViewItem);
                moreItemsTaskCompletionSource.SetResult(true);
            };
            ListViewPublicationsResults.Items.Add(moreItemsListViewItem);

            // Return the Task that will complete when the ListViewItem is clicked
            return moreItemsTaskCompletionSource.Task;
        }

        private Task WaitForNextHandlersPageRequestAsync(int pageLimit)
        {
            var moreItemsTaskCompletionSource = new TaskCompletionSource<bool>();
            var moreItemsListViewItem = new ListViewItem()
            {
                Content = $"More than {pageLimit } handlers found, click to fetch more"
            };
            moreItemsListViewItem.MouseUp += (sender, args) =>
            {
                ListViewHandlersResults.Items.Remove(moreItemsListViewItem);
                moreItemsTaskCompletionSource.SetResult(true);
            };
            ListViewHandlersResults.Items.Add(moreItemsListViewItem);

            // Return the Task that will complete when the ListViewItem is clicked
            return moreItemsTaskCompletionSource.Task;
        }

        private void AddToListViewHandlers(object item)
        {
            var foundHandleResult = CreateFoundResult(item);
            ListViewHandlersResults.Items.Add(foundHandleResult);
        }

        private void AddToListViewPublications(object item)
        {
            var foundHandleResult = CreateFoundResult(item);
            ListViewPublicationsResults.Items.Add(foundHandleResult);
        }

        private void DisplayDiagnosticsForHandlers(IEnumerable<Diagnostic> diagnostics)
        {
            ListViewHandlersResults.Items.Clear();
            foreach (var diagnostic in diagnostics)
            {
                var item = new DiagnosticListItem(diagnostic, SearchTextBox);
                ListViewHandlersResults.Items.Add(item);
            }
        }

        private void DisplayDiagnosticsForPublications(IEnumerable<Diagnostic> diagnostics)
        {
            ListViewPublicationsResults.Items.Clear();
            foreach (var diagnostic in diagnostics)
            {
                var item = new DiagnosticListItem(diagnostic, SearchTextBox);
                ListViewPublicationsResults.Items.Add(item);
            }
        }

        /// <summary>
        /// Cancells any pending operation and creates a new CancellationToken for a new Cancellable operation
        /// </summary>
        private CancellationToken ResetCancellationToken()
        {
            // If there was a previous action we cancel it
            _cancellationTokenSource?.Cancel();

            // Then we create a new CancallationTokenSource
            var newSource = new CancellationTokenSource();
            _cancellationTokenSource = newSource;
            return newSource.Token;
        }

        private async Task<int> IterateItems(object scriptResult, CancellationToken cancellationToken, Action<object> itemAction, Func<int, Task> waitForNextsPageRequestAction)
        {
            var count = 0;
            var pageLimit = 1000;

            var items = MakeEnumerable(scriptResult);
            // Use an ThreadPoolAsyncEnumerator because the MoveNext calls into the Enumerator returned by the script
            // and we do not want to run the script code on the UI thread
            using (var enumerator = new ThreadPoolAsyncEnumerator<object>(items))
            {
                var moveNextTask = enumerator.MoveNextAsync();
                while (await moveNextTask)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    count++;
                    if (count > pageLimit)
                    {
                        // We reached the PageLimit, stop enumerating until the user requests more items
                        await waitForNextsPageRequestAction(pageLimit);
                        pageLimit += 1000;
                    }

                    var item = enumerator.Current;
                    // Start getting the next item while we process the current
                    moveNextTask = enumerator.MoveNextAsync();

                    itemAction(item);
                }
            }

            return count;
        }

        private void SetStatusMessage(string msg)
        {
            StatusLabel.Content = msg;
        }

        private static ListViewItem CreateFoundResult(object item)
        {
            var syntaxNodeOrToken = RoslynHelpers.AsSyntaxNodeOrToken(item);
            if (syntaxNodeOrToken != null)
            {
                return new SyntaxNodeOrTokenListItem(syntaxNodeOrToken.Value);
            }

            return new ListViewItem() {Content = item?.ToString() ?? "<null>"};
        }

        private static IEnumerable<object> MakeEnumerable(object input)
        {
            if (input is IEnumerable enumerable && !(input is string))
            {
                // If the item is Enumerable we return it as is
                return enumerable.Cast<object>();
            }

            // If not we wrap it in an Enumerable with a single item
            return new[] { input };
        }

    }

}
