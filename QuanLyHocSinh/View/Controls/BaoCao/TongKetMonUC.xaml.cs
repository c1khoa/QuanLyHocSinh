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
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.BaoCao;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;

namespace QuanLyHocSinh.View.Controls.BaoCao
{
    public partial class TongKetMonUC : UserControl
    {
        public TongKetMonUC()
        {
            InitializeComponent();
        }

        private async void ChiTietButton_Click(object sender, RoutedEventArgs e)
        {
            try
        {
                if (sender is Button button && button.CommandParameter is TongKetLopItem lopItem)
            {
                    var mainViewModel = this.DataContext as TongKetMonViewModel;
                    if (mainViewModel == null)
                    {
                        await ShowNotificationAsync("❌ Lỗi: Không thể truy cập thông tin chính.");
                        return;
                    }

                    if (string.IsNullOrEmpty(mainViewModel.SelectedMonHoc))
                    {
                        await ShowNotificationAsync("⚠️ Vui lòng chọn môn học cụ thể để xem chi tiết.");
                        return;
                    }

                    var dialog = new View.Dialogs.TongKetMonDetailDialog();
                    dialog.DataContext = new TongKetMonDetailDialogViewModel(
                        lopItem.TenLop, 
                        mainViewModel.SelectedMonHoc, 
                        2, // Mặc định học kỳ 2
                        lopItem.NamHoc
                    );
                dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync($"❌ Lỗi khi mở chi tiết: {ex.Message}");
            }
        }

        private async Task ShowNotificationAsync(string message)
        {
            try
            {
                await DialogHost.Show(new NotifyDialog("Lỗi", message), "RootDialog_Main");
            }
            catch
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
