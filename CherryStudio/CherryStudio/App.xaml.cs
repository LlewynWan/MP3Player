using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CherryStudio
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void CustomWindow_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                win.DragMove();
            }
        }

        private void CustomWindowClose_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.Close();
        }

        private void LyricWindowClose_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.Visibility = Visibility.Hidden;
        }

        // 最小化
        private void CustomWindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.WindowState = WindowState.Minimized;
        }

        private void CustomWindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.WindowState = win.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}
