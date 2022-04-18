using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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
            browser.Address = @"https://lms.catchon.jp/";
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
            browser.LifeSpanHandler = new Handlers.LifespanHandler();
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

            if (e.Key == Key.OemPlus || e.Key == Key.Add)
                browser.ZoomInCommand.Execute(null);
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                browser.ZoomOutCommand.Execute(null);
            if (e.Key == Key.D0 || e.Key == Key.NumPad0)
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
                    Topmost = true,
                    Left = ExtensionClass.GetMousePosition().X - 70,
                    Top = ExtensionClass.GetMousePosition().Y + 30,
                };
                _previewWindow.Show();
                ForceActivate();
            }
            e.Handled = true;
        }

        private void TabItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_previewWindow != null)
                _previewWindow.Close();
            _previewWindow = null;
        }

        private void TabItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (_previewWindow != null)
                _previewWindow.Left = ExtensionClass.GetMousePosition().X - 70;
                _previewWindow.Top = ExtensionClass.GetMousePosition().Y + 30;
        }



        private void address_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MessageBox.Show("Test");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
            if (chromium.CanGoBack)
            {
                chromium.Back();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
            MessageBox.Show(tabControl.SelectedIndex.ToString());
            if (chromium.CanGoForward)
            {
                chromium.Forward();
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
            if (chromium.Title == "" || chromium.Title == null) {
                this.Title = "New Tab - Safali for Windows"; }
            else
                this.Title = chromium.Title + " - Safali for Windows";
        }

        public void ForceActivate()
        {
            this.Activate();

            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(100);
                Dispatcher.Invoke(() => this.Focus());
            });
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
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


