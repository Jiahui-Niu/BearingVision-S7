// CI 编译用桩文件 —— 仅提供类型定义，供编译器解析引用
// 生产环境中 VM SDK 类型来自真实的 VisionMaster 4.4.3 SDK DLL；
// HslCommunication 类型来自手动放置的 HslCommunication.dll

// ── HslCommunication 桩（WpfApp1 用到的最小 API 集） ──────────────────────
namespace HslCommunication
{
    public class OperateResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ToMessageShowString() => Message;
    }

    public class OperateResult<T> : OperateResult
    {
        public T Content { get; set; }
    }
}

namespace HslCommunication.Profinet.Siemens
{
    public enum SiemensPLCS { S200, S200Smart, S300, S400, S1200, S1500, S300_Smart }

    public class SiemensS7Net
    {
        public SiemensS7Net(SiemensPLCS plcType, string ipAddress) { }
        public int Port { get; set; }
        public HslCommunication.OperateResult ConnectServer() => new HslCommunication.OperateResult();
        public void ConnectClose() { }
        public HslCommunication.OperateResult<short> ReadInt16(string address) => new HslCommunication.OperateResult<short>();
        public HslCommunication.OperateResult<bool> ReadBool(string address) => new HslCommunication.OperateResult<bool>();
        public HslCommunication.OperateResult Write(string address, short value) => new HslCommunication.OperateResult();
        public HslCommunication.OperateResult Write(string address, bool value) => new HslCommunication.OperateResult();
    }
}

namespace HslCommunication.Profinet.Omron
{
    public class OmronFinsNet
    {
        public OmronFinsNet(string ipAddress, int port) { }
        public int Port { get; set; }
        public HslCommunication.OperateResult ConnectServer() => new HslCommunication.OperateResult();
        public void ConnectClose() { }
        public HslCommunication.OperateResult<short> ReadInt16(string address) => new HslCommunication.OperateResult<short>();
        public HslCommunication.OperateResult<bool> ReadBool(string address) => new HslCommunication.OperateResult<bool>();
        public HslCommunication.OperateResult Write(string address, short value) => new HslCommunication.OperateResult();
        public HslCommunication.OperateResult Write(string address, bool value) => new HslCommunication.OperateResult();
    }
}

// ── VisionMaster SDK 桩 ────────────────────────────────────────────────────

namespace VM.Core
{
    public class VmSolution
    {
        private static readonly VmSolution _instance = new VmSolution();
        public static VmSolution Instance => _instance;
        public static void Load(string path, string password, bool autoRun) { }
        public void CloseSolution() { }
        public VmIndependentProcedure GetProcedure(string procedureName) => null;
        public VmIndependentProcedure this[string name] => null;
    }

    public class VmIndependentProcedure
    {
        public int WaitRun(int timeoutMs) => 0;
        public void Run() { }
        public object GetModule(string moduleName) => null;
    }
}

namespace GlobalVariableModuleCs
{
    public interface IMVSGlobalVariableModuCs
    {
        int GetIntValue(string varName);
        string GetStringValue(string varName);
        double GetDoubleValue(string varName);
        void SetIntValue(string varName, int value);
        void SetStringValue(string varName, string value);
    }
}

namespace ImageCollectModuCs
{
    public class VMImageInfo
    {
        public System.Drawing.Bitmap ToBitmap() => null;
    }

    public interface IImageCollectModuCs
    {
        VMImageInfo GetOutputImage();
    }
}
