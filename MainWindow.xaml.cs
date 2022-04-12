using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Safali
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //コードを見たそこの君
        //えっちですよ!!
        public MainWindow()
        {
            InitializeComponent();

            browser.Address = "https://expired.badssl.com/";
            browser.BrowserSettings.Javascript = CefState.Enabled;
            // document.execCommandでのcopy&pasteを有効にする。
            browser.BrowserSettings.JavascriptDomPaste = CefState.Enabled;
            // localStorageを有効にする。
            browser.BrowserSettings.LocalStorage = CefState.Enabled;
            // フォントサイズを設定する。(レイアウトがずれる場合があるので注意)
            browser.BrowserSettings.DefaultFontSize = 16;
            browser.KeyboardHandler = new Handlers.KeyboardHandler();
            browser.RequestHandler = new Handlers.RequestHandler();
            browser.DisplayHandler = new Handlers.DisplayHandler();
            browser.PreviewMouseWheel += CefBrowser_PreviewMouseWheel;
            browser.KeyUp += CefBrowser_KeyUp;
        }

        private void CefBrowser_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
                browser.ZoomInCommand.Execute(null);
            else
                browser.ZoomOutCommand.Execute(null);
            e.Handled = true;
        }

        private void CefBrowser_KeyUp(object sender, KeyEventArgs e)
        {

            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Key == Key.Add)
                browser.ZoomInCommand.Execute(null);
            if (e.Key == Key.Subtract)
                browser.ZoomOutCommand.Execute(null);
            if (e.Key == Key.NumPad0)
                browser.ZoomLevel = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public static MainWindow getMainFrame(IBrowser browser)
        {
            IntPtr hWnd = browser.GetHost().GetWindowHandle();
            var rootVisual = HwndSource.FromHwnd(hWnd).RootVisual;
            return (MainWindow)rootVisual;
        }

        // Win32のGetParent
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);
    }

    public class PasswordBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
          DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));



        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
          DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }
}


