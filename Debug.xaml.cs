using System.Windows;

namespace Safali
{
    /// <summary>
    /// Debug.xaml の相互作用ロジック
    /// </summary>
    public partial class Debug : Window
    {
        MainWindow mainWindow;
        public Debug(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void autoresize_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AutoRisizeTab();
        }

        private void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.FullScreen = (bool)fullscreen.IsChecked;
        }

        private void DevTools_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)DevTools.IsChecked)
                mainWindow.ShowDevTools();
            else
                mainWindow.HideDevTools();
        }

        private void SelText_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.address.SelectAll();
        }

        private void AlwaysTop_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Topmost = (bool)AlwaysTop.IsChecked;
        }

        private void GetTitle_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Log(mainWindow.getSelectedWebView().CoreWebView2.DocumentTitle);
        }

        private async void Runjs_Click(object sender, RoutedEventArgs e)
        {
            //document.getElementsByClassName('ytp - time - current')[0].innerText
            string result = await mainWindow.getSelectedWebView().CoreWebView2.ExecuteScriptAsync(jscode.Text);
            mainWindow.Log((result));
        }
    }
}
