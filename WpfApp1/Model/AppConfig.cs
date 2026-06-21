using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace WpfApp1.Model
{
    public class AppConfig
    {
        public string SolutionPath { get; set; } = @"D:\Debug\Sol\成品.sol";
        public string ModelNo { get; set; } = "";
        public bool HasEnclosure { get; set; } = false;

        public bool[] CameraEnable { get; set; } = new bool[6] { true, true, true, true, true, true };

        public string PLCType { get; set; } = "S7";
        public string PLCIp { get; set; } = "192.168.0.1";
        public int PLCPort { get; set; } = 102;
        public string PLCRack { get; set; } = "0";
        public string PLCSlot { get; set; } = "1";

        public string TotalCountAddr { get; set; } = "DB85.1";
        public string OKCountAddr { get; set; } = "DB85.3";
        public string NGCountAddr { get; set; } = "DB85.5";
        public string StartAddr { get; set; } = "DB85.7";
        public string ClearAddr { get; set; } = "DB85.9";
        public string DataType { get; set; } = "Int16";

        public string SaveImagePath { get; set; } = @"D:\Images";
        public int SaveDays { get; set; } = 30;
        public bool SaveOK { get; set; } = true;
        public bool SaveNG { get; set; } = true;
        public bool UsePhotometricStereo { get; set; } = false;

        // VM SDK 配置 — 名称需与 VM 方案编辑器中一致
        public string VMProcedurePrefix { get; set; } = "Cam";           // 流程名前缀，实际流程名 = prefix + (camIndex+1)
        public string VMGlobalVarModuleName { get; set; } = "全局变量";   // VM 方案中全局变量模块的名称
        public string VMResultVarName { get; set; } = "Result";           // 检测结果变量名 (1=OK, 2=NG)
        public string VMDefectVarName { get; set; } = "DefectInfo";       // 缺陷描述变量名
        public string VMImageModuleName { get; set; } = "ImageCollect0";  // 图像采集模块名
        public int VMRunTimeout { get; set; } = 8000;                     // 等待流程执行超时 (ms)

        public List<CameraConfig> Cameras { get; set; } = new List<CameraConfig>();
        public List<BrightnessStage> BrightnessStages { get; set; } = new List<BrightnessStage>();

        public string CurrentBatchNo { get; set; } = "";
        public string EditBatchNo { get; set; } = "";

        public bool HeartbeatEnabled { get; set; } = false;
        public string HeartbeatAddr { get; set; } = "";
        public string HeartbeatDataType { get; set; } = "Bool";

        public static AppConfig Default()
        {
            var cfg = new AppConfig();
            cfg.Cameras = new List<CameraConfig>
            {
                new CameraConfig { Index = 0, StationName = "内径1", FirstAddr = "DB85.40", StartAddr = "DB85.0",  ResultAddr = "DB85.20", DelayShots = 0, TotalShots = 18, ShotInterval = 0, IsOnline = true, EndFlag = false, IsRotation = true },
                new CameraConfig { Index = 1, StationName = "端面1", FirstAddr = "DB85.42", StartAddr = "DB85.2",  ResultAddr = "DB85.22", DelayShots = 0, TotalShots = 1,  ShotInterval = 0, IsOnline = true, EndFlag = false, IsRotation = false },
                new CameraConfig { Index = 2, StationName = "端面2", FirstAddr = "DB85.44", StartAddr = "DB85.4",  ResultAddr = "DB85.24", DelayShots = 0, TotalShots = 1,  ShotInterval = 0, IsOnline = true, EndFlag = true,  IsRotation = false },
                new CameraConfig { Index = 3, StationName = "内径2", FirstAddr = "DB85.46", StartAddr = "DB85.6",  ResultAddr = "DB85.26", DelayShots = 0, TotalShots = 18, ShotInterval = 0, IsOnline = true, EndFlag = false, IsRotation = true },
                new CameraConfig { Index = 4, StationName = "外径1", FirstAddr = "DB85.48", StartAddr = "DB85.8",  ResultAddr = "DB85.28", DelayShots = 0, TotalShots = 15, ShotInterval = 0, IsOnline = true, EndFlag = false, IsRotation = false },
                new CameraConfig { Index = 5, StationName = "外径2", FirstAddr = "DB85.50", StartAddr = "DB85.10", ResultAddr = "DB85.30", DelayShots = 0, TotalShots = 15, ShotInterval = 0, IsOnline = true, EndFlag = false, IsRotation = false },
            };
            cfg.BrightnessStages = new List<BrightnessStage>
            {
                new BrightnessStage { StageName = "一号分频阶段1", BackLight = 40, RingLight = 120, TopLight = 0 },
                new BrightnessStage { StageName = "一号分频阶段2", BackLight = 40, RingLight = 0,   TopLight = 240 },
                new BrightnessStage { StageName = "二号分频阶段1", BackLight = 0,  RingLight = 80,  TopLight = 0 },
                new BrightnessStage { StageName = "二号分频阶段2", BackLight = 0,  RingLight = 0,   TopLight = 220 },
            };
            cfg.CurrentBatchNo = DateTime.Now.ToString("yyyy-MM-dd-01");
            cfg.EditBatchNo = cfg.CurrentBatchNo;
            return cfg;
        }

        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData.dat");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var cfg = JsonConvert.DeserializeObject<AppConfig>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { }
            return Default();
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                ICPlatformTools.LogHelper.Log.Error("保存配置失败", ex);
            }
        }
    }

    public class CameraConfig
    {
        public int Index { get; set; }
        public string StationName { get; set; } = "";
        public string FirstAddr { get; set; } = "";
        public string StartAddr { get; set; } = "";
        public string ResultAddr { get; set; } = "";
        public int DelayShots { get; set; } = 0;
        public int TotalShots { get; set; } = 1;
        public int ShotInterval { get; set; } = 0;
        public bool IsOnline { get; set; } = true;
        public bool EndFlag { get; set; } = false;
        public bool IsRotation { get; set; } = false;
        public bool UsePhotometricStereo { get; set; } = false;
        public bool SingleRun { get; set; } = false;
        public int ExecuteInterval { get; set; } = 100;
        // 可选：覆盖该相机对应的 VM 流程名（留空则用 VMProcedurePrefix + (Index+1)）
        public string VMProcedureName { get; set; } = "";
    }

    public class BrightnessStage
    {
        public string StageName { get; set; } = "";
        public int BackLight { get; set; } = 0;
        public int RingLight { get; set; } = 0;
        public int TopLight { get; set; } = 0;
    }

    public class UserAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; } // 0=操作员, 1=工程师, 2=管理员
    }
}
