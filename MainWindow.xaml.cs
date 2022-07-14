using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Safali
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool fullScreen = false;
        Point pt;
        private Grid parent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await getSelectedWebView().EnsureCoreWebView2Async();
            getSelectedWebView().CoreWebView2.ContainsFullScreenElementChanged += this.CoreWebView2_ContainsFullScreenElementChanged;
        }

        private bool isReload = false;

        private void Reload(object sender, RoutedEventArgs e)
        {
            getSelectedWebView().Reload();
            isReload = true;
            ReloadBtn.Content = new MahApps.Metro.IconPacks.PackIconBootstrapIcons()
            {
                Kind = MahApps.Metro.IconPacks.PackIconBootstrapIconsKind.X,
                Width = 9,
                Height = 9,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        private WebView2 getSelectedWebView()
        {
            var grid = tab.SelectedContent as Grid;
            WebView2 webview = null;
            if (grid.Children.Count != 0)
            {
                webview = grid.Children[0] as WebView2;
            }
            else if (fullscreen.Children.Count != 0)
            {
                webview = fullscreen.Children[0] as WebView2;
            }
            return webview;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Regex.IsMatch(address.Text, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"))
                {
                    getSelectedWebView().Source = new Uri(address.Text);
                }
                else
                {
                    try
                    {
                        var domain = new Uri("http://" + address.Text).DnsSafeHost;
                        var splitHostName = domain.Split('.');
                        if (Array.IndexOf(tldlist.tld, splitHostName[splitHostName.Length - 1].ToUpper()) == -1)
                        {
                            getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={address.Text}");
                        }
                        else
                        {
                            getSelectedWebView().Source = new Uri("http://" + address.Text);
                        }
                    }
                    catch (Exception)
                    {
                        getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={address.Text}");
                    }
                }
            }
            else if (e.Key == Key.F12)
            {
                getSelectedWebView().CoreWebView2.OpenDevToolsWindow();
            }
        }

        [DefaultValue(false)]
        private WebView2 fullScreenWebView;
        public bool FullScreen
        {
            get { return fullScreen; }
            set
            {
                fullScreen = value;
                try
                {
                    if (value)
                    {
                        this.WindowState = WindowState.Normal;
                        this.WindowStyle = WindowStyle.None;
                        this.WindowState = WindowState.Maximized;
                        parent = (Grid)VisualTreeHelper.GetParent(getSelectedWebView());
                        fullScreenWebView = getSelectedWebView();
                        parent.Children.Remove(fullScreenWebView);
                        fullscreen.Children.Add(fullScreenWebView);
                    }
                    else
                    {
                        this.Activate();
                        this.WindowStyle = WindowStyle.SingleBorderWindow;
                        this.WindowState = WindowState.Normal;
                        fullscreen.Children.RemoveAt(0);
                        parent.Children.Add(fullScreenWebView);
                    }
                }
                catch
                {

                }

            }
        }

        private async void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await (sender as WebView2).EnsureCoreWebView2Async();
            isReload = false;
            ReloadBtn.Content = new MahApps.Metro.IconPacks.PackIconBootstrapIcons()
            {
                Kind = MahApps.Metro.IconPacks.PackIconBootstrapIconsKind.ArrowCounterclockwise,
                Width = 12,
                Height = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            (sender as WebView2).CoreWebView2.ContainsFullScreenElementChanged -= this.CoreWebView2_ContainsFullScreenElementChanged;
            (sender as WebView2).CoreWebView2.ContainsFullScreenElementChanged += this.CoreWebView2_ContainsFullScreenElementChanged;
            address.Text = getSelectedWebView().Source.ToString();
            await Task.Delay(500);
            try
            {

                if (makeTabHeader(selectItem().Width, getSelectedWebView().CoreWebView2.DocumentTitle ?? "New Tab") != null)
                {
                    changeTitle(getSelectedWebView().CoreWebView2.DocumentTitle ?? "New Tab");
                }
                if (getSelectedWebView() != null)
                {
                    this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
                }
            }
            catch { }
        }

        private void CoreWebView2_ContainsFullScreenElementChanged(object sender, object e)
        {
            this.FullScreen = (sender as CoreWebView2).ContainsFullScreenElement;
        }

        public string getTitle()
        {
            return ((selectItem().Header as Grid).Children[1] as TextBlock).Text;
        }

        public void changeTitle(string title)
        {
            ((selectItem().Header as Grid).Children[1] as TextBlock).Text = title;
        }

        private TabItem selectItem()
        {
            return (tab.SelectedItem as TabItem);
        }

        private void titleChange()
        {
            address.Text = getSelectedWebView().Source.ToString();
            this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                titleChange();
            }
            catch
            {

            }
        }

        private Grid makeTabHeader(double tabwidth, string text = "New Tab")
        {
            //CloseButton
            var CloseButton = new Button();
            CloseButton.Click += CloseButton_Click;
            CloseButton.Width = 17;

            CloseButton.Height = 17;
            CloseButton.HorizontalAlignment = HorizontalAlignment.Center;
            CloseButton.Background = null;
            CloseButton.BorderBrush = null;
            Grid.SetColumn(CloseButton, 0);
            CloseButton.Content = new MahApps.Metro.IconPacks.PackIconBootstrapIcons()
            {
                Kind = MahApps.Metro.IconPacks.PackIconBootstrapIconsKind.X,
                Width = 7,
                Height = 7,
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            //HeaderText
            var headerText = new TextBlock();
            headerText.Text = text;
            headerText.Height = 18;
            headerText.Margin = new Thickness(24, 0, 24, 0);
            headerText.TextAlignment = TextAlignment.Center;
            Grid.SetColumn(headerText, 1);
            //Grid
            var grid1 = new Grid();
            grid1.MinWidth = 205;
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(17, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.MinWidth = 190;
            grid1.ColumnDefinitions.Add(c1);
            grid1.ColumnDefinitions.Add(c2);
            grid1.Children.Add(CloseButton);
            grid1.Children.Add(headerText);
            return grid1;
        }

        public T CloneXamlElement<T>(T source) where T : class
        {
            if (source == null)
                return null;
            object cloned = null;
            using (var stream = new MemoryStream())
            {
                XamlWriter.Save(source, stream);
                stream.Seek(0, SeekOrigin.Begin);
                cloned = XamlReader.Load(stream);
            }
            return (cloned is T) ? (T)cloned : null;
        }

        private Window _previewWindow;

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
                    Viewport = new Rect(new Size(200, 100)),
                    Viewbox = new Rect(new Size(200, 100)),
                };

                var myRectangle = new Rectangle
                {
                    Width = 200,
                    Height = 100,
                    Stroke = Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = vb,
                };
                pt = ((Control)sender).PointToScreen(new Point(0.0d, 0.0d));
                _previewWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    Width = 200,
                    Height = 100,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Left = pt.X,
                    Top = pt.Y + 30,
                    Content = myRectangle,
                };
                _previewWindow.SizeChanged += previewWindow_SizeChanged;
                _previewWindow.Show();
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

        private void TabItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (_previewWindow != null)
            {
                _previewWindow.Left = pt.X;
                _previewWindow.Top = pt.Y + 30;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (tab.Items.Count <= 1)
            {
                Application.Current.Shutdown();
            }
            tab.Items.RemoveAt(tab.SelectedIndex);
        }

        private void NewTab(object sender, RoutedEventArgs e)
        {
            var webview = new WebView2();
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.HorizontalAlignment = HorizontalAlignment.Stretch;
            webview.VerticalAlignment = VerticalAlignment.Stretch;
            webview.Source = new Uri("https://google.com/");
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.SourceChanged += WebView2_SourceChanged;
            var grid = new Grid();
            grid.Children.Add(webview);
            var tabitem = new TabItem();
            tabitem.Content = grid;
            tabitem.Width = 215;
            tabitem.Header = makeTabHeader(215);
            tabitem.Margin = new Thickness(-2, 3, -2, -4);
            tabitem.SizeChanged += TabItem_SizeChanged;
            tab.Items.Add(tabitem);
            tab.SelectedItem = tabitem;
        }

        private async void WebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            await getSelectedWebView().EnsureCoreWebView2Async();
            address.Text = getSelectedWebView().Source.ToString();
            await Task.Delay(500);
            if (selectItem() != null)
            {
                try
                {
                    titleChange();
                }
                catch
                {

                }
            }
        }

        private async void WebView2_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            await getSelectedWebView().EnsureCoreWebView2Async();
            address.Text = getSelectedWebView().Source.ToString();
            await Task.Delay(500);
            try
            {
                if (selectItem() != null)
                {
                    try
                    {
                        titleChange();
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {
            }

        }

        private void address_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;
        }

        private void tab_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void TabItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*
            if (tab.ActualWidth / tab.Items.Count <= 215)
            {
                foreach (TabItem tabitem in tab.Items)
                {
                    tabitem.Header = makeTabHeader(tab.ActualWidth / tab.Items.Count, getTitle());
                }
            }
            else
            {
                foreach (TabItem tabitem in tab.Items)
                {
                    tabitem.Header = makeTabHeader(215, getTitle());
                }
            }
            */
        }
    }
}
