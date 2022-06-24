using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            getSelectedWebView().Reload();
        }

        private WebView2 getSelectedWebView()
        {
            var webview = tab.SelectedContent as WebView2;
            return webview;
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

        private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            address.Text = getSelectedWebView().Source.ToString();
            //(tab.SelectedItem as TabItem).Header = getSelectedWebView().CoreWebView2.DocumentTitle;
            //(tab.SelectedItem as TabItem).Header = (tab.Items[0] as TabItem).Header;
            (tab.SelectedItem as TabItem).Header = makeTabHeader();
            if (getSelectedWebView() != null)
            {
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            address.Text = getSelectedWebView().Source.ToString();
            try
            {
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
            catch
            {

            }
        }

        private Grid makeTabHeader()
        {
            var grid1 = new Grid();
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star)});
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            var headerText = new TextBlock();
            headerText.Text = "000";
            var CloseButton = new Button();
            # region Path
            var path = new FrameworkElementFactory(typeof(Path));

            //Path.Style
            var pathStyle = new Style();
            pathStyle.TargetType = typeof(Path);
            //Style.Triggers.Trigger
            var trigger1 = new Trigger()
            {
                Property = IsMouseOverProperty,
                Value = false
            };
            var trigger2 = new Trigger()
            {
                Property = IsMouseOverProperty,
                Value = true
            };

            //Trigger.Setter
            var setter1 = new Setter()
            {
                Property = Shape.StrokeProperty,
                Value = new SolidColorBrush(Colors.LightGray)
            };
            var setter2 = new Setter()
            {
                Property = Shape.StrokeProperty,
                Value = new SolidColorBrush(Colors.Black)
            };

            //Add Setter & Trigger
            trigger1.Setters.Add(setter1);
            trigger2.Setters.Add(setter2);
            pathStyle.Triggers.Add(trigger1);
            pathStyle.Triggers.Add(trigger2);

            //Set Style
            path.SetValue(Path.DataProperty, Geometry.Parse("M0,0 L8,8 M8,0 L0,8"));
            path.SetValue(StyleProperty, pathStyle);
            path.SetValue(Path.StrokeThicknessProperty, (double)3);
            path.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            path.SetValue(Path.MarginProperty, new Thickness(5, 4, 0, 2));
            #endregion
            var ctrlTemplate = new ControlTemplate();
            ctrlTemplate.TargetType = typeof(Button);
            ctrlTemplate.VisualTree = path;
            CloseButton.Template = ctrlTemplate;
            CloseButton.Click += CloseButton_Click;
            Grid.SetColumn(headerText, 0);
            Grid.SetColumn(CloseButton, 1);
            grid1.Children.Add(headerText);
            grid1.Children.Add(CloseButton);
            return grid1;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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
            Debug.WriteLine(((tab.SelectedItem as TabItem).Header as Grid).ColumnDefinitions[0].Width);
            tab.Items.Add(tabitem);
        }

        private void tabClose(object sender, RoutedEventArgs e)
        {
            tab.Items.RemoveAt(tab.SelectedIndex);
        }

        private void WebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            address.Text = getSelectedWebView().Source.ToString();
            //(tab.SelectedItem as TabItem).Header = getSelectedWebView().CoreWebView2.DocumentTitle;
            (tab.SelectedItem as TabItem).Header = makeTabHeader();
            if (getSelectedWebView() != null)
            {
                this.Title = getSelectedWebView().CoreWebView2.DocumentTitle + " - Safali";
            }
        }
    }
}
