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
        private string _simulationImageFolder;
        private int _simulationIntervalMs = 3000;

        public bool SimulationMode { get => _simulationMode; set => SetField(ref _simulationMode, value); }
        public string SimulationImageFolder { get => _simulationImageFolder; set => SetField(ref _simulationImageFolder, value); }
        public int SimulationIntervalMs { get => _simulationIntervalMs; set => SetField(ref _simulationIntervalMs, value); }

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
            SimulationImageFolder = config.SimulationImageFolder;
            SimulationIntervalMs = config.SimulationIntervalMs;

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
                    ExecuteInterval = c.ExecuteInterval
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
            config.SimulationImageFolder = SimulationImageFolder;
            config.SimulationIntervalMs = SimulationIntervalMs;

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
                    ExecuteInterval = vm.ExecuteInterval
                });
            }
        }
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
    }
}
