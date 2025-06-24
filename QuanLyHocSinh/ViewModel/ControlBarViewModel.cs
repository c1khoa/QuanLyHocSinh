using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Converters;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyHocSinh.ViewModel
{
    public class ControlBarViewModel : BaseViewModel
    {
        public ICommand CloseWindowCommand { get; }
        public ICommand MinimizeWindowCommand { get; }
        public ICommand MaximizeWindowCommand { get; }
        public ICommand MouseDownWindowCommand { get; }
        

        public ControlBarViewModel()
        {
            MinimizeWindowCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    var w = Application.Current.Windows
                                .OfType<Window>()
                                .FirstOrDefault(x => x.IsActive);
                    if (w != null)
                        w.WindowState = WindowState.Minimized;
                });

            MaximizeWindowCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    var w = Application.Current.Windows
                                .OfType<Window>()
                                .FirstOrDefault(x => x.IsActive);
                    if (w != null)
                        w.WindowState = w.WindowState == WindowState.Maximized
                            ? WindowState.Normal
                            : WindowState.Maximized;
                });

            CloseWindowCommand = new RelayCommand<object>(
                            async (p) =>
                            {
                                var window = Application.Current.Windows
                                                .OfType<Window>()
                                                .FirstOrDefault(x => x.IsActive);

                                if (window == null) return;

                                var dialogHost = window.FindVisualChild<DialogHost>();
                                if (dialogHost == null) return;

                                var dialog = new ConfirmDialog
                                {
                                    DataContext = new ConfirmDialogViewModel("Bạn có muốn thoát không?")
                                };

                                var result = await dialogHost.ShowDialog(dialog);

                                if (result?.ToString() == "True")
                                {
                                    window.Close();
                                }
                            });


            MouseDownWindowCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    var w = Application.Current.Windows
                                .OfType<Window>()
                                .FirstOrDefault(x => x.IsActive);
                    w?.DragMove();
                });
        }
    }
}
