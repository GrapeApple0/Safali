using CefSharp;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
}
