using System.ComponentModel;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Dialogs.MessageBox
{
    /// <summary>
    /// Interaction logic for ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : UserControl
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }
    }

    // ✅ ViewModel phải nằm ngoài class ConfirmDialog, nhưng vẫn trong cùng namespace
    public class ConfirmDialogViewModel
    {
        public string Message { get; set; }

        public ConfirmDialogViewModel(string message)
        {
            Message = message;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
