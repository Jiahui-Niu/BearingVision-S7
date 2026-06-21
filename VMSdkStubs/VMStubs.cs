// CI 编译用桩文件 —— 仅提供类型定义，供编译器解析引用
// 生产环境中这些类型来自真实的 VisionMaster 4.4.3 SDK DLL

namespace VM.Core
{
    public class MVisionMaster
    {
        private static readonly MVisionMaster _instance = new MVisionMaster();
        public static MVisionMaster Instance => _instance;
        public IMVSolution Solution { get; } = null;
    }

    public interface IMVSolution
    {
        void Load(string path);
        void DestroyAllModule();
        void StopAllModules();
        IMVSProcedure GetProcedure(string procedureName);
    }

    public interface IMVSProcedure
    {
        int WaitRun(int timeoutMs);
        void Run();
        object GetModule(string moduleName);
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
