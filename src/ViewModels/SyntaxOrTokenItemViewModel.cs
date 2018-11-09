using CaliburnMicroMessageNavigator.Extensions;
using Microsoft.CodeAnalysis;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class SyntaxOrTokenItemViewModel : ItemViewModel
    {
        public SyntaxOrTokenItemViewModel(SyntaxNodeOrToken syntaxNodeOrToken, int? maxFilePathLength = null)
        {
            LineSpan = syntaxNodeOrToken.GetLocation().GetLineSpan();

            Content =
                $"{LineSpan.Path.PadRight(maxFilePathLength ?? 100)} {LineSpan.Span.ToString().PadLeft(20)}{"".PadLeft(5)}{syntaxNodeOrToken.ToString().Truncate()}";
            ToolTip = syntaxNodeOrToken.ToString();
        }

        public FileLinePositionSpan LineSpan { get; }
    }
}