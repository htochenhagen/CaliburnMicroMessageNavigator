using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using CaliburnMicroMessageNavigator.Extensions;
using CaliburnMicroMessageNavigator.ToolWindows;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using TomsToolbox.Desktop;
using TomsToolbox.Wpf;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class MessageNavigatorToolWindowControlViewModel : ObservableObject
    {
        private const int PageLimit = 10000;
        private readonly ScriptGlobals _scriptGlobals;
        private CancellationTokenSource _cancellationTokenSource;
        private ItemViewModel _currentHandler;
        private ItemViewModel _currentPublication;
        private List<string> _errorList;
        private string _errors;
        private ObservableCollection<ItemViewModel> _handlers;
        private bool _hasErrors;
        private bool _isEnabled;
        private bool _isSearchInputFocused;
        private ObservableCollection<string> _messageTypes;
        private bool _onSearching;
        private ObservableCollection<ItemViewModel> _publications;
        private string _searchText;
        private string _status;


        public MessageNavigatorToolWindowControlViewModel(MessageNavigatorToolWindowState state)
        {
            try
            {
                ToolWindowState = state;

                Status = "Ready";
                Errors = null;
                ErrorList = new List<string>();

                _scriptGlobals = new ScriptGlobals(ResetCancellationToken());
                if (_scriptGlobals.IsSolutionReady)
                    Initialize();
                else
                    _scriptGlobals.SolutionReady += OnSolutionReady;

                Publications = new ObservableCollection<ItemViewModel>();
                Handlers = new ObservableCollection<ItemViewModel>();
            }
            catch (Exception ex)
            {
                HandleError(ex);
                throw;
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsSearchInputFocused
        {
            get => _isSearchInputFocused;
            set
            {
                _isSearchInputFocused = value;
                OnPropertyChanged();
            }
        }


        public bool OnSearching
        {
            get => _onSearching;
            set
            {
                _onSearching = value;
                OnPropertyChanged();
            }
        }

        public ItemViewModel CurrentPublication
        {
            get => _currentPublication;
            set
            {
                _currentPublication = value;
                OnPropertyChanged();
            }
        }

        public ItemViewModel CurrentHandler
        {
            get => _currentHandler;
            set
            {
                _currentHandler = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand => new DelegateCommand(CanSearch, Search);

        public ICommand CancelCommand => new DelegateCommand(CanCancel, Cancel);


        public ICommand ExecuteDefaultPublicationItemCommand =>
            new DelegateCommand(CanExecuteDefaultPublicationItem, ExecuteDefaultPublicationItem);

        public ICommand ExecuteDefaultHandlerItemCommand =>
            new DelegateCommand(CanExecuteDefaultHandlerItem, ExecuteDefaultHandlerItem);

        public ObservableCollection<string> MessageTypes
        {
            get => _messageTypes;
            set
            {
                _messageTypes = value;
                OnPropertyChanged();
            }
        }

        public string Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                OnPropertyChanged();
            }
        }


        public List<string> ErrorList
        {
            get => _errorList;
            set
            {
                _errorList = value;
                OnPropertyChanged();
            }
        }

        public bool HasErrors
        {
            get => _hasErrors;
            set
            {
                _hasErrors = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ItemViewModel> Publications
        {
            get => _publications;
            set
            {
                _publications = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ItemViewModel> Handlers
        {
            get => _handlers;
            set
            {
                _handlers = value;
                OnPropertyChanged();
            }
        }

        public MessageNavigatorToolWindowState ToolWindowState { get; }

        private bool CanSearch()
        {
            return IsEnabled && !_onSearching;
        }
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void Search()
        {
            try
            {
                OnSearching = true;
                var cancellationToken = ResetCancellationToken();

                ErrorList.Clear();
                Errors = null;

                var publicationsSearchTask = ExecutePublicationsSearchAsync(cancellationToken);
                var handlersSearchTask = ExecuteHandlersSearchAsync(cancellationToken);

                await Task.WhenAll(publicationsSearchTask, handlersSearchTask);

                var publicationsCount = publicationsSearchTask.Result;
                var handlersCount = handlersSearchTask.Result;

                Status = $"{publicationsCount} publications and {handlersCount} handlers found.";
            }
            finally
            {
                OnSearching = false;
                IsSearchInputFocused = false;
                IsSearchInputFocused = true;
            }
        }

        private bool CanCancel()
        {
            return IsEnabled && _onSearching;
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();

            OnSearching = false;

            foreach (var itemViewModel in Publications.Where(x => x is PageRequestItemViewModel).ToArray())
                Publications.Remove(itemViewModel);
            foreach (var itemViewModel in Handlers.Where(x => x is PageRequestItemViewModel).ToArray())
                Handlers.Remove(itemViewModel);
        }

        private bool CanExecuteDefaultPublicationItem()
        {
            return IsEnabled;
        }

        private void ExecuteDefaultPublicationItem()
        {
            if (CurrentPublication is PageRequestItemViewModel pageRequestItem)
            {
                Publications.Remove(pageRequestItem);
                pageRequestItem.TaskCompletionSource.SetResult(true);
            }
            else if (CurrentPublication is DiagnosticItemViewModel diagnosticItem)
            {
                // Select the text that corresponds to the diagnostic
                var sourceSpan = diagnosticItem.Diagnostic.Location.SourceSpan;
                var codeTextBox = new TextBox();
                codeTextBox.Select(sourceSpan.Start, sourceSpan.Length);

                codeTextBox.ScrollToLine(diagnosticItem.Diagnostic.Location.GetLineSpan().StartLinePosition.Line);
                codeTextBox.Focus();
            }
            else if (CurrentPublication is SyntaxOrTokenItemViewModel syntaxOrTokenItem)
            {
                RoslynVisxHelpers.SelectSpanInCodeWindow(syntaxOrTokenItem.LineSpan);
            }
        }

        private bool CanExecuteDefaultHandlerItem()
        {
            return IsEnabled;
        }

        private void ExecuteDefaultHandlerItem()
        {
            if (CurrentHandler is PageRequestItemViewModel pageRequestItem)
            {
                Handlers.Remove(pageRequestItem);
                pageRequestItem.TaskCompletionSource.SetResult(true);
            }
            else if (CurrentHandler is DiagnosticItemViewModel diagnosticItem)
            {
                // Select the text that corresponds to the diagnostic
                var sourceSpan = diagnosticItem.Diagnostic.Location.SourceSpan;
                var codeTextBox = new TextBox();
                codeTextBox.Select(sourceSpan.Start, sourceSpan.Length);

                codeTextBox.ScrollToLine(diagnosticItem.Diagnostic.Location.GetLineSpan().StartLinePosition.Line);
                codeTextBox.Focus();
            }
            else if (CurrentHandler is SyntaxOrTokenItemViewModel syntaxOrTokenItem)
            {
                RoslynVisxHelpers.SelectSpanInCodeWindow(syntaxOrTokenItem.LineSpan);
            }
        }

        private void OnSolutionReady(object sender, object e)
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                HandleError(ex);
                throw;
            }
        }

        private void HandleError(Exception ex)
        {
            ErrorList.Add(ex.GetErrorMessage());
            Errors = string.Join(Environment.NewLine, ErrorList);
            HasErrors = ErrorList.Count > 0;
        }

        private void Initialize()
        {
            IsEnabled = _scriptGlobals.IsSolutionReady;

            var allPublicationTypes = _scriptGlobals.AllPublicationTypes.ToList();
            var allHandlerTypes = _scriptGlobals.AllHandlerTypes.ToList();

            MessageTypes = new ObservableCollection<string>(allPublicationTypes.Union(allHandlerTypes).OrderBy(x => x));
        }

        private async Task<int> ExecutePublicationsSearchAsync(CancellationToken cancellationToken)
        {
            Publications.Clear();

            //var searchExpression = SearchTextBox.Text;
            var searchExpression =
                @"AllPublications.Where(ie =>
            {
                var expression = ie.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                if (expression != null)
                {
                    var sematicModel = Workspace.CurrentSolution.GetDocument(expression.SyntaxTree).GetSemanticModelAsync().Result;
                    var typeInfo = sematicModel.GetTypeInfo(expression);
                    var fullTypeName = typeInfo.Type;
                    if (fullTypeName != null)
                    {
                        var splittedType = fullTypeName.ToString().Split('.');
                        if (splittedType.Length > 1)
                        {
                            var type = fullTypeName.ToString().Split('.').Last();
                            return System.Text.RegularExpressions.Regex.Match(type, """ + SearchText +
                @""", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success;
                        }
                    }
                }
                return false;
            }).ToList()";
            try
            {
                var scriptResult = await ScriptRunner.RunScriptAsync(searchExpression,
                    new ScriptGlobals(cancellationToken), cancellationToken);

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
            Handlers.Clear();

            //var searchExpression = SearchTextBox.Text;
            var searchExpression =
                $"AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == \"Handle\" && System.Text.RegularExpressions.Regex.Match(md.ParameterList.Parameters.Last().Type.ToString(), \"{SearchText}\", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success).ToList()";
            try
            {
                var scriptResult = await ScriptRunner.RunScriptAsync(searchExpression,
                    new ScriptGlobals(cancellationToken), cancellationToken);

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
            Publications.Clear();

            var maxFilePathLength = 0;
            var expressions = MakeEnumerable(scriptResult).ToList().Select(e => e as ExpressionSyntax).Where(e =>
                e != null).ToList();
            if (expressions.Any())
                maxFilePathLength = expressions.Max(i =>
                    i?.GetLocation()?.SourceTree
                        ?.FilePath?.Length ?? 0);
            return await IterateItemsAsync(scriptResult, maxFilePathLength, cancellationToken,
                AddToListViewPublications,
                WaitForNextPublicationsPageRequestAsync);
        }

        private async Task<int> DisplayHandlersResultAsync(object scriptResult, CancellationToken cancellationToken)
        {
            Handlers.Clear();

            var maxFilePathLength = 0;
            var methodDeclarations = MakeEnumerable(scriptResult).ToList().Select(md => md as MethodDeclarationSyntax)
                .Where(md =>
                    md != null).ToList();
            if (methodDeclarations.Any())
                maxFilePathLength = methodDeclarations.Max(d =>
                    d?.GetLocation()?.SourceTree
                        ?.FilePath?.Length ?? 0);

            return await IterateItemsAsync(scriptResult, maxFilePathLength, cancellationToken, AddToListViewHandlers,
                WaitForNextHandlersPageRequestAsync);
        }

        private Task WaitForNextPublicationsPageRequestAsync(int pageLimit)
        {
            var moreItemsTaskCompletionSource = new TaskCompletionSource<bool>();
            var moreItemsItem = new PageRequestItemViewModel
            {
                Content = $"More than {pageLimit} publications found, click to fetch more",
                TaskCompletionSource = moreItemsTaskCompletionSource
            };

            Publications.Add(moreItemsItem);

            // Return the Task that will complete when the ListViewItem is clicked
            return moreItemsTaskCompletionSource.Task;
        }

        private Task WaitForNextHandlersPageRequestAsync(int pageLimit)
        {
            var moreItemsTaskCompletionSource = new TaskCompletionSource<bool>();
            var moreItemsItem = new PageRequestItemViewModel
            {
                Content = $"More than {pageLimit} handlers found, click to fetch more",
                TaskCompletionSource = moreItemsTaskCompletionSource
            };

            Handlers.Add(moreItemsItem);

            // Return the Task that will complete when the ListViewItem is clicked
            return moreItemsTaskCompletionSource.Task;
        }

        private void AddToListViewHandlers(object item, int? maxFilePathLength = null)
        {
            var foundHandleResult = CreateFoundResult(item, maxFilePathLength);
            Handlers.Add(foundHandleResult);
        }

        private void AddToListViewPublications(object item, int? maxFilePathLength = null)
        {
            var foundHandleResult = CreateFoundResult(item, maxFilePathLength);
            Publications.Add(foundHandleResult);
        }

        private void DisplayDiagnosticsForHandlers(IEnumerable<Diagnostic> diagnostics)
        {
            Handlers.Clear();

            foreach (var diagnostic in diagnostics)
            {
                var item = new DiagnosticItemViewModel(diagnostic);
                Handlers?.Add(item);
            }
        }

        private void DisplayDiagnosticsForPublications(IEnumerable<Diagnostic> diagnostics)
        {
            Publications.Clear();

            foreach (var diagnostic in diagnostics)
            {
                var item = new DiagnosticItemViewModel(diagnostic);
                Publications?.Add(item);
            }
        }

        /// <summary>
        ///     Cancells any pending operation and creates a new CancellationToken for a new Cancellable operation
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

        private static async Task<int> IterateItemsAsync(object scriptResult, int maxFilePathLength,
            CancellationToken cancellationToken,
            Action<object, int?> itemAction, Func<int, Task> waitForNextsPageRequestAction)
        {
            var count = 0;
            var pageLimit = PageLimit;

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
                        pageLimit += PageLimit;
                    }

                    var item = enumerator.Current;
                    // Start getting the next item while we process the current
                    moveNextTask = enumerator.MoveNextAsync();

                    itemAction(item, maxFilePathLength);
                }
            }

            return count;
        }

        private static ItemViewModel CreateFoundResult(object item, int? maxFilePathLength = null)
        {
            var syntaxNodeOrToken = RoslynHelpers.AsSyntaxNodeOrToken(item);
            if (syntaxNodeOrToken != null)
                return new SyntaxOrTokenItemViewModel(syntaxNodeOrToken.Value, maxFilePathLength);

            return new ItemViewModel {Content = item?.ToString() ?? "<null>"};
        }

        private static IEnumerable<object> MakeEnumerable(object input)
        {
            if (input is IEnumerable enumerable && !(input is string)) return enumerable.Cast<object>();

            // If not we wrap it in an Enumerable with a single item
            return new[] {input};
        }
    }
}