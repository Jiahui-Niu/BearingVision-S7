using System.Windows;
using System.Windows.Controls;
using VMControls.WPF.Release;
using WpfApp1.ViewModel;

namespace WpfApp1.UserControls
{
    public partial class MainImageShowUserControl : UserControl
    {
        public MainImageShowUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 每个相机格子里的 VmRenderControl 实例加载完成后，把自身挂到对应相机的 ViewModel 上，
        /// 供 MainViewModel 在方案加载完成后调用 ModuleSource 绑定
        /// </summary>
        private void VmRenderControl_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as VmRenderControl;
            var fe = sender as FrameworkElement;
            if (ctrl != null && fe?.DataContext is MainImageShowViewModel camVm)
                camVm.RenderControl = ctrl;
        }
    }
}
