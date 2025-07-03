using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.View.Controls.TraCuu
{
    public partial class TraCuuGiaoVienUC : UserControl
    {
        public TraCuuGiaoVienUC()
        {
            InitializeComponent();
            
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is TraCuuGiaoVienViewModel vm)
                {
                    vm.FilterCommand?.Execute(null);
                }
            };
            
            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    if (LopPopup.IsOpen)
                    {
                        LopPopup.IsOpen = false;
                        e.Handled = true;
                    }
                    if (BoMonPopup.IsOpen)
                    {
                        BoMonPopup.IsOpen = false;
                        e.Handled = true;
                    }
                }
            };
            
            this.PreviewMouseDown += (s, e) =>
            {
                var clickedElement = e.OriginalSource as FrameworkElement;
                
                // Handle Lop popup
                if (LopPopup.IsOpen)
                {
                    bool isClickOnLopButton = LopDropdownButton.IsAncestorOf(clickedElement) || clickedElement == LopDropdownButton;
                    bool isClickInLopPopup = LopPopup.Child != null && 
                                           (((FrameworkElement)LopPopup.Child).IsAncestorOf(clickedElement) || clickedElement == LopPopup.Child);
                    
                    if (!isClickOnLopButton && !isClickInLopPopup)
                    {
                        LopPopup.IsOpen = false;
                    }
                }
                
                // Handle BoMon popup
                if (BoMonPopup.IsOpen)
                {
                    bool isClickOnBoMonButton = BoMonDropdownButton.IsAncestorOf(clickedElement) || clickedElement == BoMonDropdownButton;
                    bool isClickInBoMonPopup = BoMonPopup.Child != null && 
                                             (((FrameworkElement)BoMonPopup.Child).IsAncestorOf(clickedElement) || clickedElement == BoMonPopup.Child);
                    
                    if (!isClickOnBoMonButton && !isClickInBoMonPopup)
                    {
                        BoMonPopup.IsOpen = false;
                    }
                }
            };
        }

        private void LopDropdownButton_Click(object sender, RoutedEventArgs e)
        {
            LopPopup.IsOpen = !LopPopup.IsOpen;
            
            if (LopPopup.IsOpen)
            {
                LopPopup.Focus();
            }
        }

        private void LopCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var item = checkBox?.DataContext as LopCheckboxItem;
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is TraCuuGiaoVienViewModel viewModel)
                {
                    var selectedCount = viewModel.DanhSachLopCheckbox?.Count(x => x.IsSelected) ?? 0;
                    if (selectedCount == 0)
                    {
                        LopPopup.IsOpen = false;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
            
            e.Handled = true;
        }

        private void BoMonDropdownButton_Click(object sender, RoutedEventArgs e)
        {
            BoMonPopup.IsOpen = !BoMonPopup.IsOpen;
            
            if (BoMonPopup.IsOpen)
            {
                BoMonPopup.Focus();
            }
        }

        private void BoMonCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var item = checkBox?.DataContext as BoMonCheckboxItem;
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is TraCuuGiaoVienViewModel viewModel)
                {
                    var selectedCount = viewModel.DanhSachBoMonCheckbox?.Count(x => x.IsSelected) ?? 0;
                    if (selectedCount == 0)
                    {
                        BoMonPopup.IsOpen = false;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
            
            e.Handled = true;
        }
    }
}
