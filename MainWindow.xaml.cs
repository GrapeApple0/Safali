using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Reload(object sender, RoutedEventArgs e)
        {
            getSelectedWebView().Reload();
        }

        private WebView2 getSelectedWebView()
        {
            var webview = tab.SelectedContent as WebView2;
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
                try
                {
                    getSelectedWebView().Source = new Uri(address.Text);
                }
                catch
                {
                    try
                    {
                        var domain = new Uri("http://" + address.Text).DnsSafeHost;
                        var splitHostName = domain.Split('.');
                        Debug.WriteLine(Array.IndexOf(tldlist.tld, splitHostName[splitHostName.Length - 1].ToUpper()));
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

        private async void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await getSelectedWebView().EnsureCoreWebView2Async();
            address.Text = getSelectedWebView().Source.ToString();
            selectItem().Header = makeTabHeader(getSelectedWebView().CoreWebView2.DocumentTitle);
            if (getSelectedWebView() != null)
            {
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
        }

        public void getTitle()
        {

        }

        private TabItem selectItem()
        {
            return (tab.SelectedItem as TabItem);
        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                address.Text = getSelectedWebView().Source.ToString();
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
            catch
            {

            }
        }

        private Grid makeTabHeader(string text = "New Tab")
        {
            var grid1 = new Grid();
            var headerText = new TextBlock();
            headerText.Text = text;
            headerText.Width = 105;
            headerText.Height = 18;
            headerText.Margin = new Thickness(18, 0, 0, 0);
            headerText.TextAlignment = TextAlignment.Center;

            var CloseButton = new Button();
            //CloseButton.Template = this.FindResource("RoundedButton") as ControlTemplate;
            CloseButton.Click += CloseButton_Click;
            CloseButton.MouseEnter += Button_MouseEnter;
            CloseButton.MouseLeave += Button_MouseLeave;
            CloseButton.Width = 18;
            CloseButton.Height = 18;
            CloseButton.Margin = new Thickness(0, 0, 105, 0);
            CloseButton.HorizontalAlignment = HorizontalAlignment.Center;
            CloseButton.Content = new MahApps.Metro.IconPacks.PackIconBootstrapIcons() {
                Kind = MahApps.Metro.IconPacks.PackIconBootstrapIconsKind.X,
                Width = 12,
                Height = 12,
                Background = new SolidColorBrush(Colors.White)
            };
            CloseButton.Background = null;
            CloseButton.BorderBrush = null;
            Grid.SetColumn(CloseButton, 0);
            Grid.SetColumn(headerText, 1);
            grid1.Children.Add(CloseButton);
            grid1.Children.Add(headerText);
            return grid1;
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
        Point pt;
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
            tab.Items.RemoveAt(tab.SelectedIndex);
        }

        private void NewTab(object sender, RoutedEventArgs e)
        {
            var webview = new WebView2();
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.HorizontalAlignment = HorizontalAlignment.Stretch;
            webview.VerticalAlignment = VerticalAlignment.Stretch;
            webview.Source = new Uri("https://04.si/");
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.SourceChanged += WebView2_SourceChanged;
            var tabitem = new TabItem();
            tabitem.Content = webview;
            tabitem.Width = 150;
            tabitem.Header = makeTabHeader();
            tabitem.Margin = new Thickness(-2, 3, -2, -4);
            tab.Items.Add(tabitem);

        }

        private void tabClose(object sender, RoutedEventArgs e)
        {
            tab.Items.RemoveAt(tab.SelectedIndex);
        }

        private async void WebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            await getSelectedWebView().EnsureCoreWebView2Async();
            address.Text = getSelectedWebView().Source.ToString();
            selectItem().Header = makeTabHeader(getSelectedWebView().CoreWebView2.DocumentTitle);
            if (getSelectedWebView() != null)
            {
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                Debug.WriteLine(i);
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Mouse Leave");
        }

        private async void WebView2_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            await getSelectedWebView().EnsureCoreWebView2Async();
            address.Text = getSelectedWebView().Source.ToString();
            selectItem().Header = makeTabHeader(getSelectedWebView().CoreWebView2.DocumentTitle);

            if (getSelectedWebView() != null)
            {
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
