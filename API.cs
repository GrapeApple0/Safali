using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
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


        public static Grid makeTabHeader(double tabwidth, MainWindow parent, string text = "New Tab", bool showFavicon = true)
        {
            //CloseButton
            var CloseButton = new Button();
            var Favicon = new Image();
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
            }
            else// Favicon
            {
                Favicon.Width = 17;
                Favicon.Height = 17;
                Favicon.HorizontalAlignment = HorizontalAlignment.Center;
                Favicon.Source = new BitmapImage(new Uri("https://www.google.com/s2/favicons?domain=https://04.si"));
                Grid.SetColumn(Favicon, 0);
            }
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
}
