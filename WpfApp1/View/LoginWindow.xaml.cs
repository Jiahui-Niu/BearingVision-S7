using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Model;

namespace WpfApp1.View
{
    public partial class LoginWindow : Window
    {
        private static readonly Dictionary<string, (string pwd, int role)> _users = new Dictionary<string, (string, int)>
        {
            { "操作员", ("111", 0) },
            { "工程师", ("222", 1) },
            { "管理员", ("333", 2) },
        };

        public int LoginRole { get; private set; } = -1;
        public string LoginUser { get; private set; } = "";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            LoginRole = 0;
            LoginUser = "操作员";
            OpenMainWindow();
        }

        private void PwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) DoLogin();
        }

        private void DoLogin()
        {
            var user = (cmbUser.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "";
            var pwd = pwdBox.Password;

            if (_users.TryGetValue(user, out var info) && info.pwd == pwd)
            {
                LoginRole = info.role;
                LoginUser = user;
                OpenMainWindow();
            }
            else
            {
                txtError.Text = "密码错误，请重试";
                pwdBox.Clear();
            }
        }

        private void OpenMainWindow()
        {
            var mainWin = new MainWindow(LoginRole, LoginUser);
            mainWin.Show();
            Close();
        }
    }
}
