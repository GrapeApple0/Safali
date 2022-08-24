using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Safali
{
    public class API
    {
        #region Win32API
        #region Define
        public const int WS_BORDER = 8388608;
        public const int WS_DLGFRAME = 4194304;
        public const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        public const int WS_SYSMENU = 524288;
        public const int WS_THICKFRAME = 262144;
        public const int WS_MINIMIZE = 536870912;
        public const int WS_MAXIMIZEBOX = 65536;
        public const int WS_MINIMIZEBOX = 0x20000; //minimize button
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x1;
        public const int SWP_NOMOVE = 0x2;
        public const int SWP_NOSIZE = 0x1;
        public const int SWP_FRAMECHANGED = 0x20;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_REMOVE = 0x1000;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int HWND_TOP = 0;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;
        public const int SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const int SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32")]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(
            IntPtr hWnd, IntPtr ProcessId);
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AttachThreadInput(
            uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfoGet(
            uint action, uint param, ref uint vparam, uint init);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfoSet(
            uint action, uint param, uint vparam, uint init);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd,
            int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        #endregion

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        public static async Task DownloadFileAsync(string dlUrl, string dlPath)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(dlUrl);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return;
            if (File.Exists(dlPath))
                    File.WriteAllText(dlPath, "");
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var outStream = File.Create(dlPath))
                {
                    stream.CopyTo(outStream);
                }
            }
        }

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

        public static void ActiveWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }
            IntPtr forehWnd = GetForegroundWindow();
            if (forehWnd == hWnd)
            {
                return;
            }
            uint foreThread = GetWindowThreadProcessId(forehWnd, IntPtr.Zero);
            uint thisThread = GetCurrentThreadId();
            uint timeout = 200000;
            if (foreThread != thisThread)
            {
                SystemParametersInfoGet(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, ref timeout, 0);
                SystemParametersInfoSet(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, 0, 0);
                AttachThreadInput(thisThread, foreThread, true);
            }
            SetForegroundWindow(hWnd);
            SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_ASYNCWINDOWPOS);
            BringWindowToTop(hWnd);
            ShowWindowAsync(hWnd, SW_SHOW);
            SetFocus(hWnd);

            if (foreThread != thisThread)
            {
                SystemParametersInfoSet(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, timeout, 0);
                AttachThreadInput(thisThread, foreThread, false);
            }
        }
        #endregion

        #region TabHeader
        public static FormattedText CreateFormatedText(string s, double width, TextAlignment align, MainWindow parent)
        {
            FormattedText fmtText = new FormattedText(
                s,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("メイリオ"),
                35, Brushes.Black,
                VisualTreeHelper.GetDpi(parent).PixelsPerDip);
            fmtText.MaxTextWidth = width;
            fmtText.TextAlignment = align;
            return fmtText;
        }
        public static Grid makeTabHeader(MainWindow parent, string text = "新しいタブ", bool hideCloseBtn = true, string faviconUrl = "", int width = 0, string url = "", bool isPinned = false)
        {
            //CloseButton
            var CloseButton = new Button();
            var Favicon = new Image();
            InlineUIContainer inlineUIContainer = new InlineUIContainer();
            CloseButton.Click += parent.CloseButton_Click;
            CloseButton.Width = 17;
            CloseButton.Height = 17;
            CloseButton.HorizontalAlignment = HorizontalAlignment.Center;
            CloseButton.Background = null;
            CloseButton.BorderBrush = null;
            if (hideCloseBtn)
                CloseButton.Visibility = Visibility.Hidden;
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
            Favicon.Width = 15;
            Favicon.HorizontalAlignment = HorizontalAlignment.Center;
            Favicon.VerticalAlignment = VerticalAlignment.Center;
            Favicon.Margin = new Thickness(0, 0, 0, -3);
            
            VisualBrush vb = new VisualBrush();
            var border = new Border();
            border.Width = Favicon.ActualWidth;
            border.Height = Favicon.ActualHeight;
            border.CornerRadius = new CornerRadius(5);
            vb.Visual = border;
            if (faviconUrl == "")
            {
                if (url == "")
                {
                    Favicon.Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("Safali.star.png"));
                }
                else
                {
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = new BitmapImage(new Uri(@"D:\Downloads\square.png", UriKind.Absolute));
                    DrawingGroup drawingGroup = new DrawingGroup();
                    if ((new Uri(url).DnsSafeHost).Length > 0)
                    {
                        using (DrawingContext drawContent = drawingGroup.Open())
                        {
                            drawContent.DrawImage(imgBrush.ImageSource, new Rect(0, 0, 48, 48));
                            var str = (new Uri(url).DnsSafeHost)[0].ToString();
                            var formattedText = CreateFormatedText(str, 37, TextAlignment.Center, parent);
                            var point = new Point(3, 0);
                            drawContent.DrawText(formattedText, point);
                        }
                    }
                    Favicon.Source = new DrawingImage(drawingGroup);
                }
            }
            else
            {
                try
                {
                    Favicon.Source = new BitmapImage(new Uri(faviconUrl));
                }
                catch
                {
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = new BitmapImage(new Uri(@"D:\Downloads\square.png", UriKind.Absolute));
                    DrawingGroup drawingGroup = new DrawingGroup();
                    using (DrawingContext drawContent = drawingGroup.Open())
                    {
                        drawContent.DrawImage(imgBrush.ImageSource, new Rect(0, 0, 48, 48));
                        var str = (new Uri(url).DnsSafeHost)[0].ToString();
                        var formattedText = CreateFormatedText(str, 37, TextAlignment.Center, parent);
                        var point = new Point(3, 0);
                        drawContent.DrawText(formattedText, point);
                    }
                    Favicon.Source = new DrawingImage(drawingGroup);
                }
            }
            /*ParserContext context = new ParserContext();
            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("svgc", "http://sharpvectors.codeplex.com/svgc/");
            string Xaml = @"<Image Source=""{ svgc:SvgImage Source =./ favicon.svg }""/>";
            Image rslt = XamlReader.Parse(Xaml,context) as Image;
            if (faviconUrl.ToLower().EndsWith(".svg"))
                inlineUIContainer.Child = Favicon;
            else
                inlineUIContainer.Child = rslt; */
            inlineUIContainer.Child = Favicon;
            //HeaderText
            var headerText = new TextBlock();
            headerText.Inlines.Add(inlineUIContainer);
            if (text == "")
                text = "新しいタブ";
            Run run = new Run(text);
            headerText.Inlines.Add(run);
            headerText.Height = 18;
            headerText.Margin = new Thickness(0, 0, 30, 0);
            headerText.TextAlignment = TextAlignment.Center;
            headerText.TextWrapping = TextWrapping.NoWrap;
            Grid.SetColumn(headerText, 1);
            //Grid
            var grid1 = new Grid();
            grid1.MinWidth = width - 10;
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(17, GridUnitType.Pixel);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.MinWidth = width - 15;
            grid1.ColumnDefinitions.Add(c1);
            grid1.ColumnDefinitions.Add(c2);
            grid1.Children.Add(CloseButton);
            if (width > 0)
                grid1.Width = width;
            grid1.Children.Add(headerText);
            return grid1;
        }
        #endregion
    }
}
