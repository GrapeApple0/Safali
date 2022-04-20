using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        //ちなみにワイは一日ちびちびやっているのでよろしくおねがいします
        //Twitter:https://twitter.com/GrapeApple0
        private Window _previewWindow;
        public MainWindow()
        {
            InitializeComponent();
            browser.Address = @"https://www.youtube.com/watch?v=wTcf-2zo_tk";
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
            browser.TitleChanged += Cbrowser_TitleChanged;
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
                    Viewport = new Rect(new Size(300, 200)),
                    Viewbox = new Rect(new Size(300, 200)),
                };

                var myRectangle = new Rectangle
                {
                    Width = 150,
                    Height = 100,
                    Stroke = Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = vb,
                };
                pt = ((Control)sender).PointToScreen(new Point(0.0d, 0.0d));
                _previewWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    Width = 150,
                    Height = 100,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Left = pt.X,
                    Top = pt.Y + 30,
                    Content = myRectangle,
                };
                _previewWindow.SizeChanged += previewWindow_SizeChanged;
                _previewWindow.Show();
                ForceActivate();
            }
            e.Handled = true;
        }

        void previewWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = sender as Window;
            if (window != null)
                window.Top = pt.Y + 30;
        }


        private void TabItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_previewWindow != null)
                _previewWindow.Close();
            _previewWindow = null;
        }
        Point pt;
        private void TabItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (_previewWindow != null)
            {
                _previewWindow.Left = pt.X;
                _previewWindow.Top = pt.Y + 30;
            }
        }

        private void address_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //ここは絶対0!
                var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
                chromium.Load(address.Text);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //ここは絶対0!
            var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
            if (chromium.CanGoBack)
            {
                chromium.Back();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            //ここは絶対0!
            var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
            if (chromium.CanGoForward)
            {
                chromium.Forward();
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ここは絶対0!
            /*
            var chromium = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.SelectedContent).ElementAtOrDefault(0);
            if (chromium.Title == "" || chromium.Title == null)
            {
                this.Title = "New Tab - Safali for Windows";
            }
            else
                this.Title = chromium.Title + " - Safali for Windows";
            */
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
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ChromiumWebBrowser item = ExtensionClass.FindVisualChilds<ChromiumWebBrowser>((DependencyObject)tabControl.Items[0]).ElementAtOrDefault(0);
            //System.Windows.Forms.MessageBox.Show(item.Address);
            int nChildCount = VisualTreeHelper.GetChildrenCount(tabControl);
            System.Windows.Forms.MessageBox.Show(nChildCount.ToString()+tabControl.Items.Count);
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            ChromiumWebBrowser cbrowser = new ChromiumWebBrowser();
            cbrowser.Address = @"https://www.youtube.com/watch?v=wTcf-2zo_tk";
            cbrowser.BrowserSettings.Javascript = CefState.Enabled;
            // document.execCommandでのcopy&pasteを有効にする。
            cbrowser.BrowserSettings.JavascriptDomPaste = CefState.Enabled;
            // localStorageを有効にする。
            cbrowser.BrowserSettings.LocalStorage = CefState.Enabled;
            // フォントサイズを設定する。(レイアウトがずれる場合があるので注意)
            cbrowser.BrowserSettings.DefaultFontSize = 16;
            cbrowser.KeyboardHandler = new Handlers.KeyboardHandler();
            cbrowser.RequestHandler = new Handlers.RequestHandler();
            cbrowser.DisplayHandler = new Handlers.DisplayHandler();
            cbrowser.LifeSpanHandler = new Handlers.LifespanHandler();
            cbrowser.PreviewMouseWheel += CefBrowser_PreviewMouseWheel;
            cbrowser.KeyUp += CefBrowser_KeyUp;
            cbrowser.TitleChanged += Cbrowser_TitleChanged;
            tabControl.Items.Add(new TabItem { Content = cbrowser ,Header = cbrowser.Title}); 
        }

        private void Cbrowser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Title = ((ChromiumWebBrowser)sender).Title + " - Safali for Windows";
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> commandHandler;
        private readonly Func<object, bool> canExecuteHandler;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> commandHandler, Func<object, bool> canExecuteHandler = null)
        {
            this.commandHandler = commandHandler;
            this.canExecuteHandler = canExecuteHandler;
        }
        public RelayCommand(Action commandHandler, Func<bool> canExecuteHandler = null)
            : this(_ => commandHandler(), canExecuteHandler == null ? null : new Func<object, bool>(_ => canExecuteHandler()))
        {
        }

        public void Execute(object parameter)
        {
            commandHandler(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return
                canExecuteHandler == null ||
                canExecuteHandler(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }

    public class RelayCommand<T> : RelayCommand
    {
        public RelayCommand(Action<T> commandHandler, Func<T, bool> canExecuteHandler = null)
            : base(o => commandHandler(o is T t ? t : default(T)), canExecuteHandler == null ? null : new Func<object, bool>(o => canExecuteHandler(o is T t ? t : default(T))))
        {
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


