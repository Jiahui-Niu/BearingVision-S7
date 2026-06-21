using System.Windows;
using System.Windows.Controls;
using WpfApp1.ViewModel;

namespace WpfApp1.UserControls
{
    public partial class CheckSetUserControl : UserControl
    {
        public CheckSetUserControl() => InitializeComponent();

        private void BtnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (DataContext is CheckSetViewModel vm)
                dialog.SelectedPath = vm.SaveImagePath ?? @"D:\";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (DataContext is CheckSetViewModel vm2)
                    vm2.SaveImagePath = dialog.SelectedPath;
            }
        }
    }
}
