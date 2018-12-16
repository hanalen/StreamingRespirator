using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;

namespace StreamingRespirator.Core
{
    internal static class Program
    {
        public const string MutexName = "{5FF75362-95BA-4399-8C77-C1A0C5B8A291}";
        public static readonly CefSettings     DefaultCefSetting;
        public static readonly BrowserSettings DefaultBrowserSetting;

        public static readonly string CookiePath;

        static Program()
        {
            DefaultCefSetting = new CefSettings
            {
                CachePath                  = null,
                LogSeverity                = LogSeverity.Disable,
                LogFile                    = null,
                WindowlessRenderingEnabled = true,
                BrowserSubprocessPath      = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "StreamingRespirator.cef.exe"),
            };

#if !DEBUG
            DefaultCefSetting.CefCommandLineArgs["no-proxy-server"]           = "1";
#endif
            DefaultCefSetting.CefCommandLineArgs["disable-application-cache"] = "1";
            DefaultCefSetting.CefCommandLineArgs["disable-extensions"       ] = "1";
            DefaultCefSetting.CefCommandLineArgs["off-screen-frame-rate"    ] = "1";

            // Disable Surfaces so internal PDF viewer works for OSR
            DefaultCefSetting.CefCommandLineArgs["disable-surfaces"] = "1";

            //DisableGpuAcceleration
            DefaultCefSetting.DisableGpuAcceleration();
            DefaultCefSetting.CefCommandLineArgs["disable-gpu"] = "1";

            //SetOffScreenRenderingBestPerformanceArgs
            DefaultCefSetting.SetOffScreenRenderingBestPerformanceArgs();
            DefaultCefSetting.CefCommandLineArgs["disable-gpu"                  ] = "1";
            DefaultCefSetting.CefCommandLineArgs["disable-gpu-compositing"      ] = "1";
            DefaultCefSetting.CefCommandLineArgs["enable-begin-frame-scheduling"] = "1";

            DefaultBrowserSetting = new BrowserSettings
            {
                DefaultEncoding           = "UTF-8",
                WebGl                     = CefState.Disabled,
                Plugins                   = CefState.Disabled,
                JavascriptAccessClipboard = CefState.Disabled,
                ImageLoading              = CefState.Disabled,
                JavascriptCloseWindows    = CefState.Disabled,
                ApplicationCache          = CefState.Disabled,
                RemoteFonts               = CefState.Disabled,
                WindowlessFrameRate       = 1,
                //LocalStorage              = CefState.Disabled, // => Uncaught TypeError: Cannot read property 'getItem' of null
                Databases                 = CefState.Disabled,
            };

            CookiePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), ".cookie");
        }

        [STAThread]
        static void Main()
        {
            using (var mut = new Mutex(true, MutexName, out bool createdNew))
            {
                if (!createdNew)
                    return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
                if (!CheckUpdate())
                    return;
#endif

                var context = new MainContext();
                Application.Run(context);
                context.StopProxy();

                Cef.Shutdown();
            }
        }

        static bool CheckUpdate()
        {
            if (GithubLatestRelease.CheckNewVersion())
            {
                MessageBox.Show("새로운 업데이트가 있습니다.", "스트리밍 호흡기");

                try
                {
                    Process.Start("https://github.com/RyuaNerin/StreamingRespirator/blob/master/README.md")?.Dispose();
                }
                catch
                {
                }

                return false;
            }

            return true;
        }
    }
}
