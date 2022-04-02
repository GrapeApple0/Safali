using System;
using System.Collections.Generic;
using System.Windows;
using CefSharp;
using CefSharp.Enums;
using CefSharp.Wpf;

namespace Safali.Handlers
{
    class DisplayHandler : IDisplayHandler
    {
        public void OnAddressChanged(IWebBrowser chromiumWebBrowser, AddressChangedEventArgs addressChangedArgs)
        {
        }

        public bool OnAutoResize(IWebBrowser chromiumWebBrowser, IBrowser browser, Size newSize)
        {
            // ブラウザのデフォルト処理を行う
            return false;
        }

        public bool OnAutoResize(IWebBrowser chromiumWebBrowser, IBrowser browser, CefSharp.Structs.Size newSize)
        {
            throw new NotImplementedException();
        }

        public bool OnConsoleMessage(IWebBrowser chromiumWebBrowser, ConsoleMessageEventArgs consoleMessageArgs)
        {
            // コンソールにメッセージを出力する
            return false;
        }

        public bool OnCursorChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr cursor, CursorType type, CefSharp.Structs.CursorInfo customCursorInfo)
        {
            throw new NotImplementedException();
        }

        public void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls)
        {
        }

        public void OnFullscreenModeChange(IWebBrowser chromiumWebBrowser, IBrowser browser, bool fullscreen)
        {
        }

        public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
        {
        }

        public void OnStatusMessage(IWebBrowser chromiumWebBrowser, StatusMessageEventArgs statusMessageArgs)
        {
        }

        public void OnTitleChanged(IWebBrowser chromiumWebBrowser, TitleChangedEventArgs titleChangedArgs)
        {
            /*
            // コントロールのトップレベルのコントロールを取得（SimpleBrowserFrame）
            SimpleBrowserFrame mainFrame = SimpleBrowserFrame.getMainFrame(titleChangedArgs.Browser);

            if (mainFrame != null)
            {
                // 親コントロールのコンテキストでタイトル文字列を変更する。
                mainFrame.BeginInvoke(new Action(() =>
                {
                    // タイトル文字列を変更する
                    mainFrame.Text = titleChangedArgs.Title;
                }));
            }*/
        }

        public bool OnTooltipChanged(IWebBrowser chromiumWebBrowser, ref string text)
        {
            return false;
        }
    }
}
