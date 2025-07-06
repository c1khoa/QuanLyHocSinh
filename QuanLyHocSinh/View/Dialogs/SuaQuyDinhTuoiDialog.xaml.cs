using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class SuaQuyDinhTuoiDialog : Window
    {
        public SuaQuyDinhTuoiDialog()
        {
            InitializeComponent();

        }
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // không phải số
            e.Handled = regex.IsMatch(e.Text);
        }

        // Chặn dán (Ctrl+V)
        private void NumberOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V))
            {
                e.Handled = true;
            }
        }
    }
}