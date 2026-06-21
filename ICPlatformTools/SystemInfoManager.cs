using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using OpenHardwareMonitor.Hardware;


namespace ICPlatformTools
{
    public class SystemInfoManager
    {
        private Computer computer = new Computer();
        private UpdateVisitor visitor = new UpdateVisitor();
        private ISensor CpuTotalSensor;
        private ISensor CpuAppSensor;
        private ISensor MemoryUsedSensor;
        private ISensor AppMemoryUsedSensor;
        private static SystemInfoManager sysM;
        private static readonly object lockRoot = new object();
        private bool isStart = true;
        private Thread m_thread;
        private SystemInfo sysInfo = new SystemInfo();
        private bool isDone = false;
        // 设置构造方法为私有，这样就不能在外部实例化类对象了
        private SystemInfoManager()
        {
            try
            {
                computer.CPUEnabled = true;
                computer.RAMEnabled = true;
                computer.HardwareAdded += computer_HardwareAdded;
                computer.Open();
                computer.Accept(visitor);
                isStart = true;
                m_thread = new Thread(UpdatePCStatus);
                m_thread.IsBackground = true;
                m_thread.Start();

                Task.Run(() =>
                {
                    try
                    {
                        LogHelper.Log.Debug(computer.GetReport());
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
		}

        /// <summary>
        /// 硬件添加
        /// </summary>
        /// <param name="hardware"></param>
        private void computer_HardwareAdded(IHardware hardware)
        {
            try
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (hardware.HardwareType == HardwareType.CPU)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                        {
                            CpuTotalSensor = sensor;
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU APP")
                        {
                            CpuAppSensor = sensor;
                        }
                    }

                    if (hardware.HardwareType == HardwareType.RAM)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
                        {
                            MemoryUsedSensor = sensor;
                        }

                        if (sensor.SensorType == SensorType.Load && sensor.Name == "App used memory")
                        {
                            AppMemoryUsedSensor = sensor;
                        }
                    }
                }
            }
            catch { }
        }

        private string GetUnit(ISensor sensor)
        {
            string fixedFormat;

            switch (sensor.SensorType)
            {
            case SensorType.Voltage: fixedFormat = "{0:F3} V"; break;
            case SensorType.Clock: fixedFormat = "{0:F1} MHz"; break;
            case SensorType.Load: fixedFormat = "{0:F1} %"; break;
            case SensorType.Fan: fixedFormat = "{0:F0} RPM"; break;
            case SensorType.Flow: fixedFormat = "{0:F0} L/h"; break;
            case SensorType.Control: fixedFormat = "{0:F1} %"; break;
            case SensorType.Level: fixedFormat = "{0:F1} %"; break;
            case SensorType.Power: fixedFormat = "{0:F1} W"; break;
            case SensorType.Data: fixedFormat = "{0:F1} GB"; break;
            case SensorType.SmallData: fixedFormat = "{0:F1} MB"; break;
            case SensorType.Factor: fixedFormat = "{0:F3}"; break;
            case SensorType.Temperature: fixedFormat = "{0:F1} °C"; break;
            default: fixedFormat = "{0}"; break;
            }

            return fixedFormat;
        }

        /// <summary>
        /// 双检锁单例
        /// </summary>
        /// <returns></returns>
        public static SystemInfoManager GetSingleInstance()
        {
            if (sysM == null)
            {
				lock (lockRoot) 
                {
                    if (sysM == null)
                    {
                        sysM = new SystemInfoManager();
					}
				}
			}
            return sysM;
        }

        public SystemInfo CurrentSystemInfo
        {
            get 
            {
                if (isDone)
                {
                    return sysInfo; 
                }
                else
                {
                    return new SystemInfo();
                }
            }
        }

        /// <summary>
        /// 更新PC状态信息
        /// </summary>
        private void UpdatePCStatus()
        {
            const int MonitorInterval = 1000;
            Stopwatch watch = Stopwatch.StartNew();

            while (isStart)
            {
                if (watch.ElapsedMilliseconds < MonitorInterval)
                {
                    Thread.Sleep(200);
                    continue;
                }
                watch.Restart();

                try
                {
                    //get cpu usage
                    computer.Accept(visitor);

                    if (CpuTotalSensor != null)
                    {
                        sysInfo.cpuRate = CpuTotalSensor.Value.HasValue ? CpuTotalSensor.Value.Value : 0;
                        if (sysInfo.cpuRate > 70)
                        {
                            LogHelper.Log.InfoFormat("CPU占用率：{0}", sysInfo.cpuRate.ToString("0.0") + "%");
                        }
                    }

                    if (CpuAppSensor != null)
                    {
                        sysInfo.appCpuRate = CpuAppSensor.Value.HasValue ? CpuAppSensor.Value.Value : 0;
                        if (sysInfo.appCpuRate > 60)
                        {
                            LogHelper.Log.InfoFormat("物流平台CPU占用率：{0}", sysInfo.appCpuRate.ToString("0.0") + "%");
                        }
                    }

                    if (MemoryUsedSensor != null)
                    {
                        sysInfo.memRate = MemoryUsedSensor.Value.HasValue ? MemoryUsedSensor.Value.Value : 0;

                        if (AppMemoryUsedSensor != null)
                        {
                            sysInfo.appMemRate = AppMemoryUsedSensor.Value.HasValue ? AppMemoryUsedSensor.Value.Value : 0;
                        }

                        if (sysInfo.memRate > 80)
                        {
                            LogHelper.Log.InfoFormat("内存占用率：{0}, 物流平台内存占用: {1}"
                                , sysInfo.memRate.ToString("0.0") + "%"
                                , sysInfo.appMemRate.ToString("0.0") + "%");
                        }
                    }

                    DriveInfo[] allDirves = DriveInfo.GetDrives();

                    Monitor.Enter(SystemInfo.locklist); // 锁住 divideDiskInfo

                    sysInfo.divideDiskInfo.Clear();
                    foreach (DriveInfo item in allDirves)
                    {
                        //Fixed 硬盘
                        //Removable 可移动存储设备，如软盘驱动器或USB闪存驱动器。
                        if (item.DriveType.Equals(DriveType.Fixed))
                        {
                            if (item.IsReady)
                            {
                                isDone = false;
                                DiskInfo temp = new DiskInfo();
                                temp.name = item.Name.Substring(0, 1);
                                temp.totalSpace = item.TotalSize;
                                temp.FreeSpace = item.TotalFreeSpace;
                                temp.rate = (1.0 - (double)temp.FreeSpace / temp.totalSpace) * 100;
                                sysInfo.divideDiskInfo.Add(temp);
                            }
                            else
                            {
                                //Console.Write("没有就绪");
                            }
                        }
                    }

                    Monitor.Exit(SystemInfo.locklist); // 释放锁

                    isDone = true;
                    long allDiskSpace = 0L;
                    long allDiskFreeSpace = 0L;
                    foreach (var item in sysInfo.divideDiskInfo)
                    {
                        allDiskSpace += item.totalSpace;
                        allDiskFreeSpace += item.FreeSpace;
                    }

                    // avoid divide by zero exception
                    sysInfo.allDiskSpace = allDiskSpace == 0 ? 1 : allDiskSpace;
                    sysInfo.allDiskFreeSpace = allDiskFreeSpace;
                    sysInfo.totalDiskRate = (1.0 - (double)sysInfo.allDiskFreeSpace / sysInfo.allDiskSpace) * 100;
                }
                catch (Exception ex)
                {
                    LogHelper.Log.ErrorFormat("UpdatePCStatus异常{0}", ex);
                }
            }
        }

        public void Close()
        {
            isStart = false;
            m_thread.Join(1000);
            computer.Close();
        }

        /// <summary>
        /// 获得已使用的物理内存的大小，单位 (Byte)，如果获取失败，返回 -1.
        /// </summary>
        /// <returns></returns>
        private  long GetTotalPhysicalMemory()
        {
            long capacity = 0;
            try
            {
                foreach (ManagementObject mo1 in new ManagementClass("Win32_PhysicalMemory").GetInstances())
                    capacity += long.Parse(mo1.Properties["Capacity"].Value.ToString());
            }
            catch (Exception ex)
            {
                capacity = -1;
                Console.WriteLine(ex.Message);
            }
            return capacity;
        }

        /// <summary>
        /// 获得已使用的物理内存的大小，单位 (Byte)，如果获取失败，返回 -1.
        /// </summary>
        /// <returns></returns>
        private long GetAvailablePhysicalMemory()
        {
            long capacity = 0;
            try
            {
                foreach (ManagementObject mo1 in new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory").GetInstances())
                    capacity += long.Parse(mo1.Properties["AvailableBytes"].Value.ToString());
            }
            catch (Exception ex)
            {
                capacity = -1;
                Console.WriteLine(ex.Message);
            }
            return capacity;
        }
    }

    public class SystemInfo
    {
        public static object locklist = new object();
        public float cpuRate;
        public float appCpuRate;
        public double memRate;
        public double appMemRate;
        public long allDiskSpace;
        public long allDiskFreeSpace;
        public double totalDiskRate;
        public List<DiskInfo> divideDiskInfo = new List<DiskInfo>();
    }

    public class DiskInfo 
    {
        public string name;
        public long totalSpace;
        public long FreeSpace;
        public double rate;
    }

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }
}
