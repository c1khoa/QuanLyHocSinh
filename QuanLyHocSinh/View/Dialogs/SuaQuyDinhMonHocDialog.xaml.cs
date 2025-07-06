using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class SuaQuyDinhMonHocDialog : Window
    {
        public SuaQuyDinhMonHocDialog()
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

        // Cho phép số từ 0 đến 10 với tối đa 2 chữ số thập phân
        private void NumberFloatOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string previewText = GetPreviewText(textBox, e.Text);

            // Kiểm tra chuỗi preview có phải là số hợp lệ trong khoảng 0–10 không
            e.Handled = !IsValidInput(previewText);
        }

        private bool IsValidInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return true;

            // Cho phép gõ số dở dang: ".", "0.", "0.1", "10.", "10.0", v.v.
            if (!Regex.IsMatch(input, @"^\d{0,2}([.]\d{0,2})?$"))
                return false;

            // Kiểm tra giá trị cuối cùng nếu hợp lệ
            if (double.TryParse(input, out double value))
            {
                return value >= 0 && value <= 10;
            }

            // Trường hợp gõ dở như ".", "1." vẫn cho phép
            return true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string previewText = GetPreviewText(textBox, e.Text);
                e.Handled = !IsValidDecimalInput(previewText);
            }
        }

        // Hàm tạo preview sau khi nhập
        private string GetPreviewText(TextBox textBox, string input)
        {
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;

            string text = textBox.Text.Remove(selectionStart, selectionLength);
            text = text.Insert(selectionStart, input);
            return text;
        }

        // Hàm kiểm tra input hợp lệ (0 đến 10, tối đa 2 chữ số sau dấu chấm)
        private bool IsValidDecimalInput(string input)
        {
            // Cho nhập dở dang: ".", "1.", "10.", "0.5", "0.55", ...
            if (!Regex.IsMatch(input, @"^\d{0,2}(\.\d{0,2})?$"))
                return false;

            if (double.TryParse(input, out double value))
            {
                return value >= 0 && value <= 10;
            }

            return true;
        }




    }
}