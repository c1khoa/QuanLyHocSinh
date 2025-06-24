using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace QuanLyHocSinh.View.Converters
{
        public static class VisualTreeHelperExtensions
        {
            public static T FindVisualChild<T>(this DependencyObject parent) where T : DependencyObject
            {
                if (parent == null) return null;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child is T tChild)
                        return tChild;

                    var result = FindVisualChild<T>(child);
                    if (result != null)
                        return result;
                }

                return null;
            }
        }
    }
