using System;
using System.IO;
using System.Windows;
using WpfApp1.ViewModel;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow(int role = 0, string userName = "操作员")
        {
            InitializeComponent();
            _vm = new MainViewModel();
            _vm.CurrentUserRole = role;
            _vm.CurrentUserName = userName;
            DataContext = _vm;

            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SolutionPath))
                    LoadFrontendSolution();
            };
            LoadFrontendSolution();
        }

        /// <summary>
        /// 把当前方案路径加载进方案配置页的VM原生工作台(VmFrontendControl)，供查看/编辑；
        /// 与"开始检测"时后台加载的运行态方案(MainViewModel.LoadVMSolution)相互独立
        /// </summary>
        private void LoadFrontendSolution()
        {
            var path = _vm.SolutionPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            try { VmFrontend.LoadSol(path, _vm.SolutionPassword); }
            catch (Exception ex) { MessageBox.Show("加载方案到VM工作台失败: " + ex.Message, "提示"); }
        }

        /// <summary>"导入方案"按钮：显式触发加载，用于填写/修改方案密码后重试</summary>
        private void BtnImportSolution_Click(object sender, RoutedEventArgs e) => LoadFrontendSolution();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_vm.IsRunning)
            {
                var r = MessageBox.Show("检测正在运行，确认退出？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (r != MessageBoxResult.Yes) { e.Cancel = true; return; }
            }
            _vm.Cleanup();
        }

        private void MenuSelectSolution_Click(object sender, RoutedEventArgs e) => _vm.SelectSolutionPath();
        private void MenuLoadSolution_Click(object sender, RoutedEventArgs e) => MessageBox.Show("方案将在启动检测时自动加载");
        private void MenuCameraSet_Click(object sender, RoutedEventArgs e) => MessageBox.Show("相机设置请在VM平台中配置");
        private void MenuGlobalVar_Click(object sender, RoutedEventArgs e) => MessageBox.Show("全局变量请在VM平台中查看");
        private void MenuLightComm_Click(object sender, RoutedEventArgs e) => mainTab.SelectedIndex = 3;
        private void MenuLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new View.LoginWindow();
            if (login.ShowDialog() == true) { }
        }

        private void BtnBrowseSolution_Click(object sender, RoutedEventArgs e) => _vm.SelectSolutionPath();
    }
}
