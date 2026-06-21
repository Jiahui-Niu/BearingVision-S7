using System;
using System.Threading.Tasks;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Omron;
using FinsTcp = HslCommunication.Profinet.Omron.OmronFinsNet;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class TCPClient : ViewModelBase
    {
        private string _plcType = "S7";
        private string _ip = "192.168.0.1";
        private int _port = 102;
        private string _address = "";
        private string _value = "0";
        private bool _isConnected;
        private string _statusText = "未连接";

        private SiemensS7Net _s7Client;
        private OmronFinsNet _finsClient;

        public string PLCType { get => _plcType; set { SetField(ref _plcType, value); OnPropertyChanged(nameof(IsS7)); OnPropertyChanged(nameof(IsFins)); } }
        public string IP { get => _ip; set => SetField(ref _ip, value); }
        public int Port { get => _port; set => SetField(ref _port, value); }
        public string Address { get => _address; set => SetField(ref _address, value); }
        public string Value { get => _value; set => SetField(ref _value, value); }
        public bool IsConnected { get => _isConnected; set => SetField(ref _isConnected, value); }
        public string StatusText { get => _statusText; set => SetField(ref _statusText, value); }

        public bool IsS7 => PLCType == "S7";
        public bool IsFins => PLCType == "Fins";

        public RelayCommand ConnectCommand => new RelayCommand(Connect);
        public RelayCommand DisconnectCommand => new RelayCommand(Disconnect);
        public RelayCommand ReadCommand => new RelayCommand(ReadAddress);
        public RelayCommand WriteCommand => new RelayCommand(WriteAddress);

        public void LoadFromConfig(AppConfig config)
        {
            PLCType = config.PLCType;
            IP = config.PLCIp;
            Port = config.PLCPort;
        }

        public void SaveToConfig(AppConfig config)
        {
            config.PLCType = PLCType;
            config.PLCIp = IP;
            config.PLCPort = Port;
        }

        public void Connect()
        {
            Task.Run(() =>
            {
                try
                {
                    Disconnect();
                    if (PLCType == "S7")
                    {
                        _s7Client = new SiemensS7Net(SiemensPLCS.S1200, IP);
                        _s7Client.Port = Port;
                        var result = _s7Client.ConnectServer();
                        IsConnected = result.IsSuccess;
                        StatusText = result.IsSuccess ? $"已连接 {IP}:{Port}" : $"连接失败: {result.Message}";
                    }
                    else if (PLCType == "Fins")
                    {
                        _finsClient = new OmronFinsNet(IP);
                        _finsClient.Port = Port;
                        var result = _finsClient.ConnectServer();
                        IsConnected = result.IsSuccess;
                        StatusText = result.IsSuccess ? $"已连接 {IP}:{Port}" : $"连接失败: {result.Message}";
                    }
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    StatusText = $"连接异常: {ex.Message}";
                    ICPlatformTools.LogHelper.Log.Error("PLC连接失败", ex);
                }
            });
        }

        public void Disconnect()
        {
            try
            {
                _s7Client?.ConnectClose();
                _s7Client = null;
                _finsClient?.ConnectClose();
                _finsClient = null;
                IsConnected = false;
                StatusText = "已断开";
            }
            catch { }
        }

        private void ReadAddress()
        {
            if (!IsConnected) return;
            Task.Run(() =>
            {
                try
                {
                    string result;
                    if (_s7Client != null)
                    {
                        var r = _s7Client.ReadInt16(Address);
                        result = r.IsSuccess ? r.Content.ToString() : r.Message;
                    }
                    else if (_finsClient != null)
                    {
                        var r = _finsClient.ReadInt16(Address);
                        result = r.IsSuccess ? r.Content.ToString() : r.Message;
                    }
                    else return;
                    Value = result;
                }
                catch (Exception ex) { StatusText = ex.Message; }
            });
        }

        private void WriteAddress()
        {
            if (!IsConnected) return;
            Task.Run(() =>
            {
                try
                {
                    if (short.TryParse(Value, out short v))
                    {
                        if (_s7Client != null) _s7Client.Write(Address, v);
                        else if (_finsClient != null) _finsClient.Write(Address, v);
                    }
                }
                catch (Exception ex) { StatusText = ex.Message; }
            });
        }

        // Exposed for MainViewModel to use same client
        public SiemensS7Net S7Client => _s7Client;
        public OmronFinsNet FinsClient => _finsClient;
    }
}
