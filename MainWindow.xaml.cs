using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Nager.PublicSuffix;

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
                    var domainParser = new DomainParser(new WebTldRuleProvider());
                    try
                    {
                        var domainInfo = domainParser.Parse(address.Text);
                        var splitHostName = domainInfo.TLD.Split('.');
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
            (tab.SelectedItem as TabItem).Header = getSelectedWebView().CoreWebView2.DocumentTitle;
        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            address.Text = getSelectedWebView().Source.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var webview = new WebView2();
            webview.NavigationCompleted += WebView2_NavigationCompleted;
            webview.HorizontalAlignment = HorizontalAlignment.Stretch;
            webview.VerticalAlignment = VerticalAlignment.Stretch;
            webview.Source = new Uri("https://04.si/");
            tab.Items.Add(webview);
        }

        private void tab_close_Click(object sender, RoutedEventArgs e)
        {
            tab.Items.RemoveAt(tab.SelectedIndex);
        }
    }
}
