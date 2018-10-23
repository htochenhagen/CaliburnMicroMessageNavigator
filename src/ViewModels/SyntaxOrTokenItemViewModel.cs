using CaliburnMicroMessageNavigator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class SyntaxOrTokenItemViewModel : ItemViewModel
    {
        public FileLinePositionSpan LineSpan { get; }

        public SyntaxOrTokenItemViewModel(SyntaxNodeOrToken syntaxNodeOrToken)
        {
            LineSpan = syntaxNodeOrToken.GetLocation().GetLineSpan();

            Content = $"{LineSpan}: {syntaxNodeOrToken.Kind()} {syntaxNodeOrToken.ToString().Truncate()}";
            ToolTip = syntaxNodeOrToken.ToString();
        }
    }
}