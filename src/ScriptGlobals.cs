using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CaliburnMicroMessageNavigator
{
    public class ScriptGlobals : IDisposable
    {
        private Workspace _workspace;

        public ScriptGlobals(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;

            IsSolutionOpen = RoslynVisxHelpers.Dte2?.Solution?.IsOpen ?? false;

            if (!IsSolutionOpen)
            {
                Task.Run(CheckIfSolutionIsOpenAsync);
            }
            else
            {
                #region Test Queries

                //var nodes = AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == "Handle" && md.ParameterList.Parameters.Last().Type.ToString().StartsWith("BusyEvent"));
                //var nodes = AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == "Handle" && System.Text.RegularExpressions.Regex.Match(md.ParameterList.Parameters.Last().Type.ToString(), "BusyEvent", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success);
                //var nodes = AllPublications.Where(ie =>
                //{
                //    var expression = ie.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                //    if (expression != null)
                //    {
                //        var sematicModel = Workspace.CurrentSolution.GetDocument(expression.SyntaxTree)
                //            .GetSemanticModelAsync().Result;
                //        var typeInfo = sematicModel.GetTypeInfo(expression);
                //        var fullTypeName = typeInfo.Type;
                //        if (fullTypeName != null)
                //        {
                //            var splittedType = fullTypeName.ToString().Split('.');
                //            if (splittedType.Length > 1)
                //            {
                //                var type = splittedType.Last();
                //                return Regex.Match(type, "ReportRequestSave", RegexOptions.IgnoreCase).Success;
                //            }
                //        }
                //    }

                //    return false;
                //}).ToList();

                #endregion            }
            }
        }


        /// <summary>
        ///     Provides access to the Workspace
        /// </summary>
        public Workspace Workspace
        {
            get => _workspace ?? (_workspace = RoslynVisxHelpers.GetWorkspace());
            internal set => _workspace = value;
        }

        /// <summary>
        ///     Returns the SyntaxToken that is currently selected in the active code window
        /// </summary>
        public SyntaxToken SelectedToken
        {
            get
            {
                var token = RoslynVisxHelpers.GetSelectedToken();
                if (!token.HasValue) throw new InvalidOperationException("No SyntaxToken is currently selected");

                return token.Value;
            }
        }

        public Document ActiveDocument => RoslynVisxHelpers.GetCodeAnalysisDocument();


        /// <summary>
        ///     Returns all SyntaxNodes for all the Documents for all Projects in the current Solution
        /// </summary>
        public IEnumerable<SyntaxNode> AllNodes =>
            Workspace.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .SelectMany(d => d.GetSyntaxRootAsync(CancellationToken)?.Result.DescendantNodesAndSelf());

        public IEnumerable<InvocationExpressionSyntax> AllPublications => AllNodes.OfType<InvocationExpressionSyntax>()
            .Where(ie =>
                Regex.Match(ie.Expression.ToString(), "publish.*(thread)?", RegexOptions.IgnoreCase).Success);

        public IEnumerable<MethodDeclarationSyntax> AllHandlers => AllNodes.OfType<MethodDeclarationSyntax>()
            .Where(md => md.Identifier.Text == "Handle");

        public IEnumerable<string> AllPublicationTypes => AllPublications.Select(p =>
        {
            if (p.ArgumentList.Arguments.Any())
            {
                var expression = p.ArgumentList.Arguments.First()?.Expression;
                if (expression != null)
                {
                    var semanticModel = Workspace.CurrentSolution.GetDocument(expression.SyntaxTree)
                        .GetSemanticModelAsync()?.Result;
                    if (semanticModel != null)
                    {
                        var sematicModel = semanticModel;
                        var typeInfo = sematicModel.GetTypeInfo(expression);
                        var fullTypeName = typeInfo.Type;
                        var splittedType = fullTypeName.ToString().Split('.');
                        if (splittedType.Length > 1)
                        {
                            var type = splittedType.Last();
                            return type;
                        }
                    }
                }
            }
            return null;
        }).Where(t => t != null).Distinct();

        public IEnumerable<string> AllHandlerTypes =>
            AllHandlers.Select(p => p.ParameterList.Parameters.Last().Type.ToString()).Distinct();

        public CancellationToken CancellationToken { get; }

        public bool IsSolutionOpen { get; set; }

        public void Dispose()
        {
            _workspace?.Dispose();
        }

        private async Task CheckIfSolutionIsOpenAsync()
        {
            while (!RoslynVisxHelpers.Dte2?.Solution?.IsOpen ?? true)
                // don't run again for at least 200 milliseconds
                await Task.Delay(200, CancellationToken);

            IsSolutionOpen = true;
            OnSolutionOpened();
        }

        public event EventHandler<object> SolutionOpened;

        protected virtual void OnSolutionOpened()
        {
            SolutionOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}