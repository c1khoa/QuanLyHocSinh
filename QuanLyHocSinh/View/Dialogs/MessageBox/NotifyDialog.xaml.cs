using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Dialogs.MessageBox
{
    public partial class NotifyDialog : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(NotifyDialog), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(NotifyDialog), new PropertyMetadata(string.Empty));
        private string v;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public NotifyDialog()
        {
            InitializeComponent();
        }

        public NotifyDialog(string title, string message) : this()
        {
            Title = title;
            Message = message;
        }

        public NotifyDialog(string v)
        {
            this.v = v;
        }
    }
}
