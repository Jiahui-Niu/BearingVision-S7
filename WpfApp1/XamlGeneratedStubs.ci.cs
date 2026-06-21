// CI stub — replaces XAML-compiler-generated code when BuildingForCI=true.
// Provides InitializeComponent() and x:Name fields so C# compiles without mc.exe.
// Do NOT use at runtime; the application will not render UI from this stub.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfApp1
{
    partial class App : Application
    {
        public void InitializeComponent() { }

        [System.STAThreadAttribute()]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }

    partial class MainWindow
    {
        internal TabControl mainTab;
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }
}

namespace WpfApp1.UserControls
{
    partial class MainImageShowUserControl
    {
        internal UniformGrid cameraGrid;
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }

    partial class BrightnessUserControl
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }

    partial class CheckSetUserControl
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }

    partial class DataSelectUserControl
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }

    partial class TCPClientUserControl
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }
}

namespace WpfApp1.View
{
    partial class LoginWindow
    {
        internal ComboBox cmbUser;
        internal PasswordBox pwdBox;
        internal TextBlock txtError;
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }

    partial class ReViewWindow
    {
        internal ComboBox cmbCamera;
        internal DatePicker dpStart;
        internal DatePicker dpEnd;
        internal ComboBox cmbResult;
        internal TextBlock txtStatus;
        internal ListBox lstImages;
        internal Image imgPreview;
        internal TextBlock txtNoImage;
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }
    }
}
