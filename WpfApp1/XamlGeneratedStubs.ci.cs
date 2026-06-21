// This file replaces XAML-compiler-generated code in CI (BuildingForCI=true).
// It provides InitializeComponent() stubs and x:Name fields so the C# compiler
// succeeds without running mc.exe / WPFTaskHost.exe (which fails in CI due to
// missing System.Core 3.5 assembly dependencies from referenced NuGet DLLs).
// Do NOT use at runtime — the application will not render any UI from this stub.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

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

    partial class MainWindow : System.Windows.Markup.IComponentConnector
    {
        internal TabControl mainTab;
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }
}

namespace WpfApp1.UserControls
{
    partial class MainImageShowUserControl : System.Windows.Markup.IComponentConnector
    {
        internal UniformGrid cameraGrid;
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }

    partial class BrightnessUserControl : System.Windows.Markup.IComponentConnector
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }

    partial class CheckSetUserControl : System.Windows.Markup.IComponentConnector
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }

    partial class DataSelectUserControl : System.Windows.Markup.IComponentConnector
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }

    partial class TCPClientUserControl : System.Windows.Markup.IComponentConnector
    {
        private bool _contentLoaded;

        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
        }

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }
}

namespace WpfApp1.View
{
    partial class LoginWindow : System.Windows.Markup.IComponentConnector
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

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }

    partial class ReViewWindow : System.Windows.Markup.IComponentConnector
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

        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }
}
