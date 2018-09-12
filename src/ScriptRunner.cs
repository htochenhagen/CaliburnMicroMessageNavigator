using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;

namespace CaliburnMicroMessageNavigator
{
    internal class ScriptRunner
    {

        // These types are used to set the references and Usings for the script
        private static readonly IEnumerable<Type> NeededTypes = new[]
        {
            typeof (object),
            typeof (Enumerable),
            typeof (Workspace),
            typeof (ModelExtensions),
            typeof (CSharpSyntaxNode),
            typeof (ClassDeclarationSyntax),
            typeof (MethodDeclarationSyntax)
        };

        private static readonly string[] Usings = NeededTypes.Select(t => t.Namespace).Distinct().ToArray();
        private static readonly IEnumerable<Assembly> ReferencedAssemblies = NeededTypes.Select(t => t.Assembly).ToArray();
        
        private static readonly ScriptOptions ScriptOptions = ScriptOptions.Default
            .AddReferences(ReferencedAssemblies)
            .AddImports(Usings);

        public static Task<object> RunScriptAsync(string code, object globals, CancellationToken cancellationToken)
        {
            // Execute the whole script on the thread pool
            return Task.Run(async () => await CSharpScript.EvaluateAsync(code, ScriptOptions, globals, null, cancellationToken), cancellationToken);
        }

    }
}
