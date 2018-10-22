using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CaliburnMicroMessageNavigator
{
    public class ScriptGlobals
    {
        private Workspace _workspace;

        public ScriptGlobals(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;

            #region Test Queries

            //var nodes = AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == "Handle" && md.ParameterList.Parameters.Last().Type.ToString().StartsWith("BusyEvent"));
            //var nodes = AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == "Handle" && System.Text.RegularExpressions.Regex.Match(md.ParameterList.Parameters.Last().Type.ToString(), "BusyEvent", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success);
            //var nodes = AllPublications.Where(ie =>
            //{
            //    var expression = ie.ArgumentList.Arguments.First().Expression;
            //    var sematicModel = Workspace.CurrentSolution.GetDocument(expression.SyntaxTree)
            //        .GetSemanticModelAsync().Result;
            //    var typeInfo = sematicModel.GetTypeInfo(expression);
            //    var fullTypeName = typeInfo.Type;
            //    var splittedType = fullTypeName.ToString().Split('.');
            //    if (splittedType.Length > 1)
            //    {
            //        var type = splittedType.Last();
            //        return Regex.Match(type, "report", RegexOptions.IgnoreCase).Success;
            //    }

            //    return false;
            //}).ToList();

            #endregion
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
                .SelectMany(d => d.GetSyntaxRootAsync(CancellationToken).Result.DescendantNodesAndSelf());

        public IEnumerable<InvocationExpressionSyntax> AllPublications => AllNodes.OfType<InvocationExpressionSyntax>()
            .Where(ie =>
                Regex.Match(ie.Expression.ToString(), "publish.*(thread)?", RegexOptions.IgnoreCase).Success);

        public CancellationToken CancellationToken { get; }
    }
}