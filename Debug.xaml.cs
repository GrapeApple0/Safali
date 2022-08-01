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
    }
}
