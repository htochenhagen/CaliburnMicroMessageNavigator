using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace CaliburnMicroMessageNavigator
{
    public class ScriptGlobals
    {
        private Workspace _workspace;

        public ScriptGlobals(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            //var nodes = AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == "Handle" && md.ParameterList.Parameters.Last().Type.ToString().StartsWith("BusyEvent"));
            //var nodes = AllNodes.OfType<MethodDeclarationSyntax>().Where(md => md.Identifier.Text == "Handle" && System.Text.RegularExpressions.Regex.Match(md.ParameterList.Parameters.Last().Type.ToString(), "BusyEvent", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Success);
        }

        /// <summary>
        /// Provides access to the Workspace
        /// </summary>
        public Workspace Workspace
        {
            get => _workspace ?? (_workspace = RoslynVisxHelpers.GetWorkspace());
            internal set => _workspace = value;
        }

        /// <summary>
        /// Returns the SyntaxToken that is currently selected in the active code window
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
        /// Returns all SyntaxNodes for all the Documents for all Projects in the current Solution
        /// </summary>
        public IEnumerable<SyntaxNode> AllNodes =>
            Workspace.CurrentSolution.Projects
            .SelectMany(p => p.Documents)
            .SelectMany(d => d.GetSyntaxRootAsync(CancellationToken).Result.DescendantNodesAndSelf());

        public CancellationToken CancellationToken { get; }

    }
}