using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using CefSharp;
using CefSharp.Enums;
using CefSharp.Wpf;

namespace Safali.Handlers
{
    class KeyboardHandler : IKeyboardHandler
    {
        bool IKeyboardHandler.OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            return false;
        }

        private static readonly List<double> ZOOM_LEVLES = new List<double> { -7.604, -6.081, -3.802, -2.197, -1.578, -1.224, -0.578, 0, 0.523, 1.224, 3.069, 3.802, 5.026, 6.026, 7.604, 8.827 };

        bool IKeyboardHandler.OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            if (type == KeyType.RawKeyDown)
            {
                // ALT+VK_LEFT
                if (windowsKeyCode == (int)Keys.Left && modifiers == CefEventFlags.AltDown)
                {
                    if (browser.CanGoBack)
                    {
                        // 戻る
                        browser.GoBack();
                        return true;
                    }
                }
                // ALT+VK_RIHGT
                else if (windowsKeyCode == (int)Keys.Right && modifiers == CefEventFlags.AltDown)
                {
                    if (browser.CanGoForward)
                    {
                        // 進む
                        browser.GoForward();
                        return true;
                    }
                }
                // VK_F5
                else if (windowsKeyCode == (int)Keys.F5 && modifiers == CefEventFlags.None)
                {
                    // 更新する
                    browser.Reload();
                    return true;
                }
                // VK_F12キー
                if (windowsKeyCode == (int)Keys.F12 && modifiers == CefEventFlags.None)
                {
                    // 開発者ツールを表示する
                    browser.ShowDevTools();
                    return true;
                }
                // Ctrl + NumberPad_Add
                // Ctrl + Add
                if ((windowsKeyCode == (int)Keys.Add && (modifiers == (CefEventFlags.ControlDown | CefEventFlags.IsKeyPad))) ||
                    (windowsKeyCode == (int)Keys.Oemplus && (modifiers == (CefEventFlags.ControlDown | CefEventFlags.ShiftDown))))
                {
                    // 現在のZoomLevelを取得する
                    browser.GetZoomLevelAsync().ContinueWith((t) =>
                    {
                        // 現在のZoomLevelの次に大きなZoomLevelを求めて設定する
                        int index = ZOOM_LEVLES.IndexOf(t.Result);
                        index = Math.Min(++index, ZOOM_LEVLES.Count - 1);
                        browser.SetZoomLevel(ZOOM_LEVLES[index]);
                    });
                    return true;
                }
                // Ctrl + NumberPad_Minus
                // Ctrl + Minus
                else if ((windowsKeyCode == (int)Keys.Subtract && (modifiers == (CefEventFlags.ControlDown | CefEventFlags.IsKeyPad))) ||
                    (windowsKeyCode == (int)Keys.OemMinus && (modifiers == (CefEventFlags.ControlDown | CefEventFlags.ShiftDown))))
                {
                    // 現在のZoomLevelを取得する
                    browser.GetZoomLevelAsync().ContinueWith((t) =>
                    {
                        // 現在のZoomLevelの次に小さいZoomLevelを求めて設定する
                        int index = ZOOM_LEVLES.IndexOf(t.Result);
                        index = Math.Max(--index, 0);
                        browser.SetZoomLevel(ZOOM_LEVLES[index]);
                    });
                    return true;
                }
                // Ctrl + NumberPad_0
                // Ctrl + 0
                else if ((windowsKeyCode == (int)Keys.D0 && modifiers == CefEventFlags.ControlDown) ||
                    (windowsKeyCode == (int)Keys.NumPad0 && modifiers == (CefEventFlags.ControlDown | CefEventFlags.IsKeyPad)))
                {
                    // 100%の倍率に戻す
                    browser.SetZoomLevel(0);
                    return true;
                }
                // CTRL＋[P]
                if (windowsKeyCode == (int)Keys.P && modifiers == CefEventFlags.ControlDown)
                {
                    // 印刷する
                    browser.Print();
                    return true;
                }
            }
            return false;
        }
    }
    class RequestHandler : IRequestHandler
    {
        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            // コントロールのトップレベルのコントロールを取得（SimpleBrowserFrame）
            Window mainFrame = ExtensionClass.getMainFrame(browser);

            if (mainFrame != null)
            {
                // 親コントロールのコンテキストで非同期にダイアログを表示する。
                mainFrame.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // ログオンダイアログを表示する
                    Auth dlg = new Auth();
                    bool? ret = dlg.ShowDialog();

                    if (ret == true)
                    {
                        // 入力されたユーザ名とパスワードで認証を継続する。
                        callback.Continue(dlg.UserName, dlg.Password);
                    }
                    else
                    {
                        // 認証処理をキャンセルする。
                        callback.Cancel();
                    }
                }));
            }
            else
            {
                // 認証処理をキャンセルする。
                callback.Cancel();
            }
            return true;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return null;
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            // ナビゲーションを許可する
            return false;
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            callback.Continue(true);
            return true;
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return true;
        }

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
        }

        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            // クオータの要求は常に許可する。
            callback.Continue(true);
            return true;
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {

            // デフォルトのクライアント証明書の選択方法とする。
            return false;
        }
    }

    class DisplayHandler : IDisplayHandler
    {
        public void OnAddressChanged(IWebBrowser chromiumWebBrowser, AddressChangedEventArgs addressChangedArgs)
        {
        }


        public bool OnAutoResize(IWebBrowser chromiumWebBrowser, IBrowser browser, CefSharp.Structs.Size newSize)
        {
            return false;
        }

        public bool OnConsoleMessage(IWebBrowser chromiumWebBrowser, ConsoleMessageEventArgs consoleMessageArgs)
        {
            // コンソールにメッセージを出力する
            return false;
        }

        public bool OnCursorChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr cursor, CursorType type, CefSharp.Structs.CursorInfo customCursorInfo)
        {
            return false;
        }

        public void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls)
        {
        }

        private Grid parent;
        private Window fullScreenWindow;
        void IDisplayHandler.OnFullscreenModeChange(IWebBrowser browserControl, IBrowser browser, bool fullscreen)
        {
            var webBrowser = (ChromiumWebBrowser)browserControl;

            webBrowser.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (fullscreen)
                {
                    parent = (Grid)VisualTreeHelper.GetParent(webBrowser);
                    parent.Children.Remove(webBrowser);
                    fullScreenWindow = new Window
                    {
                        WindowStyle = WindowStyle.None,
                        WindowState = WindowState.Maximized,
                        Content = webBrowser
                    };
                    fullScreenWindow.ShowDialog();
                }
                else
                {
                    fullScreenWindow.Content = null;
                    parent.Children.Add(webBrowser);
                    fullScreenWindow.Close();
                    fullScreenWindow = null;
                    parent = null;
                }
            }));
        }

        public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
        {
        }


        public void OnStatusMessage(IWebBrowser chromiumWebBrowser, StatusMessageEventArgs statusMessageArgs)
        {
        }

        public void OnTitleChanged(IWebBrowser chromiumWebBrowser, TitleChangedEventArgs titleChangedArgs)
        {
            // コントロールのトップレベルのコントロールを取得（SimpleBrowserFrame）
            Window mainFrame = new Window();
            try
            {
                mainFrame = ExtensionClass.getMainFrame(titleChangedArgs.Browser);
            }
            catch
            {

            }


            if (mainFrame != null)
            {
                // 親コントロールのコンテキストでタイトル文字列を変更する。
                mainFrame.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // タイトル文字列を変更する
                    if (titleChangedArgs.Title == "")
                        mainFrame.Title = "New Tab - Safali for Windows";
                    else
                        mainFrame.Title = titleChangedArgs.Title + " - Safali for Windows";
                }));
            }
        }

        public bool OnTooltipChanged(IWebBrowser chromiumWebBrowser, ref string text)
        {
            return false;
        }
    }

    class LifespanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl,
            string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
            IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            return false;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
