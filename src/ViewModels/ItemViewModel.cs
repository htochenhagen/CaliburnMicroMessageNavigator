using TomsToolbox.Desktop;

namespace CaliburnMicroMessageNavigator.ViewModels
{
    public class ItemViewModel : ObservableObject
    {
        private string _content;
        private string _toolTip;
        private string _class;

        public string Class
        {
            get => _class;
            set
            {
                _class = value;
                OnPropertyChanged();
            }
        }

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