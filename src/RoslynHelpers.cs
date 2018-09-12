using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CaliburnMicroMessageNavigator
{
    internal class RoslynHelpers
    {
        public static IEnumerable<SyntaxNodeOrToken> GetSyntaxNodesAndTokens(IEnumerable<object> source )
        {
            return source
                .Select(AsSyntaxNodeOrToken)
                .Where(snot => snot != null)
                .Select(n => n.Value);
        }

        public static SyntaxNodeOrToken? AsSyntaxNodeOrToken(object item)
        {
            switch (item)
            {
                case SyntaxNode node:
                    return node;
                case SyntaxToken token:
                    return token;
            }

            return item as SyntaxNodeOrToken?;
        }
    }
}
