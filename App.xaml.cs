using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace Safali
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            CefSettings settings = new CefSettings()
            {
                AcceptLanguageList = "ja,en-US;q=0.9,en;q=0.8",
                CachePath = $@"{Path.GetTempPath()}safali\cache\",
                UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36 Safali/1.0.0.0",
                LogSeverity = LogSeverity.Verbose
            };
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            Cef.Initialize(settings);
            Safali.App app = new Safali.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
