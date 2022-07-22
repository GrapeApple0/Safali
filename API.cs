using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Safali
{
    public class API
    {
        public const int WS_BORDER = 8388608;
        public const int WS_DLGFRAME = 4194304;
        public const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        public const int WS_SYSMENU = 524288;
        public const int WS_THICKFRAME = 262144;
        public const int WS_MINIMIZE = 536870912;
        public const int WS_MAXIMIZEBOX = 65536;
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x1;
        public const int SWP_NOMOVE = 0x2;
        public const int SWP_NOSIZE = 0x1;
        public const int SWP_FRAMECHANGED = 0x20;
        public const uint MF_BYPOSITION = 0x400;
        public const uint MF_REMOVE = 0x1000;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32")]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        public static void MakeExternalWindowBorderless(IntPtr MainWindowHandle)
        {
            int Style = 0;
            Style = GetWindowLong(MainWindowHandle, GWL_STYLE);
            Style = Style & ~WS_CAPTION;
            Style = Style & ~WS_SYSMENU;
            Style = Style & ~WS_THICKFRAME;
            Style = Style & ~WS_MINIMIZE;
            Style = Style & ~WS_MAXIMIZEBOX;
            SetWindowLong(MainWindowHandle, GWL_STYLE, Style);
            Style = GetWindowLong(MainWindowHandle, GWL_EXSTYLE);
            SetWindowLong(MainWindowHandle, GWL_EXSTYLE, Style | WS_EX_DLGMODALFRAME);
            SetWindowPos(MainWindowHandle, new IntPtr(0), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
        }

        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void ChangeWindowStatus(IntPtr hwnd, int windowStatus)
        {
            ShowWindow(hwnd, windowStatus);
        }

        private void RemoveClickEvent(object b)
        {
            FieldInfo f1 = typeof(Control).GetField("EventClick",
                BindingFlags.Static | BindingFlags.NonPublic);
            object obj = f1.GetValue(b);
            PropertyInfo pi = b.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(b, null);
            list.RemoveHandler(obj, list[obj]);
        }

        public static void Log(object content)
        {
            Debug.WriteLine(content);
        }

        public static Grid makeTabHeader(double tabwidth, MainWindow parent, string text = "New Tab", bool showFavicon = true, string faviconUrl = null)
        {
            //CloseButton
            var CloseButton = new Button();
            var Favicon = new Image();
            InlineUIContainer inlineUIContainer = new InlineUIContainer();
            if (!showFavicon)
            {
                CloseButton.Click += parent.CloseButton_Click;
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

                Favicon.Width = 14;
                Favicon.HorizontalAlignment = HorizontalAlignment.Center;
                Favicon.VerticalAlignment = VerticalAlignment.Center;
                Favicon.Margin = new Thickness(0, 0, 0, -2);
                if (faviconUrl == null)
                {
                    Favicon.Source = new BitmapImage(new Uri(@"D:\Downloads\star-fill.png"));
                }
                else
                {
                    if (faviconUrl == "")
                    {
                        Favicon.Source = new BitmapImage(new Uri(@"D:\Downloads\star-fill.png"));
                    }
                    else
                    {
                        Favicon.Source = new BitmapImage(new Uri(faviconUrl));

                    }
                }
                inlineUIContainer.Child = Favicon;
            }
            else// Favicon
            {
                Favicon.Width = 17;
                Favicon.Height = 17;
                Favicon.HorizontalAlignment = HorizontalAlignment.Center;
                Favicon.VerticalAlignment = VerticalAlignment.Center;
                if (faviconUrl == null)
                {
                    Favicon.Source = new BitmapImage(new Uri(@"D:\Downloads\star-fill.png"));
                }
                else
                {
                    Favicon.Source = new BitmapImage(new Uri(faviconUrl));
                }
                Grid.SetColumn(Favicon, 0);
            }
            //HeaderText
            var headerText = new TextBlock();
            headerText.Inlines.Add(inlineUIContainer);
            Run run = new Run(text);
            headerText.Inlines.Add(run);
            headerText.Height = 18;
            headerText.Margin = new Thickness(24, 0, 24, 0);
            headerText.TextAlignment = TextAlignment.Center;
            Grid.SetColumn(headerText, 1);
            //Grid
            var grid1 = new Grid();
            grid1.MinWidth = 205;
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(17, GridUnitType.Pixel);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.MinWidth = 190;
            grid1.ColumnDefinitions.Add(c1);
            grid1.ColumnDefinitions.Add(c2);
            if (!showFavicon)
            {
                grid1.Children.Add(CloseButton);
            }
            else
            {
                grid1.Children.Add(Favicon);
            }
            grid1.Children.Add(headerText);
            return grid1;
        }
    }

    public class MinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) - 15.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
