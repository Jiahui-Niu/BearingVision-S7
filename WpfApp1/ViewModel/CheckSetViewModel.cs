using System.Collections.ObjectModel;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class CheckSetViewModel : ViewModelBase
    {
        private ObservableCollection<CameraConfigViewModel> _cameras = new ObservableCollection<CameraConfigViewModel>();

        private string _totalCountAddr;
        private string _okCountAddr;
        private string _ngCountAddr;
        private string _startAddr;
        private string _clearAddr;
        private string _dataType;

        private string _saveImagePath;
        private int _saveDays;
        private bool _saveOK;
        private bool _saveNG;
        private bool _usePhotometricStereo;

        public ObservableCollection<CameraConfigViewModel> Cameras
        {
            get => _cameras;
            set => SetField(ref _cameras, value);
        }

        public string TotalCountAddr { get => _totalCountAddr; set => SetField(ref _totalCountAddr, value); }
        public string OKCountAddr { get => _okCountAddr; set => SetField(ref _okCountAddr, value); }
        public string NGCountAddr { get => _ngCountAddr; set => SetField(ref _ngCountAddr, value); }
        public string StartAddr { get => _startAddr; set => SetField(ref _startAddr, value); }
        public string ClearAddr { get => _clearAddr; set => SetField(ref _clearAddr, value); }
        public string DataType { get => _dataType; set => SetField(ref _dataType, value); }

        public string SaveImagePath { get => _saveImagePath; set => SetField(ref _saveImagePath, value); }
        public int SaveDays { get => _saveDays; set => SetField(ref _saveDays, value); }
        public bool SaveOK { get => _saveOK; set => SetField(ref _saveOK, value); }
        public bool SaveNG { get => _saveNG; set => SetField(ref _saveNG, value); }
        public bool UsePhotometricStereo { get => _usePhotometricStereo; set => SetField(ref _usePhotometricStereo, value); }

        private bool _simulationMode;
        private int _simulationIntervalMs = 3000;

        public bool SimulationMode { get => _simulationMode; set => SetField(ref _simulationMode, value); }
        public int SimulationIntervalMs { get => _simulationIntervalMs; set => SetField(ref _simulationIntervalMs, value); }
        public ObservableCollection<SimCameraFolderViewModel> SimFolders { get; } = new ObservableCollection<SimCameraFolderViewModel>();

        private string _vmProcedurePrefix;
        private string _vmGlobalVarModuleName;
        private string _vmResultVarName;
        private string _vmDefectVarName;
        private string _vmImageModuleName;
        private int _vmRunTimeout;

        /// <summary>VM流程名前缀，实际流程名 = 前缀 + (相机序号+1)，单个相机可在下方列表单独覆盖</summary>
        public string VMProcedurePrefix { get => _vmProcedurePrefix; set => SetField(ref _vmProcedurePrefix, value); }
        /// <summary>VM方案中全局变量模块的名称，需与VM方案编辑器中一致</summary>
        public string VMGlobalVarModuleName { get => _vmGlobalVarModuleName; set => SetField(ref _vmGlobalVarModuleName, value); }
        /// <summary>检测结果变量名(1=OK, 2=NG)</summary>
        public string VMResultVarName { get => _vmResultVarName; set => SetField(ref _vmResultVarName, value); }
        /// <summary>缺陷描述变量名</summary>
        public string VMDefectVarName { get => _vmDefectVarName; set => SetField(ref _vmDefectVarName, value); }
        /// <summary>图像采集模块名</summary>
        public string VMImageModuleName { get => _vmImageModuleName; set => SetField(ref _vmImageModuleName, value); }
        /// <summary>等待VM流程执行完成的超时时间(ms)</summary>
        public int VMRunTimeout { get => _vmRunTimeout; set => SetField(ref _vmRunTimeout, value); }

        public void LoadFromConfig(AppConfig config)
        {
            TotalCountAddr = config.TotalCountAddr;
            OKCountAddr = config.OKCountAddr;
            NGCountAddr = config.NGCountAddr;
            StartAddr = config.StartAddr;
            ClearAddr = config.ClearAddr;
            DataType = config.DataType;

            SaveImagePath = config.SaveImagePath;
            SaveDays = config.SaveDays;
            SaveOK = config.SaveOK;
            SaveNG = config.SaveNG;
            UsePhotometricStereo = config.UsePhotometricStereo;

            SimulationMode = config.SimulationMode;
            SimulationIntervalMs = config.SimulationIntervalMs;
            SimFolders.Clear();
            if (config.SimulationImageFolders == null || config.SimulationImageFolders.Length < 6)
                config.SimulationImageFolders = new string[6];
            for (int i = 0; i < 6; i++)
                SimFolders.Add(new SimCameraFolderViewModel { CameraName = $"Cam{i + 1}", Folder = config.SimulationImageFolders[i] ?? "" });

            VMProcedurePrefix = config.VMProcedurePrefix;
            VMGlobalVarModuleName = config.VMGlobalVarModuleName;
            VMResultVarName = config.VMResultVarName;
            VMDefectVarName = config.VMDefectVarName;
            VMImageModuleName = config.VMImageModuleName;
            VMRunTimeout = config.VMRunTimeout;

            _cameras.Clear();
            foreach (var c in config.Cameras)
            {
                _cameras.Add(new CameraConfigViewModel
                {
                    Index = c.Index,
                    StationName = c.StationName,
                    FirstAddr = c.FirstAddr,
                    StartAddr = c.StartAddr,
                    ResultAddr = c.ResultAddr,
                    DelayShots = c.DelayShots,
                    TotalShots = c.TotalShots,
                    ShotInterval = c.ShotInterval,
                    IsOnline = c.IsOnline,
                    EndFlag = c.EndFlag,
                    IsRotation = c.IsRotation,
                    UsePhotometricStereo = c.UsePhotometricStereo,
                    SingleRun = c.SingleRun,
                    ExecuteInterval = c.ExecuteInterval,
                    VMProcedureName = c.VMProcedureName
                });
            }
        }

        public void SaveToConfig(AppConfig config)
        {
            config.TotalCountAddr = TotalCountAddr;
            config.OKCountAddr = OKCountAddr;
            config.NGCountAddr = NGCountAddr;
            config.StartAddr = StartAddr;
            config.ClearAddr = ClearAddr;
            config.DataType = DataType;

            config.SaveImagePath = SaveImagePath;
            config.SaveDays = SaveDays;
            config.SaveOK = SaveOK;
            config.SaveNG = SaveNG;
            config.UsePhotometricStereo = UsePhotometricStereo;

            config.SimulationMode = SimulationMode;
            config.SimulationIntervalMs = SimulationIntervalMs;
            config.SimulationImageFolders = new string[6];
            for (int i = 0; i < SimFolders.Count && i < 6; i++)
                config.SimulationImageFolders[i] = SimFolders[i].Folder ?? "";

            config.VMProcedurePrefix = VMProcedurePrefix;
            config.VMGlobalVarModuleName = VMGlobalVarModuleName;
            config.VMResultVarName = VMResultVarName;
            config.VMDefectVarName = VMDefectVarName;
            config.VMImageModuleName = VMImageModuleName;
            config.VMRunTimeout = VMRunTimeout;

            config.Cameras.Clear();
            foreach (var vm in _cameras)
            {
                config.Cameras.Add(new CameraConfig
                {
                    Index = vm.Index,
                    StationName = vm.StationName,
                    FirstAddr = vm.FirstAddr,
                    StartAddr = vm.StartAddr,
                    ResultAddr = vm.ResultAddr,
                    DelayShots = vm.DelayShots,
                    TotalShots = vm.TotalShots,
                    ShotInterval = vm.ShotInterval,
                    IsOnline = vm.IsOnline,
                    EndFlag = vm.EndFlag,
                    IsRotation = vm.IsRotation,
                    UsePhotometricStereo = vm.UsePhotometricStereo,
                    SingleRun = vm.SingleRun,
                    ExecuteInterval = vm.ExecuteInterval,
                    VMProcedureName = vm.VMProcedureName
                });
            }
        }
    }

    public class SimCameraFolderViewModel : ViewModelBase
    {
        public string CameraName { get; set; }
        private string _folder;
        public string Folder { get => _folder; set => SetField(ref _folder, value); }
    }

    public class CameraConfigViewModel : ViewModelBase
    {
        private int _index;
        private string _stationName;
        private string _firstAddr;
        private string _startAddr;
        private string _resultAddr;
        private int _delayShots;
        private int _totalShots;
        private int _shotInterval;
        private bool _isOnline;
        private bool _endFlag;
        private bool _isRotation;
        private bool _usePhotometricStereo;
        private bool _singleRun;
        private int _executeInterval;

        public int Index { get => _index; set => SetField(ref _index, value); }
        public string StationName { get => _stationName; set => SetField(ref _stationName, value); }
        public string FirstAddr { get => _firstAddr; set => SetField(ref _firstAddr, value); }
        public string StartAddr { get => _startAddr; set => SetField(ref _startAddr, value); }
        public string ResultAddr { get => _resultAddr; set => SetField(ref _resultAddr, value); }
        public int DelayShots { get => _delayShots; set => SetField(ref _delayShots, value); }
        public int TotalShots { get => _totalShots; set => SetField(ref _totalShots, value); }
        public int ShotInterval { get => _shotInterval; set => SetField(ref _shotInterval, value); }
        public bool IsOnline { get => _isOnline; set => SetField(ref _isOnline, value); }
        public bool EndFlag { get => _endFlag; set => SetField(ref _endFlag, value); }
        public bool IsRotation { get => _isRotation; set => SetField(ref _isRotation, value); }
        public bool UsePhotometricStereo { get => _usePhotometricStereo; set => SetField(ref _usePhotometricStereo, value); }
        public bool SingleRun { get => _singleRun; set => SetField(ref _singleRun, value); }
        public int ExecuteInterval { get => _executeInterval; set => SetField(ref _executeInterval, value); }

        public string DisplayName => $"Cam{_index + 1}";

        private string _vmProcedureName = "";
        /// <summary>覆盖该相机对应的VM流程名，留空则用VMProcedurePrefix + (Index+1)</summary>
        public string VMProcedureName { get => _vmProcedureName; set => SetField(ref _vmProcedureName, value); }
    }
}
