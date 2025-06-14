using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.Helpers
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordHelper),
                new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordHelper),
                new PropertyMetadata(false));

        public static string GetBoundPassword(DependencyObject dp) =>
            (string)dp.GetValue(BoundPassword);

        public static void SetBoundPassword(DependencyObject dp, string value) =>
            dp.SetValue(BoundPassword, value);

        public static bool GetBindPassword(DependencyObject dp) =>
            (bool)dp.GetValue(BindPassword);

        public static void SetBindPassword(DependencyObject dp, bool value) =>
            dp.SetValue(BindPassword, value);

        private static bool GetUpdatingPassword(DependencyObject dp) =>
            (bool)dp.GetValue(UpdatingPassword);

        private static void SetUpdatingPassword(DependencyObject dp, bool value) =>
            dp.SetValue(UpdatingPassword, value);

        private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox passwordBox)
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                if (!(bool)GetUpdatingPassword(passwordBox))
                {
                    passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
                }

                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox passwordBox)
            {
                bool wasBound = (bool)(e.OldValue);
                bool needToBind = (bool)(e.NewValue);

                if (wasBound)
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }

                if (needToBind)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetUpdatingPassword(passwordBox, true);
                SetBoundPassword(passwordBox, passwordBox.Password);
                SetUpdatingPassword(passwordBox, false);
            }
        }
    }
}
