using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Safali
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 変数
        public static Uri uri;
        bool isloaded = false;
        Debug debug;
        #endregion

        #region ボタン類
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
            Log("Reloading");
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

        #region ウィンドウ
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debug = new Debug(this);
            debug.Show();
            _panel = new System.Windows.Forms.Panel();
            windowsFormsHost1.Child = _panel;
            makenewTab();
            Log(getSelectedWebView().Source);
            await Task.Delay(1000);
            await getSelectedWebView().EnsureCoreWebView2Async();
            getSelectedWebView().CoreWebView2.ContainsFullScreenElementChanged += this.CoreWebView2_ContainsFullScreenElementChanged;
            HideDevTools();
            uri = getSelectedWebView().Source;
            address.TextAlignment = TextAlignment.Center;
            addressApply();
            this.Icon = BitmapFrame.Create(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Safali.icon.png"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            debug.Close();
            if (_process != null)
            {
                _process.Refresh();
                _process.Close();
            }
        }
        #endregion

        #region アドレス欄
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
            address.SelectAll();
        }

        private void address_LostFocus(object sender, RoutedEventArgs e)
        {
            address.TextAlignment = TextAlignment.Center;
            addressApply();
        }
        #endregion

        #region タブ
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
            wv2s.Children.RemoveAt(tab.SelectedIndex);
            tab.Items.RemoveAt(tab.SelectedIndex);

        }

        private void tab_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoRisizeTab();
        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tab.Dispatcher.BeginInvoke(
                new Action(() => address.Text = getSelectedWebView().Source.ToString())
            );
            tab.Dispatcher.BeginInvoke(new Action(async () =>
            {
                int i = 0;
                foreach (TabItem item in tab.Items)
                {
                    await ((wv2s.Children[i] as Grid).Children[0] as WebView2).EnsureCoreWebView2Async();
                    item.Header = API.makeTabHeader(this, ((wv2s.Children[i] as Grid).Children[0] as WebView2).CoreWebView2.DocumentTitle ?? "新しいタブ", item != tab.SelectedItem, ((wv2s.Children[i] as Grid).Children[0] as WebView2).CoreWebView2.FaviconUri, (int)(item.Width - 10));
                    i++;
                }
                AutoRisizeTab();
            }));
            try
            {
                this.Title = (((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WebView2).CoreWebView2.DocumentTitle ?? "新しいタブ") + " - Safali";
            }
            catch { }
            wv2s.Dispatcher.BeginInvoke(new Action(() =>
            {
                int i = 0;
                foreach (Grid grid in wv2s.Children)
                {
                    if (i == tab.SelectedIndex)
                    {
                        grid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid.Visibility = Visibility.Collapsed;
                    }
                    i++;
                }
            }));
            if (_process != null)
            {
                HideDevTools();
            }
            applyIcon();
            debug.Dispatcher.BeginInvoke(new Action(() => Log($"Selected tab Changed\nNow Selected:{tab.SelectedIndex}")));
        }

        public void applyIcon()
        {
            if (this.FullScreen == false)
            {
                int i = 0;
                foreach (TabItem item in tab.Items)
                {
                    var wv2 = ((wv2s.Children[i] as Grid).Children[0] as WebView2);
                    if (wv2.CoreWebView2 != null)
                    {
                        item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle, item != tab.SelectedItem, wv2.CoreWebView2.FaviconUri, (int)(item.Width - 10));
                    }
                    i++;
                }
            }

        }

        public void AutoRisizeTab()
        {
            var size = tab.ActualWidth / tab.Items.Count;
            var minsize = 200;
            if (size < minsize)
                size = minsize;
            int i = 0;
            foreach (TabItem item in tab.Items)
            {
                item.Width = size;
                (item.Header as Grid).Width = size - 10;
                i++;
            }
        }

        private void NewTab(object sender, RoutedEventArgs e)
        {
            makenewTab();
        }

        public void makenewTab(string url = "https://google.com/")
        {
            var webview = new WebView2();
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.HorizontalAlignment = HorizontalAlignment.Stretch;
            webview.VerticalAlignment = VerticalAlignment.Stretch;
            webview.Source = new Uri(url);
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.SourceChanged += WebView2_SourceChanged;
            var grid = new Grid();
            grid.Children.Add(webview);
            wv2s.Children.Add(grid);
            var tabitem = new TabItem();
            tabitem.Width = 215;
            tabitem.Header = API.makeTabHeader(this, width: (int)(tabitem.Width - 10));
            tabitem.Margin = new Thickness(-2, 1, -2, -4);
            tab.Items.Add(tabitem);
            tab.SelectedItem = tabitem;
            NewTabBtn.IsEnabled = false;
            address.Text = webview.Source.ToString();
            AutoRisizeTab();
        }
        #endregion

        #region フルスクリーン
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
                if (value)
                {
                    if (fullscreen.Children.Count <= 0)
                    {
                        this.WindowState = WindowState.Normal;
                        this.WindowStyle = WindowStyle.None;
                        this.WindowState = WindowState.Maximized;
                        fullScreenWebView = getSelectedWebView();
                        parent = (Grid)VisualTreeHelper.GetParent(getSelectedWebView());
                        parent.Children.Remove(fullScreenWebView);
                        fullscreen.Children.Add(fullScreenWebView);
                        fullscreen.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (fullscreen.Children.Count > 0)
                    {
                        this.Activate();
                        this.WindowStyle = WindowStyle.SingleBorderWindow;
                        this.WindowState = WindowState.Normal;
                        fullscreen.Children.Remove(fullScreenWebView);
                        fullscreen.Visibility = Visibility.Collapsed;
                        Log(fullscreen.Children.Count);
                        parent.Children.Add(fullScreenWebView);
                        getSelectedWebView().Focus();
                    }
                }
            }
        }

        private void CoreWebView2_ContainsFullScreenElementChanged(object sender, object e)
        {
            this.FullScreen = (sender as CoreWebView2).ContainsFullScreenElement;
        }
        #endregion

        #region WebView2のイベント
        private async void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            string script = File.ReadAllText(@"C:\Users\GrapeApple\Desktop\script.js");
            await getSelectedWebView().CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        private async void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Log("Navigation Completed");
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
            (sender as WebView2).CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;
            (sender as WebView2).CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            if (sender as WebView2 == getSelectedWebView())
                address.Text = (sender as WebView2).Source.ToString();
            applyIcon();
            NewTabBtn.IsEnabled = true;
            isloaded = true;
            await Task.Delay(500);
            var wv2 = sender as WebView2;
            var wvparent = LogicalTreeHelper.GetParent(wv2) as Grid;
            var parent = LogicalTreeHelper.GetParent(wvparent) as TabItem;
            var title = (sender as WebView2).CoreWebView2.DocumentTitle;
            if (title == "")
                title = (sender as WebView2).Source.ToString();
            (parent ?? (tab.SelectedItem as TabItem)).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), (sender as WebView2).CoreWebView2.FaviconUri);
            if ((sender as WebView2) == getSelectedWebView())
            {
                this.Title = ((sender as WebView2).CoreWebView2.DocumentTitle ?? "新しいタブ") + " - Safali";
            }
            AutoRisizeTab();
        }

        private void WebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            Log("Source Changed");
            if (sender as WebView2 == getSelectedWebView())
            {
                isloaded = false;
                var wv2 = sender as WebView2;
                var wvparent = LogicalTreeHelper.GetParent(wv2) as Grid;
                var parent = LogicalTreeHelper.GetParent(wvparent) as TabItem;
                var title = (sender as WebView2).CoreWebView2.DocumentTitle;
                if (title == "")
                    title = (sender as WebView2).Source.ToString();
                (parent ?? (tab.SelectedItem as TabItem)).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), (sender as WebView2).CoreWebView2.FaviconUri);
                this.Title = title + " - Safali";
                address.Text = (sender as WebView2).Source.ToString();
            }
        }

        private void WebView2_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            Log("Source Updated");
            if (sender as WebView2 == getSelectedWebView())
            {
                address.Text = (sender as WebView2).Source.ToString();
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (!isloaded)
            {
                makenewTab(e.Uri);
            }
            isloaded = false;
            e.Handled = true;
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
            await Task.Delay(1500);
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
                    int style = API.GetWindowLong(_process.MainWindowHandle, API.GWL_STYLE);
                    style = style & ~API.WS_CAPTION & ~API.WS_THICKFRAME;
                    API.SetWindowLong(p.MainWindowHandle, API.GWL_STYLE, style);
                    ResizeEmbeddedApp();
                    Log("Set DevTools");
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

        #region その他色々
        private void Log(object content)
        {
            debug.log.Text += content.ToString() + "\n";
            debug.log.ScrollToEnd();
        }

        private WebView2 getSelectedWebView()
        {
            var grid = ((Grid)wv2s.Children[tab.SelectedIndex]);
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
        #endregion
    }
}
