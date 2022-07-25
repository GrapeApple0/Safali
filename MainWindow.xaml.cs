using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using SharpVectors;

namespace Safali
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Uri uri;
        private WebView2 getSelectedWebView(Grid content = null)
        {
            var grid = content ?? (tab.SelectedContent as Grid);
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

        #region ActionButtons
        private void Reload(object sender, RoutedEventArgs e)
        {
            getSelectedWebView().Reload();
            ReloadBtn.Content = new MahApps.Metro.IconPacks.PackIconBootstrapIcons()
            {
                Kind = MahApps.Metro.IconPacks.PackIconBootstrapIconsKind.X,
                Width = 9,
                Height = 9,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        private void forward(object sender, RoutedEventArgs e)
        {
            if (getSelectedWebView().CanGoForward)
            {
                getSelectedWebView().GoForward();
            }
        }

        private void back(object sender, RoutedEventArgs e)
        {
            if (getSelectedWebView().CanGoBack)
            {
                getSelectedWebView().GoBack();
            }
        }
        #endregion

        #region window
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _panel = new System.Windows.Forms.Panel();
            windowsFormsHost1.Child = _panel;
            makenewTab();
            await getSelectedWebView().EnsureCoreWebView2Async();
            getSelectedWebView().CoreWebView2.ContainsFullScreenElementChanged += this.CoreWebView2_ContainsFullScreenElementChanged;
            HideDevTools();
            uri = getSelectedWebView().Source;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_process != null)
            {
                _process.Refresh();
                _process.Close();
            }
        }
        #endregion

        #region address box
        private void addressApply()
        {
            uri = getSelectedWebView().Source;
            address.Text = getSelectedWebView().Source.ToString();
        }

        private void address_KeyDown(object sender, KeyEventArgs e)
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
                            if (Regex.IsMatch(address.Text, @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$"))
                            {
                                getSelectedWebView().Source = new Uri("http://" + address.Text);
                            }
                            else
                            {
                                getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={address.Text}");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={address.Text}");
                    }
                }
                getSelectedWebView().Focus();
            }
        }

        private void address_GotFocus(object sender, RoutedEventArgs e)
        {
            address.TextAlignment = TextAlignment.Left;
            address.Text = uri.ToString();
        }

        private void address_LostFocus(object sender, RoutedEventArgs e)
        {
            address.TextAlignment = TextAlignment.Center;
            addressApply();
        }
        #endregion

        #region tab
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (tab.Items.Count <= 1)
            {
                Application.Current.Shutdown();
            }
            tab.Items.RemoveAt(tab.SelectedIndex);
        }

        private void applyIcon()
        {
            foreach (TabItem item in tab.Items)
            {
                var wv2 = ((item.Content as Grid).Children[0] as WebView2);
                if (item == tab.SelectedItem && wv2.CoreWebView2 != null)
                {
                    item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle, false, wv2.CoreWebView2.FaviconUri);
                }
                else if(wv2.CoreWebView2 != null)
                {
                    item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle, true, wv2.CoreWebView2.FaviconUri);
                }
            }
        }

        private void AutoRisizeTab()
        {

        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tab.Dispatcher.BeginInvoke(
                new Action(() => address.Text = getSelectedWebView().Source.ToString())
            );
            tab.Dispatcher.BeginInvoke(new Action(async () =>
            {
                foreach (TabItem item in tab.Items)
                {
                    await ((item.Content as Grid).Children[0] as WebView2).EnsureCoreWebView2Async();
                    item.Header = API.makeTabHeader(this, ((item.Content as Grid).Children[0] as WebView2).CoreWebView2.DocumentTitle ?? "New Tab", item != tab.SelectedItem, ((item.Content as Grid).Children[0] as WebView2).CoreWebView2.FaviconUri);
                }
            }));
            if (_process != null)
            {
                HideDevTools();
            }
            applyIcon();
        }

        private void NewTab(object sender, RoutedEventArgs e)
        {
            makenewTab();
        }

        private void makenewTab()
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
            tabitem.Header = API.makeTabHeader(this);
            tabitem.Margin = new Thickness(-2, 1, -2, -4);
            tabitem.SizeChanged += TabItem_SizeChanged;
            tab.Items.Add(tabitem);
            tab.SelectedItem = tabitem;
            NewTabBtn.IsEnabled = false;
            address.Text = webview.Source.ToString();
        }

        private void tab_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void TabItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        #endregion

        #region title
        #endregion

        #region FullScreen
        private bool fullScreen = false;
        private Grid parent;

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

        private void CoreWebView2_ContainsFullScreenElementChanged(object sender, object e)
        {
            this.FullScreen = (sender as CoreWebView2).ContainsFullScreenElement;
        }
        #endregion

        #region WebView2 event
        private async void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            string script = File.ReadAllText(@"C:\Users\GrapeApple\Desktop\script.js");
            await getSelectedWebView().CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        private async void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await (sender as WebView2).EnsureCoreWebView2Async();
            (sender as WebView2).CoreWebView2.ContextMenuRequested += delegate (object s, CoreWebView2ContextMenuRequestedEventArgs __)
            {
                IList<CoreWebView2ContextMenuItem> menuList = __.MenuItems;
                // add new item to end of collection
                CoreWebView2ContextMenuItem newItem = (sender as WebView2).CoreWebView2.Environment.CreateContextMenuItem(menuList[menuList.Count - 1].Label, null, CoreWebView2ContextMenuItemKind.Command);
                newItem.CustomItemSelected += delegate (object send, object ex)
                {
                    string pageUri = __.ContextMenuTarget.PageUri;
                    System.Threading.SynchronizationContext.Current.Post((_) =>
                    {
                        ShowDevTools();
                    }, null);
                };
                menuList.Insert(menuList.Count, newItem);
                menuList.RemoveAt(menuList.Count - 2);
            };
            ReloadBtn.Content = new MahApps.Metro.IconPacks.PackIconBootstrapIcons()
            {
                Kind = MahApps.Metro.IconPacks.PackIconBootstrapIconsKind.ArrowCounterclockwise,
                Width = 12,
                Height = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            (sender as WebView2).CoreWebView2.ContainsFullScreenElementChanged -= this.CoreWebView2_ContainsFullScreenElementChanged;
            address.Text = getSelectedWebView().Source.ToString();
            applyIcon();
            NewTabBtn.IsEnabled = true;
            await Task.Delay(500);
            var wv2 = sender as WebView2;
            var wvparent = LogicalTreeHelper.GetParent(wv2) as Grid;     
            var parent = LogicalTreeHelper.GetParent(wvparent) as TabItem;
            (parent ?? (tab.SelectedItem as TabItem)).Header = API.makeTabHeader(this, (sender as WebView2).CoreWebView2.DocumentTitle ?? "New Tab", parent != tab.SelectedItem, (sender as WebView2).CoreWebView2.FaviconUri);
            //titleChange();
        }

        private async void WebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            await (sender as WebView2).EnsureCoreWebView2Async();
        }

        private async void WebView2_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            await (sender as WebView2).EnsureCoreWebView2Async();
        }

        #endregion

        #region DevTools
        private System.Windows.Forms.Panel _panel;
        private Process _process;

        private void ResizeEmbeddedApp()
        {
            if (_process == null)
                return;

            API.SetWindowPos(_process.MainWindowHandle, IntPtr.Zero, -10, -35, (int)_panel.ClientSize.Width + 18, (int)_panel.ClientSize.Height + 43, API.SWP_NOZORDER | API.SWP_NOACTIVATE);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            ResizeEmbeddedApp();
            return size;
        }

        private async void ShowDevTools(WebView2 wv2 = null)
        {
            HideDevTools();
            if (_process != null)
            {
                _process.CloseMainWindow();
            }
            windowsFormsHost1.Visibility = Visibility.Visible;
            gridSplitter1.Visibility = Visibility.Visible;
            devClose.Visibility = Visibility.Visible;
            tab.Margin = new Thickness(-2, 30, 6, -2);
            var cd1 = new ColumnDefinition();
            cd1.Width = new GridLength(1.0, GridUnitType.Star);
            var cd2 = new ColumnDefinition();
            cd2.Width = new GridLength(5.0, GridUnitType.Pixel);
            var cd3 = new ColumnDefinition();
            cd3.MinWidth = 325;
            cd3.MaxWidth = 600;
            cd3.Width = GridLength.Auto;
            main.ColumnDefinitions.Clear();
            main.ColumnDefinitions.Add(cd1);
            main.ColumnDefinitions.Add(cd2);
            main.ColumnDefinitions.Add(cd3);
            (wv2 ?? getSelectedWebView()).CoreWebView2.OpenDevToolsWindow();
            await Task.Delay(600);
            //Get foreground window
            Process[] processes = Process.GetProcessesByName("msedgewebview2");
            foreach (Process p in processes)
            {
                if (p.MainWindowTitle.StartsWith("DevTools"))
                {
                    _process = p;
                    IntPtr windowHandle = p.MainWindowHandle;
                    API.SetWindowLong(windowHandle, API.GWL_STYLE, (int)(API.GetWindowLong(windowHandle, API.GWL_STYLE) & (0xFFFFFFFF ^ API.WS_SYSMENU)));
                    API.SetParent(p.MainWindowHandle, _panel.Handle);
                    int style = (int)API.GetWindowLong(_process.MainWindowHandle, API.GWL_STYLE);
                    style = style & ~API.WS_CAPTION & ~API.WS_THICKFRAME;
                    API.SetWindowLong(p.MainWindowHandle, API.GWL_STYLE, style);
                    ResizeEmbeddedApp();
                }
            }
            if (API.GetParent(_process.Handle) != _panel.Handle)
            {
                processes = Process.GetProcessesByName("msedgewebview2");
                foreach (Process p in processes)
                {
                    if (p.MainWindowTitle.StartsWith("DevTools"))
                    {
                        _process = p;
                        IntPtr windowHandle = p.MainWindowHandle;
                        API.SetWindowLong(windowHandle, API.GWL_STYLE, (int)(API.GetWindowLong(windowHandle, API.GWL_STYLE) & (0xFFFFFFFF ^ API.WS_SYSMENU)));
                        API.SetParent(p.MainWindowHandle, _panel.Handle);
                        int style = (int)API.GetWindowLong(_process.MainWindowHandle, API.GWL_STYLE);
                        style = style & ~API.WS_CAPTION & ~API.WS_THICKFRAME;
                        API.SetWindowLong(p.MainWindowHandle, API.GWL_STYLE, style);
                        ResizeEmbeddedApp();
                    }
                }
            }
        }

        public void HideDevTools()
        {
            if (_process != null)
            {
                _process.CloseMainWindow();
            }
            tab.Margin = new Thickness(-2, 30, -2, -1);
            main.ColumnDefinitions.Clear();
            windowsFormsHost1.Visibility = Visibility.Collapsed;
            gridSplitter1.Visibility = Visibility.Collapsed;
            devClose.Visibility = Visibility.Collapsed;
        }

        private void devClose_Click(object sender, RoutedEventArgs e)
        {
            HideDevTools();
        }

        private void windowsFormsHost1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeEmbeddedApp();
        }

        #endregion
    }
}
