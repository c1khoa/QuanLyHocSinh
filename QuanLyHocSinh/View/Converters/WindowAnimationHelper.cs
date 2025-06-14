using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QuanLyHocSinh.View.Converters
{
    public static class WindowAnimationHelper
    {
        public static void FadeOut(Window window, double duration = 0.3)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(duration));
            fadeOut.Completed += (s, e) => window.Close();
            window.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        public static void FadeIn(Window window, double duration = 0.3)
        {
            window.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(duration));
            window.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        public static Task FadeOutAsync(Window window, double duration = 0.3)
        {
            var tcs = new TaskCompletionSource<bool>();

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(duration));
            fadeOut.Completed += (s, e) =>
            {
                window.Close();
                tcs.SetResult(true);
            };

            window.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            return tcs.Task;
        }

        public static Task FadeInAsync(Window window, double duration = 0.3)
        {
            var tcs = new TaskCompletionSource<bool>();

            window.Opacity = 0;
            window.Show();

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(duration));
            fadeIn.Completed += (s, e) => tcs.SetResult(true);

            window.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            return tcs.Task;
        }

    }
}
