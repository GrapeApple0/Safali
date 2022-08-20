using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System.Linq;
using System.Net;

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
        ScrollViewer scv;
        List<bool> isShowDevTools = new List<bool> { };
        private DispatcherTimer YouTubeCurrentTimeWatcher;
        List<int> YouTubeCurrentTime = new List<int> { };
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
        private async void Window_Loaded(object s, RoutedEventArgs e)
        {
            debug = new Debug(this);
            makenewTab("https://freasearch.org/");
            ((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Child = (((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Child as System.Windows.Forms.Panel);
            await Task.Delay(1000);
            await getSelectedWebView().EnsureCoreWebView2Async();
            getSelectedWebView().CoreWebView2.ContainsFullScreenElementChanged += this.CoreWebView2_ContainsFullScreenElementChanged;
            HideDevTools();
            uri = getSelectedWebView().Source;
            address.TextAlignment = TextAlignment.Center;
            addressApply();
            this.Icon = BitmapFrame.Create(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Safali.icon.png"));
            YouTubeCurrentTimeWatcher = new DispatcherTimer(DispatcherPriority.Background);
            YouTubeCurrentTimeWatcher.Interval = new TimeSpan(0, 0, 1);
            YouTubeCurrentTimeWatcher.Tick += async (sender, eve) =>
            {
                if (getSelectedWebView().Source.ToString().StartsWith("https://www.youtube.com/watch?v="))
                {
                    var time = double.Parse(await getSelectedWebView().CoreWebView2.ExecuteScriptAsync("document.getElementsByClassName('video-stream')[0].currentTime"));
                    YouTubeCurrentTime[tab.SelectedIndex] = Convert.ToInt32(time);
                    Log(YouTubeCurrentTime[tab.SelectedIndex]);
                }
            };
            YouTubeCurrentTimeWatcher.Start();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Log("Window Activated");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Log("Window Deactivated");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            debug.Close();
            YouTubeCurrentTimeWatcher.Stop();
            if (_process != null)
            {
                foreach (var item in _process)
                {
                    if (item != null)
                    {
                        item.Refresh();
                        item.Close();
                    }
                }
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
                            getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={HttpUtility.UrlEncode(address.Text)}");
                        }
                        else
                        {
                            if (Regex.IsMatch(address.Text, @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$"))
                            {
                                getSelectedWebView().Source = new Uri("http://" + address.Text);
                            }
                            else
                            {
                                getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={HttpUtility.UrlEncode(address.Text)}");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        getSelectedWebView().Source = new Uri($"https://www.google.com/search?q={HttpUtility.UrlEncode(address.Text)}");
                    }
                }
                getSelectedWebView().Focus();
            }
        }

        private void address_GotFocus(object sender, RoutedEventArgs e)
        {
            address.TextAlignment = TextAlignment.Left;
        }

        private void address_LostFocus(object sender, RoutedEventArgs e)
        {
            address.TextAlignment = TextAlignment.Center;
            addressApply();
        }

        private void address_PreviewDrop(object sender, DragEventArgs e)
        {
            Log("address_PreviewDrop");
            address.Text = "";
            address.SelectAll();
        }

        private void address_PreviewDragEnter(object sender, DragEventArgs e)
        {
            Log("address_PreviewDragEnter");
            address.SelectAll();
        }

        private void address_PreviewDragOver(object sender, DragEventArgs e)
        {
            Log("address_PreviewDragOver");
            address.SelectAll();
            e.Handled = true;
        }
        #endregion

        #region タブ
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void srlvew_Initialized(object sender, EventArgs e)
        {
            scv = (sender as ScrollViewer);
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            closeTab();
        }

        private void tab_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoRisizeTab();
        }

        WebClient downloadClient = null;
        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if (_process != null)
                HideDevTools();
            if (tab.SelectedIndex >= 0 && isShowDevTools[tab.SelectedIndex])
                ShowDevTools();
            */
            if (tab.Items.Count == 1)
            {
                tab.Visibility = Visibility.Collapsed;
                wv2s.Margin = new Thickness(0, 24, 0, 0);
            }
            else
            {
                tab.Visibility = Visibility.Visible;
                wv2s.Margin = new Thickness(0, 54, 0, 0);
            }
            tab.Dispatcher.BeginInvoke(new Action(async () =>
            {
                int i = 0;
                foreach (TabItem item in tab.Items)
                {
                    await ((wv2s.Children[i] as Grid).Children[2] as WebView2).EnsureCoreWebView2Async();
                    WebView2 wv2 = (wv2s.Children[i] as Grid).Children[2] as WebView2;
                    if (wv2.CoreWebView2.FaviconUri.ToLower().EndsWith(".svg"))
                    {
                        WpfDrawingSettings settings = new WpfDrawingSettings();
                        settings.IncludeRuntime = true;
                        settings.TextAsGeometry = false;
                        string svgTestFile = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.svg";
                        WebClient wc = new WebClient();
                        wc.DownloadFile(wv2.CoreWebView2.FaviconUri, svgTestFile);
                        wc.Dispose();
                        ImageSvgConverter converter = new ImageSvgConverter(settings);
                        converter.EncoderType = ImageEncoderType.PngBitmap;
                        converter.Convert(svgTestFile, $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.png");
                        converter.Dispose();
                        var rndstr = RandomString(12);

                        File.Copy($@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.png", Path.GetTempPath() + rndstr + ".png");
                        item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle ?? "新しいタブ", item != tab.SelectedItem, $@"{Path.GetTempPath() + rndstr + ".png"}", (int)(item.Width), wv2.Source.ToString());
                    }
                    else
                    {
                        item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle ?? "新しいタブ", item != tab.SelectedItem, wv2.CoreWebView2.FaviconUri, (int)(item.Width), wv2.Source.ToString());
                    }
                    i++;
                }
                AutoRisizeTab();
            }));
            try
            {
                this.Title = (((wv2s.Children[tab.SelectedIndex] as Grid).Children[2] as WebView2).CoreWebView2.DocumentTitle ?? "新しいタブ") + " - Safali";
                address.Text = ((wv2s.Children[tab.SelectedIndex] as Grid).Children[2] as WebView2).Source.ToString();
            }
            catch { }

            wv2s.Dispatcher.BeginInvoke(new Action(() =>
            {
                int i = 0;
                foreach (Grid grid in wv2s.Children)
                {
                    if (i == tab.SelectedIndex)
                        grid.Visibility = Visibility.Visible;
                    else
                        grid.Visibility = Visibility.Collapsed;
                    i++;
                }
            }));
            applyIcon();
            tabclosing = false;
        }

        public void applyIcon()
        {
            if (this.FullScreen == false)
            {
                int i = 0;
                foreach (TabItem item in tab.Items)
                {
                    var wv2 = ((wv2s.Children[i] as Grid).Children[2] as WebView2);
                    if (wv2 != null && wv2.CoreWebView2 != null)
                    {
                        try
                        {
                            if (wv2.CoreWebView2.FaviconUri.ToLower().EndsWith(".svg"))
                            {
                                WpfDrawingSettings settings = new WpfDrawingSettings();
                                settings.IncludeRuntime = true;
                                settings.TextAsGeometry = false;
                                string svgTestFile = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.svg";
                                System.Net.WebClient wc = new System.Net.WebClient();
                                wc.DownloadFile(wv2.CoreWebView2.FaviconUri, svgTestFile);
                                wc.Dispose();
                                File.Delete($@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.png");
                                ImageSvgConverter converter = new ImageSvgConverter(settings);
                                converter.EncoderType = ImageEncoderType.PngBitmap;
                                converter.Convert(svgTestFile, $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.png");
                                converter.Dispose();
                                var rndstr = RandomString(12);
                                File.Copy($@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{wv2.Source.DnsSafeHost}.png", Path.GetTempPath() + rndstr + ".png");
                                item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle, item != tab.SelectedItem, $@"{Path.GetTempPath() + rndstr + ".png"}", (int)(item.Width), wv2.Source.ToString());
                            }
                            else
                            {
                                item.Header = API.makeTabHeader(this, wv2.CoreWebView2.DocumentTitle, item != tab.SelectedItem, wv2.CoreWebView2.FaviconUri, (int)(item.Width), wv2.Source.ToString());
                            }
                        }
                        catch { }
                    }
                    i++;
                }
            }
        }

        public void AutoRisizeTab()
        {
            if (1 <= tab.Items.Count)
            {
                var size = tab.ActualWidth / tab.Items.Count;
                var minsize = 200;
                if (size < minsize)
                    size = minsize;
                size -= 5 / tab.Items.Count;
                int i = 0;
                tab.Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (TabItem item in tab.Items)
                    {
                        item.Width = size;
                        item.Margin = new Thickness(0, 1, 1, -4);
                        (item.Header as Grid).Width = size - 10;
                        i++;
                    }
                    (tab.Items[tab.Items.Count - 1] as TabItem).Margin = new Thickness(-3, 1, -3, -4);
                }));
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
            webview.KeyDown += WebView2_KeyDown;
            Panel.SetZIndex(webview, 1);
            Grid.SetColumn(webview, 0);
            var grid = new Grid();
            grid.Visibility = Visibility.Hidden;
            WindowsFormsHost wfh = new WindowsFormsHost();
            Grid.SetColumn(wfh, 2);
            wfh.Visibility = Visibility.Collapsed;
            wfh.SizeChanged += windowsFormsHost1_SizeChanged;
            wfh.Child = new System.Windows.Forms.Panel();
            wfh.HorizontalAlignment = HorizontalAlignment.Stretch;
            GridSplitter gridSplitter = new GridSplitter();
            Grid.SetColumn(gridSplitter, 1);
            gridSplitter.Visibility = Visibility.Collapsed;
            gridSplitter.Width = 5;
            gridSplitter.Margin = new Thickness(0, 0, 0, 2);
            gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.Children.Add(wfh);
            grid.Children.Add(gridSplitter);
            grid.Children.Add(webview);
            wv2s.Children.Add(grid);
            var tabitem = new TabItem();
            var size = tab.ActualWidth / tab.Items.Count;
            if (tab.Items.Count == 0)
                size = tab.ActualWidth;
            var minsize = 215;
            if (size < minsize)
                size = minsize;
            tabitem.Width = size;
            tabitem.Header = API.makeTabHeader(this, width: (int)(tabitem.Width), url: url);
            tabitem.Margin = new Thickness(0, 1, 1, -4);
            isShowDevTools.Add(false);
            tab.Items.Add(tabitem);
            _process.Add(null);
            YouTubeCurrentTime.Add(0);
            tab.SelectedItem = tabitem;
            NewTabBtn.IsEnabled = false;
            address.Text = webview.Source.ToString();
            scv.ScrollToRightEnd();
            AutoRisizeTab();
            grid.Visibility = Visibility.Visible;
        }

        bool tabclosing = false;
        public void closeTab()
        {
            if (wv2s.Children.Count <= 1)
            {
                Application.Current.Shutdown();
                return;
            }
            try
            {
                Log(tab.SelectedIndex);
                int delindex = tab.SelectedIndex;
                getSelectedWebView().Dispose();
                (wv2s.Children[delindex] as Grid).Children.Clear();
                wv2s.Children.RemoveAt(delindex);
                tab.Items.RemoveAt(delindex);
                isShowDevTools.RemoveAt(delindex);
                _process[delindex].Refresh();
                _process[delindex].Close();
                _process.RemoveAt(delindex);
                tabclosing = true;
                tab.SelectedIndex = tab.Items.Count - delindex;
            }
            catch { }
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
                        //isShowDevTools[tab.SelectedIndex] = true;
                        parent = (Grid)LogicalTreeHelper.GetParent(getSelectedWebView());
                        parent.Children.RemoveAt(2);
                        fullscreen.Children.Add(fullScreenWebView);
                        fullscreen.Visibility = Visibility.Visible;
                        fullscreen.Children[0].Focus();
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
                        UIElementCollection childui = parent.Children;
                        parent.Children.Add(fullScreenWebView);
                        fullScreenWebView.Focus();
                        AutoRisizeTab();
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
        private async void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Log("Navigation Completed");
            await (sender as WebView2).EnsureCoreWebView2Async();
            (sender as WebView2).CoreWebView2.ContextMenuRequested += delegate (object s, CoreWebView2ContextMenuRequestedEventArgs __)
            {
                IList<CoreWebView2ContextMenuItem> menuList = __.MenuItems;
                CoreWebView2ContextMenuItem newItem = (sender as WebView2).CoreWebView2.Environment.CreateContextMenuItem(menuList[menuList.Count - 1].Label, null, CoreWebView2ContextMenuItemKind.Command);
                newItem.CustomItemSelected += delegate (object send, object ex)
                {
                    System.Threading.SynchronizationContext.Current.Post((_) =>
                    {
                        ShowDevTools();
                    }, null);
                };
                menuList.Insert(menuList.Count, newItem);
                Log(menuList[menuList.Count - 2].ShortcutKeyDescription);
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
            (sender as WebView2).CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            (sender as WebView2).CoreWebView2.FaviconChanged += CoreWebView2_FaviconChanged;
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
            var size = tab.ActualWidth / tab.Items.Count;
            var minsize = 215;
            if (size < minsize)
                size = minsize;
            Log((sender as WebView2).CoreWebView2.FaviconUri);

            if ((sender as WebView2).CoreWebView2.FaviconUri.ToLower().EndsWith(".svg"))
            {
                WpfDrawingSettings settings = new WpfDrawingSettings();
                settings.IncludeRuntime = true;
                settings.TextAsGeometry = false;
                string svgTestFile = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{(sender as WebView2).Source.DnsSafeHost}.svg";
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.DownloadFile((sender as WebView2).CoreWebView2.FaviconUri, svgTestFile);
                wc.Dispose();
                //File.Delete($@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{(sender as WebView2).Source.DnsSafeHost}.png");
                ImageSvgConverter converter = new ImageSvgConverter(settings);
                converter.EncoderType = ImageEncoderType.PngBitmap;
                converter.Convert(svgTestFile, $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{(sender as WebView2).Source.DnsSafeHost}.png");
                converter.Dispose();
                var rndstr = RandomString(12);
                File.Copy($@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\{(sender as WebView2).Source.DnsSafeHost}.png", Path.GetTempPath() + rndstr + ".png");
                (tab.SelectedItem as TabItem).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), $@"{Path.GetTempPath() + rndstr + ".png"}", (int)size, (sender as WebView2).Source.ToString());
            }
            else
            {
                /*await API.DownloadFileAsync((sender as WebView2).CoreWebView2.FaviconUri, Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + (sender as WebView2).Source.DnsSafeHost + @"." + Path.GetExtension((sender as WebView2).CoreWebView2.FaviconUri));
                var rndstr = RandomString(12);
                File.Copy(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + (sender as WebView2).Source.DnsSafeHost + @"." + Path.GetExtension((sender as WebView2).CoreWebView2.FaviconUri), Path.GetTempPath() + rndstr + Path.GetExtension((sender as WebView2).CoreWebView2.FaviconUri));
                Log(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + (sender as WebView2).Source.DnsSafeHost + @"\" + Path.GetExtension((sender as WebView2).CoreWebView2.FaviconUri));
                await (tab.SelectedItem as TabItem).Dispatcher.BeginInvoke(new Action(() =>
                {
                    (tab.SelectedItem as TabItem).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), Path.GetTempPath() + rndstr + Path.GetExtension((sender as WebView2).CoreWebView2.FaviconUri), (int)size, (sender as WebView2).Source.ToString());
                }));
                */
                (tab.SelectedItem as TabItem).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), ((sender as WebView2).CoreWebView2.FaviconUri), (int)size, (sender as WebView2).Source.ToString());
            }
            if ((sender as WebView2) == getSelectedWebView())
                this.Title = ((sender as WebView2).CoreWebView2.DocumentTitle ?? "新しいタブ") + " - Safali";
            AutoRisizeTab();

        }

        private async void WebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            Log("Source Changed");
            isloaded = false;
            var wv2 = sender as WebView2;
            var wvparent = LogicalTreeHelper.GetParent(wv2) as Grid;
            var parent = LogicalTreeHelper.GetParent(wvparent) as TabItem;
            var title = getSelectedWebView().CoreWebView2.DocumentTitle;
            await getSelectedWebView().EnsureCoreWebView2Async();
            Log(title);
            if (title == "")
                title = getSelectedWebView().Source.ToString();
            var size = tab.ActualWidth / tab.Items.Count;
            var minsize = 215;
            if (size < minsize)
                size = minsize; (parent ?? (tab.SelectedItem as TabItem)).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), (sender as WebView2).CoreWebView2.FaviconUri, (int)size, (sender as WebView2).Source.ToString());
            this.Title = title + " - Safali";
            address.Text = (sender as WebView2).Source.ToString();
        }

        private void WebView2_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Log("Source Updated");
            isloaded = false;
            var wv2 = sender as WebView2;
            var wvparent = LogicalTreeHelper.GetParent(wv2) as Grid;
            var parent = LogicalTreeHelper.GetParent(wvparent) as TabItem;
            var title = (sender as WebView2).CoreWebView2.DocumentTitle;
            Log(title);
            if (title == "")
                title = (sender as WebView2).Source.ToString();
            var size = tab.ActualWidth / tab.Items.Count;
            var minsize = 215;
            if (size < minsize)
                size = minsize; (parent ?? (tab.SelectedItem as TabItem)).Header = API.makeTabHeader(this, title, (sender as WebView2) != getSelectedWebView(), (sender as WebView2).CoreWebView2.FaviconUri, (int)size, (sender as WebView2).Source.ToString());
            this.Title = title + " - Safali";
            address.Text = (sender as WebView2).Source.ToString();
        }

        private void WebView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 || e.Key == Key.I && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                isShowDevTools[tab.SelectedIndex] = true;
                ShowDevTools();
            }
            else if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                debug.Show();
                Log("Showed DebugWindow");
            }
            else if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                closeTab();
            }
            else if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                makenewTab();
            }
        }
        
        private async void CoreWebView2_FaviconChanged(object sender, object e)
        {
            var cwv2 = (sender as CoreWebView2);
            string value = cwv2.FaviconUri;
            Log(value);
            Stream stream = await cwv2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
            if (stream == null || stream.Length == 0)
                this.Icon = null;
            else
                this.Icon = BitmapFrame.Create(stream);
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            Log("Title Changed");
            var title = getSelectedWebView().CoreWebView2.DocumentTitle;
            var size = tab.ActualWidth / tab.Items.Count;
            var minsize = 215;
            if (size < minsize)
                size = minsize;
            (tab.SelectedItem as TabItem).Header = API.makeTabHeader(this, title, (sender as CoreWebView2) != getSelectedWebView().CoreWebView2, getSelectedWebView().CoreWebView2.FaviconUri, (int)size, getSelectedWebView().Source.ToString());
            this.Title = (title ?? "新しいタブ") + " - Safali";
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (isloaded)
            {
                makenewTab(e.Uri);
            }
            isloaded = false;
            e.Handled = true;
        }

        #endregion

        #region DevTools
        private List<Process> _process = new List<Process> { };

        private void ResizeEmbeddedApp()
        {
            if (_process.Count <= 0 || _process[tab.SelectedIndex] == null)
                return;
            API.SetWindowPos(_process[tab.SelectedIndex].MainWindowHandle, IntPtr.Zero, -10, -35, (((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Child as System.Windows.Forms.Panel).ClientSize.Width + 18, (int)(((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Child as System.Windows.Forms.Panel).ClientSize.Height + 43, API.SWP_NOZORDER | API.SWP_NOACTIVATE);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            ResizeEmbeddedApp();

            return size;
        }

        public async void ShowDevTools(WebView2 wv2 = null)
        {
            ((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Visibility = Visibility.Visible;
            ((wv2s.Children[tab.SelectedIndex] as Grid).Children[1] as GridSplitter).Visibility = Visibility.Visible;
            devClose.Visibility = Visibility.Visible;
            var grid = LogicalTreeHelper.GetParent((wv2 ?? getSelectedWebView())) as Grid;
            var cd1 = new ColumnDefinition();
            cd1.Width = new GridLength(1.0, GridUnitType.Star);
            var cd2 = new ColumnDefinition();
            cd2.Width = new GridLength(5.0, GridUnitType.Pixel);
            var cd3 = new ColumnDefinition();
            cd3.MinWidth = 325;
            cd3.MaxWidth = 600;
            cd3.Width = GridLength.Auto;
            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.Add(cd1);
            grid.ColumnDefinitions.Add(cd2);
            grid.ColumnDefinitions.Add(cd3);
            (wv2 ?? getSelectedWebView()).CoreWebView2.OpenDevToolsWindow();
            if (wv2 == null)
                isShowDevTools[tab.SelectedIndex] = true;
            await Task.Delay(1000);
            //Get foreground window
            Process[] processes = Process.GetProcessesByName("msedgewebview2");
            foreach (Process p in processes)
            {
                if (p.MainWindowTitle.StartsWith("DevTools"))
                {
                    _process[tab.SelectedIndex] = p;
                    IntPtr windowHandle = p.MainWindowHandle;
                    ((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Margin = new Thickness(0, 20, 0, 0);
                    API.SetWindowLong(windowHandle, API.GWL_STYLE, (int)(API.GetWindowLong(windowHandle, API.GWL_STYLE) & (0xFFFFFFFF ^ API.WS_SYSMENU)));
                    API.SetParent(p.MainWindowHandle, (((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Child as System.Windows.Forms.Panel).Handle);
                    int style = API.GetWindowLong(_process[tab.SelectedIndex].MainWindowHandle, API.GWL_STYLE);
                    style = style & ~API.WS_CAPTION & ~API.WS_THICKFRAME;
                    API.SetWindowLong(p.MainWindowHandle, API.GWL_STYLE, style);
                    ResizeEmbeddedApp();
                    Log("Show DevTools");
                }
            }
        }

        public void HideDevTools()
        {
            System.Diagnostics.Debug.WriteLine(_process[tab.SelectedIndex]);
            if (_process[tab.SelectedIndex] != null)
                _process[tab.SelectedIndex].CloseMainWindow();
            var grid = LogicalTreeHelper.GetParent((getSelectedWebView())) as Grid;
            grid.ColumnDefinitions.Clear();
            ((wv2s.Children[tab.SelectedIndex] as Grid).Children[0] as WindowsFormsHost).Visibility = Visibility.Collapsed;
            ((wv2s.Children[tab.SelectedIndex] as Grid).Children[1] as GridSplitter).Visibility = Visibility.Collapsed;
            devClose.Visibility = Visibility.Collapsed;
        }

        private void devClose_Click(object sender, RoutedEventArgs e)
        {
            isShowDevTools[tab.SelectedIndex] = false;
            HideDevTools();
        }

        private void windowsFormsHost1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tab.Width = wv2s.Width;
            AutoRisizeTab();
            ResizeEmbeddedApp();
        }

        #endregion

        #region その他色々
        public void Log(object content = null,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerLineNumber] int callerLineNumber = 0)
        {
            if (debug != null)
            {
                debug.log.Text += content.ToString() + "\n";
                debug.log.Text += ($"呼び出し元: {callerMemberName}\n");
                debug.log.Text += ($"呼び出し元ファイル行番号: {callerLineNumber}\n\n");
                debug.log.ScrollToEnd();
            }
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public WebView2 getSelectedWebView()
        {
            if (tab.Items.Count == wv2s.Children.Count)
            {
                var grid = ((Grid)wv2s.Children[tab.SelectedIndex]);
                WebView2 webview = null;
                if (grid.Children.Count != 0)
                    webview = grid.Children[2] as WebView2;
                else if (fullscreen.Children.Count != 0)
                    webview = fullscreen.Children[0] as WebView2;
                return webview;
            }
            else
            {
                return null;
            }
        }

        public IntPtr Handle
        {
            get
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                return helper.Handle;
            }
        }
        #endregion
    }
}
