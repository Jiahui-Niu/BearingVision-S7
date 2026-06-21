using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ICPlatformTools
{
    public class SerialPortUtil
    {
        const int BUFFSIZE = 2048;

        private object _locker = new object();

        private SerialPort serialPort;

        public Action<byte[]> MessageReceiveHandler;

        public SerialPortUtil(string comPort, int baudRate, string parity)
        {
            var _parity = Parity.None;
            if (parity != null)
            {
                if (!Enum.TryParse<Parity>(parity, out _parity))
                {
                    _parity = Parity.None;
                }
            }
            else
            {
                _parity = Parity.None;
            }
            serialPort = new SerialPort(comPort, baudRate, _parity);
        }

        public SerialPortUtil(string comPort, int baudRate, Parity parity, int databits)
        {
            serialPort = new SerialPort(comPort, baudRate, parity, databits);
        }

        public SerialPortUtil(string comPort, int baudRate)
        {
            serialPort = new SerialPort(comPort, baudRate);
        }

        public bool IsOpen
        {
            get { return serialPort == null ? false : serialPort.IsOpen; }
        }

        public void Start()
        {
            try
            {
                serialPort.Open();
                serialPort.DataReceived += serialPort_DataReceived;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
                Log2.LogError(this, ex.ToString());
            }
            Log2.LogInfo(this, "serialport open successful");
            //SendThread = new Thread(SendMessage);
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var buffer = new byte[BUFFSIZE];
            try
            {
                Thread.Sleep(10);
                List<byte> bytes = new List<byte>();
                while (true)
                {
                    if (serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(10);
                        if (serialPort.BytesToRead == 0)
                            break;
                    }

                    var availableLen = serialPort.BytesToRead;
                    int len = serialPort.Read(buffer, 0, BUFFSIZE);
                    var data = new byte[len];
                    Array.Copy(buffer, data, len);
                    bytes.AddRange(data);
                }
                if (MessageReceiveHandler != null)
                    MessageReceiveHandler.Invoke(bytes.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("[SerilportUtil] error, at Message receiving process.", ex);
            }
        }

        public bool Send(byte[] bytes)
        {
            bool ret = false;
            if (serialPort != null && serialPort.IsOpen)
            {
                lock (_locker)
                {
                    try
                    {
                        LogHelper.Log.DebugFormat("[SerialPortUtil] Send bytes: {0}", BitConverter.ToString(bytes));
                        serialPort.Write(bytes, 0, bytes.Length);
                        ret = true;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log.Error(ex);
                    }
                }
            }
            return ret;
        }

        public bool Send(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Send(bytes);
        }


        public void Close()
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.DataReceived -= serialPort_DataReceived;
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                }
            }
        }

        public static string[] GetComPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
