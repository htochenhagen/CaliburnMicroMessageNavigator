using Microsoft.CodeAnalysis;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class DiagnosticItemViewModel : ItemViewModel
    {
        public DiagnosticItemViewModel(Diagnostic diagnostic)
        {
            Diagnostic = diagnostic;
            Content = diagnostic.ToString();
        }

        public Diagnostic Diagnostic { get; }
    }
}