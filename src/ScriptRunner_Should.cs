using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using NEdifis.Attributes;
using NUnit.Framework;

namespace CaliburnMicroMessageNavigator
{
    [TestFixtureFor(typeof(ScriptRunner))]
    // ReSharper disable once InconsistentNaming
    internal class ScriptRunner_Should
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
            .WithEmitDebugInformation(true)
            .AddReferences(ReferencedAssemblies)
            .AddImports(Usings);

        [Test]
        public async Task Eval_Simple_Addidtion_ScriptAsync()
        {
            var value = await CSharpScript.EvaluateAsync("1 + 2");
            value.Should().Be(3);
        }

        [Test]
        public void Eval_Simple_Null_Object_Script()
        {
            Assert.ThrowsAsync<NullReferenceException>(async () => await CSharpScript.EvaluateAsync("object a = null; a.ToString();"));
        }

        [Test]
        public void Eval_Simple_Null_Object_ScriptAsync()
        {
            var cancellationToken = new CancellationToken();

            Assert.ThrowsAsync<NullReferenceException>(async () =>
             await Task.Run(async () =>
                {
                    try
                    {
                        return await CSharpScript.EvaluateAsync("object a = null; a.ToString();", ScriptOptions, null, null, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }, cancellationToken));
        }

        [Test]
        public void Throw_Argument_Exception()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await CSharpScript.EvaluateAsync(@"throw new System.ArgumentException(""Test"");"));
        }

    }
}