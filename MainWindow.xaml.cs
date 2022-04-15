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
        private Window _previewWindow;
        public MainWindow()
        {
            InitializeComponent();
            browser.Address = @"https://www.youtube.com/playlist?list=PLAuaj5UkpoX-5qWuEG70l_OTxyALXpvzy";
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
            
        }

        private static TabItem GetTabItem(object sender)
        {
            var control = sender as Control;
            if (control == null)
                return null;
            var parent = control.TemplatedParent as ContentPresenter;
            if (parent == null)
                return null;
            return parent.TemplatedParent as TabItem;
        }

        void previewWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = sender as Window;
            if (window != null)
                window.Top += window.ActualHeight - 30;
        }

        private void TabItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var tabItem = GetTabItem(sender);
            if (tabItem != null)
            {
                var vb = new VisualBrush(tabItem.Content as Visual)
                {
                    //600,400
                    Viewport = new Rect(new Size(250, 100)),
                    Viewbox = new Rect(new Size(250, 100)),
                };

                var myRectangle = new Rectangle
                {
                    Width = 160,
                    Height = 80,
                    Stroke = Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = vb,
                };

                Point renderedLocation = ((Control)sender).TranslatePoint(new Point(0, 0), this);

                _previewWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ShowInTaskbar = false,
                    Content = myRectangle,
                    Left = renderedLocation.X + Left,
                    Top = renderedLocation.Y + Top,
                };
                // Top can only be calculated when the size is changed to the content, 
                // therefore the SizeChanged-event is triggered.
                _previewWindow.SizeChanged += previewWindow_SizeChanged;
                _previewWindow.Show();
            }

            e.Handled = true;
        }

        private void TabItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_previewWindow != null)
                _previewWindow.Close();
            _previewWindow = null;
        }

        public static MainWindow getMainFrame(IBrowser browser)
        {
            IntPtr hWnd = browser.GetHost().GetWindowHandle();
            var rootVisual = HwndSource.FromHwnd(hWnd).RootVisual;
            return (MainWindow)rootVisual;
        }
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


