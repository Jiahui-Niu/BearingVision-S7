using System;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;

namespace WpfApp1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化 log4net
            var configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));
            if (configFile.Exists)
                XmlConfigurator.ConfigureAndWatch(configFile);
            else
                BasicConfigurator.Configure();

            var log = ICPlatformTools.LogHelper.Log;

            // 启动横幅，方便定位日志段落
            log.Info("================================================");
            log.Info("  成品轴承外观机 启动");
            log.Info($"  版本:    {Assembly.GetExecutingAssembly().GetName().Version}");
            log.Info($"  程序目录: {AppDomain.CurrentDomain.BaseDirectory}");
            log.Info($"  OS:      {Environment.OSVersion}");
            log.Info($"  .NET:    {Environment.Version}");
            log.Info($"  CPU数:   {Environment.ProcessorCount}");
            log.Info($"  内存:    {GC.GetTotalMemory(false) / 1048576.0:F0} MB (托管)");
            log.Info($"  时间:    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            log.Info("================================================");

            // 全局异常捕获 — 确保崩溃信息写入日志
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                log.Fatal("【致命】未处理异常，程序即将退出", ex.ExceptionObject as Exception);
                MessageBox.Show(
                    $"程序发生未处理异常，请将 logs 文件夹发给开发人员。\n\n{ex.ExceptionObject}",
                    "程序错误", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            DispatcherUnhandledException += (s, ex) =>
            {
                log.Error("【UI线程】未处理异常", ex.Exception);
                MessageBox.Show(
                    $"界面发生异常，请将 logs 文件夹发给开发人员。\n\n{ex.Exception.Message}",
                    "界面错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ICPlatformTools.LogHelper.Log.Info($"程序正常退出，ExitCode={e.ApplicationExitCode}");
            base.OnExit(e);
        }
    }
}
