namespace CaliburnMicroMessageNavigator.Events
{
    public class SourceCodeTextEventMessage
    {
        public SourceCodeTextEventMessage(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}