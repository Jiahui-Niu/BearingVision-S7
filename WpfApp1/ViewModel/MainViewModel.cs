using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Omron;
using FinsStcp = HslCommunication.Profinet.Omron.OmronFinsNet;
using WpfApp1.Model;
using ICPlatformTools;

// VM SDK — 需安装 VisionMaster 4.4.3
using VM.Core;
using GlobalVariableModuleCs;   // IMVSGlobalVariableModuCs
using ImageCollectModuCs;        // IImageCollectModuCs

namespace WpfApp1.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private AppConfig _config;
        private bool _isRunning;
        private bool _isConnected;
        private string _statusText = "就绪";
        private string _modelNo;
        private string _editBatchNo;
        private string _currentBatchNo;
        private int _displayMode = 0;
        private int _currentUserRole = -1;
        private string _currentUserName = "";

        private Thread _plcPollingThread;
        private SiemensS7Net _s7Client;
        private OmronFinsNet _finsClient;
        private bool _plcConnected;

        private readonly object _detectLock = new object();
        private bool _isDetecting;

        // Disk/CPU monitoring
        private string _diskInfo = "";
        private string _memInfo = "";
        private string _cpuInfo = "";
        private string _diskUsage = "";
        private Timer _sysInfoTimer;

        #endregion

        #region Observable Collections

        public ObservableCollection<MainImageShowViewModel> Cameras { get; } = new ObservableCollection<MainImageShowViewModel>();

        #endregion

        #region Sub ViewModels

        public CheckResultViewModel ResultVM { get; } = new CheckResultViewModel();
        public BrightnessViewModel BrightnessVM { get; } = new BrightnessViewModel();
        public CheckSetViewModel CheckSetVM { get; } = new CheckSetViewModel();
        public TCPClient TCPClientVM { get; } = new TCPClient();

        #endregion

        #region Properties

        public AppConfig Config => _config;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                SetField(ref _isRunning, value);
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetField(ref _isConnected, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetField(ref _statusText, value);
        }

        public string ModelNo
        {
            get => _modelNo;
            set { SetField(ref _modelNo, value); _config.ModelNo = value; }
        }

        public string EditBatchNo
        {
            get => _editBatchNo;
            set { SetField(ref _editBatchNo, value); _config.EditBatchNo = value; }
        }

        public string CurrentBatchNo
        {
            get => _currentBatchNo;
            set { SetField(ref _currentBatchNo, value); _config.CurrentBatchNo = value; }
        }

        public int DisplayMode
        {
            get => _displayMode;
            set => SetField(ref _displayMode, value);
        }

        public bool CanStart => !_isRunning;
        public bool CanStop => _isRunning;

        public string SolutionPath
        {
            get => _config?.SolutionPath ?? "";
            set { _config.SolutionPath = value; OnPropertyChanged(); }
        }

        public bool HasEnclosure
        {
            get => _config?.HasEnclosure ?? false;
            set { _config.HasEnclosure = value; OnPropertyChanged(); }
        }

        public bool[] CameraEnable => _config?.CameraEnable ?? new bool[6];

        public string DiskInfo { get => _diskInfo; set => SetField(ref _diskInfo, value); }
        public string MemInfo { get => _memInfo; set => SetField(ref _memInfo, value); }
        public string CpuInfo { get => _cpuInfo; set => SetField(ref _cpuInfo, value); }
        public string DiskUsage { get => _diskUsage; set => SetField(ref _diskUsage, value); }

        public int CurrentUserRole { get => _currentUserRole; set => SetField(ref _currentUserRole, value); }
        public string CurrentUserName { get => _currentUserName; set => SetField(ref _currentUserName, value); }

        #endregion

        #region Commands

        public RelayCommand StartCommand => new RelayCommand(Start, () => CanStart);
        public RelayCommand StopCommand => new RelayCommand(Stop, () => CanStop);
        public RelayCommand ResetCountCommand => new RelayCommand(ResetCount);
        public RelayCommand SaveConfigCommand => new RelayCommand(SaveConfig);
        public RelayCommand OpenSavePathCommand => new RelayCommand(OpenSavePath);
        public RelayCommand<int> ManualTriggerCommand => new RelayCommand<int>(ManualTrigger);
        public RelayCommand ConfirmBatchCommand => new RelayCommand(ConfirmBatch);

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _config = AppConfig.Load();

            for (int i = 0; i < 6; i++)
            {
                var cam = new MainImageShowViewModel { CameraIndex = i };
                if (_config.Cameras.Count > i)
                    cam.StationName = _config.Cameras[i].StationName;
                else
                    cam.StationName = $"相机{i + 1}";
                Cameras.Add(cam);
            }

            BrightnessVM.LoadFromConfig(_config);
            CheckSetVM.LoadFromConfig(_config);
            TCPClientVM.LoadFromConfig(_config);

            ModelNo = _config.ModelNo;
            EditBatchNo = _config.EditBatchNo;
            CurrentBatchNo = _config.CurrentBatchNo;

            _sysInfoTimer = new Timer(UpdateSysInfo, null, 1000, 3000);
        }

        #endregion

        #region VM SDK Integration

        private IMVSolution _vmSolution;
        private bool _vmLoaded;

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private bool LoadVMSolution(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    LogHelper.Log.Error($"VM方案文件不存在: {path}");
                    return false;
                }

                if (_vmSolution != null)
                {
                    try { _vmSolution.DestroyAllModule(); } catch { }
                    _vmSolution = null;
                }

                _vmSolution = MVisionMaster.Instance.Solution;
                _vmSolution.Load(path);
                _vmLoaded = true;
                LogHelper.Log.Info($"VM方案加载成功: {path}");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("VM方案加载失败", ex);
                _vmLoaded = false;
                return false;
            }
        }

        private void StopVMSolution()
        {
            try { _vmSolution?.StopAllModules(); }
            catch (Exception ex) { LogHelper.Log.Error("VM方案停止失败", ex); }
        }

        /// <summary>
        /// 获取该相机对应的 VM 流程名
        /// 规则：CameraConfig.VMProcedureName 不为空时优先使用，否则用 prefix + (index+1)
        /// </summary>
        private string GetVMProcedureName(int camIndex)
        {
            var camCfg = _config.Cameras[camIndex];
            return !string.IsNullOrEmpty(camCfg.VMProcedureName)
                ? camCfg.VMProcedureName
                : $"{_config.VMProcedurePrefix}{camIndex + 1}";
        }

        /// <summary>
        /// 触发 VM 流程并等待检测结果
        /// 返回 true=OK，false=NG
        /// </summary>
        private bool TriggerVMCamera(int camIndex, out string defectInfo)
        {
            defectInfo = "";
            if (!_vmLoaded || _vmSolution == null) return true;

            try
            {
                var procName = GetVMProcedureName(camIndex);
                var proc = _vmSolution.GetProcedure(procName);
                if (proc == null)
                {
                    LogHelper.Log.Warn($"VM流程不存在: {procName}，默认OK");
                    return true;
                }

                // 同步执行，等待流程完成
                // WaitRun 返回 0 表示成功，非 0 为错误码
                int errCode = proc.WaitRun(_config.VMRunTimeout);
                if (errCode != 0)
                    LogHelper.Log.Warn($"VM流程[{procName}]返回错误码: {errCode}");

                // 读取全局变量模块
                // 模块名称需与 VM 方案编辑器中"全局变量"模块的【名称】一致
                var gvModName = _config.VMGlobalVarModuleName;
                var gvMod = proc.GetModule(gvModName) as IMVSGlobalVariableModuCs;
                if (gvMod == null)
                {
                    LogHelper.Log.Warn($"找不到全局变量模块[{gvModName}]，默认OK");
                    return true;
                }

                // 读取检测结果：1=OK，2=NG（与 VM 方案中变量定义一致）
                int result = gvMod.GetIntValue(_config.VMResultVarName);
                try { defectInfo = gvMod.GetStringValue(_config.VMDefectVarName); }
                catch { defectInfo = ""; }

                return result == 1;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error($"VM检测触发失败 Cam{camIndex + 1}", ex);
                defectInfo = "VM检测异常";
                return false;
            }
        }

        /// <summary>
        /// 从 VM 图像采集模块获取最近一帧图像并转为 BitmapSource（用于 UI 显示）
        /// 若无法获取则返回 null
        /// </summary>
        private BitmapSource GetVMCameraImage(int camIndex)
        {
            if (!_vmLoaded || _vmSolution == null) return null;
            try
            {
                var procName = GetVMProcedureName(camIndex);
                var proc = _vmSolution.GetProcedure(procName);
                if (proc == null) return null;

                // 模块名称需与 VM 方案中图像采集模块的【名称】一致
                var imgModName = _config.VMImageModuleName;
                var imgMod = proc.GetModule(imgModName) as IImageCollectModuCs;
                if (imgMod == null) return null;

                // VM 4.4.3 SDK：从图像采集模块取输出图像并转换为 Bitmap
                // GetOutputImage() 返回 VM 内部图像对象，ToBitmap() 转为 GDI+ Bitmap
                System.Drawing.Bitmap bmp = imgMod.GetOutputImage()?.ToBitmap();
                if (bmp == null) return null;

                return BitmapToBitmapSource(bmp);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error($"获取VM图像失败 Cam{camIndex + 1}", ex);
                return null;
            }
        }

        /// <summary>
        /// GDI+ Bitmap → WPF BitmapSource 转换（via HBitmap，无需 MemoryStream）
        /// </summary>
        private BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
                bitmap.Dispose();
            }
        }

        #endregion

        #region PLC Communication

        private bool ConnectPLC()
        {
            try
            {
                DisconnectPLC();
                var cfg = _config;
                if (cfg.PLCType == "S7")
                {
                    _s7Client = new SiemensS7Net(SiemensPLCS.S1200, cfg.PLCIp);
                    _s7Client.Port = cfg.PLCPort;
                    var r = _s7Client.ConnectServer();
                    _plcConnected = r.IsSuccess;
                    if (!r.IsSuccess) LogHelper.Log.Error($"S7 PLC连接失败: {r.Message}");
                }
                else if (cfg.PLCType == "Fins")
                {
                    _finsClient = new OmronFinsNet(cfg.PLCIp);
                    _finsClient.Port = cfg.PLCPort;
                    var r = _finsClient.ConnectServer();
                    _plcConnected = r.IsSuccess;
                    if (!r.IsSuccess) LogHelper.Log.Error($"Fins PLC连接失败: {r.Message}");
                }
                return _plcConnected;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("PLC连接异常", ex);
                _plcConnected = false;
                return false;
            }
        }

        private void DisconnectPLC()
        {
            try { _s7Client?.ConnectClose(); _s7Client = null; } catch { }
            try { _finsClient?.ConnectClose(); _finsClient = null; } catch { }
            _plcConnected = false;
        }

        private bool ReadBool(string addr)
        {
            try
            {
                if (_s7Client != null) { var r = _s7Client.ReadBool(addr); return r.IsSuccess && r.Content; }
                if (_finsClient != null) { var r = _finsClient.ReadBool(addr); return r.IsSuccess && r.Content; }
            }
            catch { }
            return false;
        }

        private void WriteBool(string addr, bool value)
        {
            try
            {
                if (_s7Client != null) _s7Client.Write(addr, value);
                else if (_finsClient != null) _finsClient.Write(addr, value);
            }
            catch (Exception ex) { LogHelper.Log.Error($"写PLC失败 {addr}", ex); }
        }

        private void WriteShort(string addr, short value)
        {
            try
            {
                if (_s7Client != null) _s7Client.Write(addr, value);
                else if (_finsClient != null) _finsClient.Write(addr, value);
            }
            catch (Exception ex) { LogHelper.Log.Error($"写PLC失败 {addr}", ex); }
        }

        private void UpdatePLCStats()
        {
            if (!_plcConnected) return;
            try
            {
                if (!string.IsNullOrEmpty(_config.TotalCountAddr))
                    WriteShort(_config.TotalCountAddr, (short)ResultVM.TotalCount);
                if (!string.IsNullOrEmpty(_config.OKCountAddr))
                    WriteShort(_config.OKCountAddr, (short)ResultVM.OKCount);
                if (!string.IsNullOrEmpty(_config.NGCountAddr))
                    WriteShort(_config.NGCountAddr, (short)ResultVM.NGCount);
            }
            catch { }
        }

        #endregion

        #region Detection Control

        public void Start()
        {
            if (_isRunning) return;

            StatusText = "正在启动...";

            // Connect PLC
            if (!ConnectPLC())
            {
                StatusText = "PLC连接失败，请检查网络配置";
                return;
            }

            // Load VM solution
            if (!string.IsNullOrEmpty(_config.SolutionPath))
            {
                if (!LoadVMSolution(_config.SolutionPath))
                {
                    StatusText = "VM方案加载失败，请检查方案路径";
                    DisconnectPLC();
                    return;
                }
            }

            IsRunning = true;
            IsConnected = true;
            StatusText = "运行中";
            CurrentBatchNo = EditBatchNo;

            _plcPollingThread = new Thread(PLCPollingLoop) { IsBackground = true, Name = "PLCPolling" };
            _plcPollingThread.Start();

            LogHelper.Log.Info($"检测启动 型号:{ModelNo} 批次:{CurrentBatchNo}");
        }

        public void Stop()
        {
            if (!_isRunning) return;
            IsRunning = false;
            IsConnected = false;
            StatusText = "停止中...";

            _plcPollingThread?.Join(3000);
            _plcPollingThread = null;

            StopVMSolution();
            DisconnectPLC();
            StatusText = "已停止";
            LogHelper.Log.Info("检测停止");
        }

        private void PLCPollingLoop()
        {
            bool[] prevFirstSignal = new bool[6];
            bool plcReconnecting = false;

            while (_isRunning)
            {
                try
                {
                    if (!_plcConnected)
                    {
                        if (!plcReconnecting)
                        {
                            plcReconnecting = true;
                            LogHelper.Log.Warn("PLC断线，尝试重连...");
                        }
                        Thread.Sleep(2000);
                        ConnectPLC();
                        continue;
                    }
                    plcReconnecting = false;

                    for (int i = 0; i < 6; i++)
                    {
                        if (!_isRunning) break;
                        if (!_config.CameraEnable[i]) continue;
                        if (_config.Cameras.Count <= i) continue;
                        var camCfg = _config.Cameras[i];
                        if (!camCfg.IsOnline) continue;

                        // 检测First信号上升沿
                        bool firstNow = ReadBool(camCfg.FirstAddr);
                        if (firstNow && !prevFirstSignal[i])
                        {
                            // 防止重入
                            if (_isDetecting) { prevFirstSignal[i] = firstNow; continue; }
                            _isDetecting = true;

                            // 回写Start信号
                            WriteBool(camCfg.StartAddr, true);
                            Application.Current?.Dispatcher.Invoke(() => Cameras[i].IsDetecting = true);

                            bool isOk = TriggerVMCamera(i, out string defectInfo);

                            // 延时读取（等待图像采集）
                            if (camCfg.ShotInterval > 0)
                                Thread.Sleep(camCfg.ShotInterval);

                            // 写检测结果到PLC
                            WriteShort(camCfg.ResultAddr, isOk ? (short)1 : (short)2);

                            // 取 VM 图像（TriggerVMCamera 执行后立即取，此时 VM 模块输出有效）
                            BitmapSource displayImg = GetVMCameraImage(i);

                            // 更新UI
                            Cameras[i].SetResult(isOk, displayImg, "", defectInfo);
                            ResultVM.AddResult(isOk);
                            UpdatePLCStats();

                            // 存图
                            if ((isOk && _config.SaveOK) || (!isOk && _config.SaveNG))
                                SaveImage(i, isOk, defectInfo);

                            Application.Current?.Dispatcher.Invoke(() => Cameras[i].IsDetecting = false);
                            _isDetecting = false;
                        }
                        prevFirstSignal[i] = firstNow;
                    }

                    Thread.Sleep(5);
                }
                catch (ThreadAbortException) { break; }
                catch (Exception ex)
                {
                    LogHelper.Log.Error("PLC轮询异常", ex);
                    _plcConnected = false;
                    Thread.Sleep(1000);
                }
            }
        }

        public void ManualTrigger(int camIndex)
        {
            if (!_isRunning || _isDetecting) return;
            if (camIndex < 0 || camIndex >= 6) return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                _isDetecting = true;
                Application.Current?.Dispatcher.Invoke(() => Cameras[camIndex].IsDetecting = true);
                bool isOk = TriggerVMCamera(camIndex, out string defectInfo);
                BitmapSource img = GetVMCameraImage(camIndex);
                Cameras[camIndex].SetResult(isOk, img, "", defectInfo);
                Application.Current?.Dispatcher.Invoke(() => Cameras[camIndex].IsDetecting = false);
                _isDetecting = false;
            });
        }

        public void ResetCount()
        {
            ResultVM.Reset();
            foreach (var cam in Cameras) cam.Reset();
            if (_plcConnected) UpdatePLCStats();
        }

        #endregion

        #region Image Save

        private void SaveImage(int camIndex, bool isOk, string defectInfo)
        {
            try
            {
                var basePath = _config.SaveImagePath;
                var date = DateTime.Now.ToString("yyyyMMdd");
                var resultFolder = isOk ? "OK" : "NG";
                var dir = Path.Combine(basePath, date, $"Cam{camIndex + 1}", resultFolder);
                Directory.CreateDirectory(dir);

                var fileName = $"{DateTime.Now:HHmmss_fff}_{_config.ModelNo}.jpg";
                var fullPath = Path.Combine(dir, fileName);

                // 从 VM 取图像（此处直接取 GDI+ Bitmap 存盘，避免二次转换）
                var bmp = GetVMCameraImageBitmap(camIndex);
                if (bmp != null)
                {
                    using (bmp)
                        bmp.Save(fullPath, ImageFormat.Jpeg);
                }

                CleanOldImages(basePath, _config.SaveDays);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error($"图像保存失败 Cam{camIndex + 1}", ex);
            }
        }

        /// <summary>
        /// 从 VM 图像采集模块获取 GDI+ Bitmap（供存盘使用，调用方负责 Dispose）
        /// </summary>
        private System.Drawing.Bitmap GetVMCameraImageBitmap(int camIndex)
        {
            if (!_vmLoaded || _vmSolution == null) return null;
            try
            {
                var proc = _vmSolution.GetProcedure(GetVMProcedureName(camIndex));
                if (proc == null) return null;

                var imgMod = proc.GetModule(_config.VMImageModuleName) as IImageCollectModuCs;
                return imgMod?.GetOutputImage()?.ToBitmap();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error($"获取VM存盘图像失败 Cam{camIndex + 1}", ex);
                return null;
            }
        }

        private void CleanOldImages(string basePath, int keepDays)
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-keepDays);
                var dirs = Directory.GetDirectories(basePath);
                foreach (var dir in dirs)
                {
                    var dirName = Path.GetFileName(dir);
                    if (DateTime.TryParseExact(dirName, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dirDate))
                    {
                        if (dirDate < cutoff)
                            Directory.Delete(dir, true);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Config

        public void SaveConfig()
        {
            BrightnessVM.SaveToConfig(_config);
            CheckSetVM.SaveToConfig(_config);
            TCPClientVM.SaveToConfig(_config);

            // 更新相机工位名称到Cameras ViewModel
            for (int i = 0; i < 6 && i < _config.Cameras.Count; i++)
                Cameras[i].StationName = _config.Cameras[i].StationName;

            _config.ModelNo = ModelNo;
            _config.EditBatchNo = EditBatchNo;
            _config.Save();
            StatusText = "配置已保存";
            LogHelper.Log.Info("配置保存成功");
        }

        private void OpenSavePath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = _config.SaveImagePath;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _config.SaveImagePath = dialog.SelectedPath;
                CheckSetVM.SaveImagePath = dialog.SelectedPath;
            }
        }

        public void SelectSolutionPath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "VM方案文件|*.sol",
                InitialDirectory = Path.GetDirectoryName(_config.SolutionPath) ?? @"D:\"
            };
            if (dialog.ShowDialog() == true)
            {
                SolutionPath = dialog.FileName;
            }
        }

        public void ConfirmBatch()
        {
            CurrentBatchNo = EditBatchNo;
            _config.CurrentBatchNo = CurrentBatchNo;
            _config.Save();
        }

        #endregion

        #region System Info

        private void UpdateSysInfo(object state)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory));
                long free = drive.AvailableFreeSpace;
                long total = drive.TotalSize;
                DiskInfo = $"{free / 1073741824.0:F2} GB / {total / 1073741824.0:F2} GB ({(1.0 - (double)free / total) * 100:F1}%)";
                DiskUsage = $"{(1.0 - (double)free / total) * 100:F1}%";

                using (var pc = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    pc.NextValue();
                    Thread.Sleep(100);
                    CpuInfo = $"{pc.NextValue():F0}%";
                }

                var mem = GC.GetTotalMemory(false);
                MemInfo = $"{mem / 1048576.0:F0} MB";
            }
            catch { }
        }

        #endregion

        public void Cleanup()
        {
            _sysInfoTimer?.Dispose();
            Stop();
        }
    }
}
