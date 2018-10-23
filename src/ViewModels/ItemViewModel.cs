using TomsToolbox.Desktop;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class ItemViewModel : ObservableObject
    {
        private string _content;
        private string _toolTip;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public string ToolTip
        {
            get => _toolTip;
            set
            {
                _toolTip = value;
                OnPropertyChanged();
            }
        }
    }
}