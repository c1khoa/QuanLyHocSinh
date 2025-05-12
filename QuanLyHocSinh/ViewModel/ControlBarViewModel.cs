using QuanLyHocSinh.View.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel
{
    public class ControlBarViewModel : BaseViewModel
    {
        #region commands
        public ICommand CloseWindowCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; set; }
        public ICommand MaximizeWindowCommand { get; set; }
        public ICommand MouseDownWindowCommand { get; set; }
        #endregion
        public ControlBarViewModel()
        {
            MinimizeWindowCommand = new RelayCommand<UserControl>(
                (p) => { return p == null ? false : true; },
                (p) => {
                    FrameworkElement window = GetWindowParents(p);
                    var w = (Window)window;
                    if (w != null)
                    {   
                       if (w.WindowState != WindowState.Minimized)
                            w.WindowState = WindowState.Minimized;
                       else
                            w.WindowState = WindowState.Maximized;
                    }
                }
            );
            MaximizeWindowCommand = new RelayCommand<UserControl>(
                (p) => { return p == null ? false : true; },
                (p) => {
                    FrameworkElement window = GetWindowParents(p);
                    var w = (Window)window;
                    if (w != null)
                    {
                        if (w.WindowState != WindowState.Maximized)
                            w.WindowState = WindowState.Maximized;
                        else
                            w.WindowState = WindowState.Normal;
                    }
                }
            );
            CloseWindowCommand = new RelayCommand<UserControl>(
                (p) => { return p == null ? false : true; },
                (p) => {
                    FrameworkElement window = GetWindowParents(p);
                    var w = (Window)window;
                    if (w != null)
                    {
                        w.Close();
                    }
                }
            );
            MouseDownWindowCommand =  new RelayCommand<UserControl>(
                (p) => { return p == null ? false : true; },
                (p) =>
                {
                    FrameworkElement window = GetWindowParents(p);
                    var w = (Window)window;
                    if (w != null)
                    {
                        w.DragMove();
                    }
                }
            );

        }
        FrameworkElement GetWindowParents(UserControl p)
        {
            FrameworkElement parent = p;
            while (parent.Parent != null)
            {
                parent = (FrameworkElement)parent.Parent;
            }
            return parent;
        }

    }
}
