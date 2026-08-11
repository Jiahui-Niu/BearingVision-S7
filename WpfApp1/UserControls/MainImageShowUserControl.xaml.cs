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
        /// 每个相机格子里的 VmRenderControl 实例加载完成后，把自身注册到对应相机的 ViewModel 上，
        /// 供 MainViewModel 在方案加载完成后调用 ModuleSource 绑定。
        /// "主页面"和"实时图像"两个Tab各有一份本控件、都绑定同一个相机VM，故用列表登记全部实例，
        /// 而不是只记最后一个，否则后加载的Tab会把先加载的Tab的绑定挤掉
        /// </summary>
        private void VmRenderControl_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as VmRenderControl;
            var fe = sender as FrameworkElement;
            if (ctrl != null && fe?.DataContext is MainImageShowViewModel camVm)
                camVm.AddRenderControl(ctrl);
        }

        private void VmRenderControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as VmRenderControl;
            var fe = sender as FrameworkElement;
            if (ctrl != null && fe?.DataContext is MainImageShowViewModel camVm)
                camVm.RemoveRenderControl(ctrl);
        }
    }
}
