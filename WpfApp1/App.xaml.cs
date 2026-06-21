using System;
using System.Windows;

namespace WpfApp1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                ICPlatformTools.LogHelper.Log.Error("未处理异常", ex.ExceptionObject as Exception);
                MessageBox.Show($"程序发生未处理异常：{ex.ExceptionObject}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            DispatcherUnhandledException += (s, ex) =>
            {
                ICPlatformTools.LogHelper.Log.Error("UI线程未处理异常", ex.Exception);
                ex.Handled = true;
            };
        }
    }
}
